using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NetworkGameManager : NetworkBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int targetScore = 300;
    [SerializeField] private int maxRounds = 8;
    [SerializeField] private int handsPerRound = 3;
    [SerializeField] private int discardsPerRound = 3;
    [SerializeField] private int initialHandSize = 5;

    [Header("Network State")]
    public NetworkVariable<int> currentRound = new NetworkVariable<int>(1);
    public NetworkVariable<int> handsRemaining = new NetworkVariable<int>(3);
    public NetworkVariable<int> discardsRemaining = new NetworkVariable<int>(3);
    public NetworkVariable<ulong> currentTurnPlayer = new NetworkVariable<ulong>(0);
    public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false);

    private Dictionary<ulong, List<int>> playerHands = new Dictionary<ulong, List<int>>();
    public static NetworkGameManager Instance { get; private set; }

    public System.Action<int, int> OnRoundChanged;
    public System.Action<ulong> OnTurnChanged;
    public System.Action<HandResult> OnHandPlayed;
    public System.Action<ulong, bool> OnGameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnNetworkDeck();
            StartCoroutine(InitializeGameDelayed());
        }
        currentRound.OnValueChanged += OnRoundChangedCallback;
        handsRemaining.OnValueChanged += OnHandsRemainingChanged;
        currentTurnPlayer.OnValueChanged += OnTurnPlayerChanged;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        currentRound.OnValueChanged -= OnRoundChangedCallback;
        handsRemaining.OnValueChanged -= OnHandsRemainingChanged;
        currentTurnPlayer.OnValueChanged -= OnTurnPlayerChanged;

        if (Instance == this)
        {
            Instance = null;
        }

        base.OnNetworkDespawn();
    }

    private IEnumerator InitializeGameDelayed()
    {
        yield return new WaitForSeconds(1f);

        foreach (var client in NetworkManager.ConnectedClients)
        {
            playerHands[client.Key] = new List<int>();
            for (int i = 0; i < initialHandSize; i++)
            {
                DealCardToPlayer(client.Key);
            }
        }

        if (NetworkManager.ConnectedClientsIds.Count > 0)
        {
            currentTurnPlayer.Value = NetworkManager.ConnectedClientsIds[0];
        }

        gameStarted.Value = true;
        StartNewRound();
    }

    private void DealCardToPlayer(ulong clientId)
    {
        if (!IsServer) return;

        if (NetworkDeckManager.Instance == null) return;

        NetworkCardData card = NetworkDeckManager.Instance.DrawCard();

        if (!playerHands.ContainsKey(clientId))
        {
            playerHands[clientId] = new List<int>();
        }

        playerHands[clientId].Add(card.cardIndex);

        SendCardToClientClientRpc(card, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    [ClientRpc]
    private void SendCardToClientClientRpc(NetworkCardData cardData, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkHandManager.Instance != null)
        {
            NetworkHandManager.Instance.AddCardToHand(cardData);
        }
    }

    private void StartNewRound()
    {
        if (!IsServer) return;

        handsRemaining.Value = handsPerRound;
        discardsRemaining.Value = discardsPerRound;

        NotifyRoundStartClientRpc(currentRound.Value);
    }

    [ClientRpc]
    private void NotifyRoundStartClientRpc(int round)
    {
        OnRoundChanged?.Invoke(round, handsRemaining.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayHandServerRpc(int[] selectedIndices, ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        if (senderId != currentTurnPlayer.Value)
        {
            NotifyInvalidActionClientRpc("Not your turn!", new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderId }
                }
            });
            return;
        }

        if (handsRemaining.Value <= 0) return;

        if (!playerHands.ContainsKey(senderId)) return;

        List<int> hand = playerHands[senderId];
        List<CardData> playedCards = new List<CardData>();

        foreach (int index in selectedIndices)
        {
            if (index >= 0 && index < hand.Count)
            {
                int cardIndex = hand[index];
                CardData cardData = NetworkDeckManager.Instance.GetCardDataFromIndex(cardIndex);
                if (cardData != null) playedCards.Add(cardData);
            }
        }

        if (playedCards.Count == 0) return;

        List<Card> cardsForEvaluation = new List<Card>();
        foreach (CardData cardData in playedCards)
        {
            GameObject tempCardObj = new GameObject("TempCard");
            Card tempCard = tempCardObj.AddComponent<Card>();
            tempCard.Initialize(cardData);
            cardsForEvaluation.Add(tempCard);
        }

        HandResult result = HandEvaluator.EvaluateHand(cardsForEvaluation);

        foreach (Card card in cardsForEvaluation)
        {
            Destroy(card.gameObject);
        }

        var playerController = NetworkManager.ConnectedClients[senderId].PlayerObject.GetComponent<NetworkPlayerController>();
        if (playerController != null)
        {
            playerController.playerScore.Value += result.finalScore;
        }

        System.Array.Sort(selectedIndices);
        for (int i = selectedIndices.Length - 1; i >= 0; i--)
        {
            int index = selectedIndices[i];
            if (index >= 0 && index < hand.Count)
            {
                int cardIndex = hand[index];
                hand.RemoveAt(index);
                NetworkDeckManager.Instance.DiscardCard(cardIndex);
            }
        }

        handsRemaining.Value--;

        NotifyHandPlayedClientRpc(senderId, result.description, result.baseScore, result.multiplier, result.finalScore);

        RemoveCardsFromHandClientRpc(selectedIndices, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { senderId }
            }
        });

        if (playerController != null && playerController.playerScore.Value >= targetScore)
        {
            EndGame(senderId, true);
            return;
        }

        if (handsRemaining.Value <= 0)
        {
            StartCoroutine(EndRoundDelayed());
        }
        else
        {
            SwitchTurn();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDiscardCardsServerRpc(int[] selectedIndices, ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        if (senderId != currentTurnPlayer.Value)
        {
            NotifyInvalidActionClientRpc("Not your turn!", new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderId }
                }
            });
            return;
        }

        if (discardsRemaining.Value <= 0)
        {
            NotifyInvalidActionClientRpc("No discards remaining!", new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderId }
                }
            });
            return;
        }

        if (!playerHands.ContainsKey(senderId)) return;

        List<int> hand = playerHands[senderId];

        System.Array.Sort(selectedIndices);
        for (int i = selectedIndices.Length - 1; i >= 0; i--)
        {
            int index = selectedIndices[i];
            if (index >= 0 && index < hand.Count)
            {
                int cardIndex = hand[index];
                hand.RemoveAt(index);
                NetworkDeckManager.Instance.DiscardCard(cardIndex);
            }
        }

        discardsRemaining.Value--;

        RemoveCardsFromHandClientRpc(selectedIndices, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { senderId }
            }
        });

        foreach (int index in selectedIndices)
        {
            DealCardToPlayer(senderId);
        }
    }

    [ClientRpc]
    private void NotifyInvalidActionClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
    }

    [ClientRpc]
    private void RemoveCardsFromHandClientRpc(int[] indices, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkHandManager.Instance != null)
        {
            NetworkHandManager.Instance.RemoveCardsAtIndices(indices);
        }
    }

    [ClientRpc]
    private void NotifyHandPlayedClientRpc(ulong playerId, string description, int baseScore, int multiplier, int finalScore)
    {
        HandResult result = new HandResult(HandType.HighCard, baseScore, multiplier, description);
        OnHandPlayed?.Invoke(result);
    }

    private void SwitchTurn()
    {
        if (!IsServer) return;

        var clientIds = NetworkManager.ConnectedClientsIds;
        List<ulong> clientIdList = new List<ulong>(clientIds);
        int currentIndex = clientIdList.IndexOf(currentTurnPlayer.Value);
        if (currentIndex == -1)
        {
          
            currentTurnPlayer.Value = clientIdList[0];
        }
        else
        {
            int nextIndex = (currentIndex + 1) % clientIdList.Count;
            currentTurnPlayer.Value = clientIdList[nextIndex];
        }
        Debug.Log($"[Server] Turn switched to player {currentTurnPlayer.Value}");
    }

    private IEnumerator EndRoundDelayed()
    {
        yield return new WaitForSeconds(2f);
        currentRound.Value++;

        if (currentRound.Value > maxRounds)
        {
            ulong winnerId = 0;
            int highestScore = 0;

            foreach (var client in NetworkManager.ConnectedClients)
            {
                var playerController = client.Value.PlayerObject.GetComponent<NetworkPlayerController>();
                if (playerController != null && playerController.playerScore.Value > highestScore)
                {
                    highestScore = playerController.playerScore.Value;
                    winnerId = client.Key;
                }
            }

            EndGame(winnerId, false);
        }
        else
        {
            StartNewRound();
        }
    }

    private void EndGame(ulong winnerId, bool reachedTarget)
    {
        if (!IsServer) return;
        NotifyGameEndClientRpc(winnerId, reachedTarget);
    }

    [ClientRpc]
    private void NotifyGameEndClientRpc(ulong winnerId, bool reachedTarget)
    {
        bool isLocalPlayerWinner = (winnerId == NetworkManager.Singleton.LocalClientId);
        OnGameEnded?.Invoke(winnerId, isLocalPlayerWinner);
    }

    private void OnRoundChangedCallback(int oldValue, int newValue)
    {
        OnRoundChanged?.Invoke(newValue, handsRemaining.Value);
    }

    private void OnHandsRemainingChanged(int oldValue, int newValue) { }

    private void OnTurnPlayerChanged(ulong oldValue, ulong newValue)
    {
        OnTurnChanged?.Invoke(newValue);
    }

    public bool IsLocalPlayerTurn()
    {
        return NetworkManager != null && currentTurnPlayer.Value == NetworkManager.LocalClientId;
    }

    public int GetTargetScore() => targetScore;
    public int GetHandsRemaining() => handsRemaining.Value;
    public int GetDiscardsRemaining() => discardsRemaining.Value;
    private void SpawnNetworkDeck()
    {
        if (!IsServer) return;
        if (NetworkDeckManager.Instance == null)
        {
            Debug.LogError("[Server] NetworkDeckManager not found in scene!");
        }
        else
        {
            Debug.Log("[Server] NetworkDeckManager ready");
        }
    }
}
