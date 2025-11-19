using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Player Info")]
    public NetworkVariable<int> playerScore = new NetworkVariable<int>(0);
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    [Header("Current Hand")]
    private NetworkList<NetworkCardData> playerHand;

    public static NetworkPlayerController LocalPlayer { get; private set; }

    private void Awake()
    {
        playerHand = new NetworkList<NetworkCardData>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this;
            Debug.Log($"Local player spawned. Client ID: {OwnerClientId}");
        }

        playerScore.OnValueChanged += OnScoreChanged;
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        playerScore.OnValueChanged -= OnScoreChanged;

        if (IsOwner)
        {
            LocalPlayer = null;
        }

        base.OnNetworkDespawn();
    }

    private void OnScoreChanged(int oldValue, int newValue)
    {
        Debug.Log($"Player {OwnerClientId} score changed: {oldValue} ? {newValue}");
    }

    [ServerRpc]
    public void RequestDrawCardServerRpc()
    {
        if (!IsServer) return;

        if (NetworkDeckManager.Instance != null)
        {
            NetworkCardData cardData = NetworkDeckManager.Instance.DrawCard();
            playerHand.Add(cardData);
            Debug.Log($"Player {OwnerClientId} drew card");
        }
    }

    [ServerRpc]
    public void RequestPlayHandServerRpc(int[] selectedIndices)
    {
        if (!IsServer) return;

        Debug.Log($"Player {OwnerClientId} played {selectedIndices.Length} cards");

        List<NetworkCardData> playedCards = new List<NetworkCardData>();
        foreach (int index in selectedIndices)
        {
            if (index >= 0 && index < playerHand.Count)
            {
                playedCards.Add(playerHand[index]);
            }
        }

        int handScore = CalculateHandScore(playedCards);
        playerScore.Value += handScore;

        for (int i = selectedIndices.Length - 1; i >= 0; i--)
        {
            int index = selectedIndices[i];
            if (index >= 0 && index < playerHand.Count)
            {
                playerHand.RemoveAt(index);
            }
        }

        NotifyHandPlayedClientRpc(handScore);
    }

    [ClientRpc]
    private void NotifyHandPlayedClientRpc(int scoreGained)
    {
        Debug.Log($"Hand played! Score gained: {scoreGained}");
    }

    private int CalculateHandScore(List<NetworkCardData> cards)
    {
        int score = 0;
        foreach (var card in cards)
        {
            score += (int)card.rank;
        }
        return score;
    }

    [ServerRpc]
    public void SetReadyServerRpc(bool ready)
    {
        isReady.Value = ready;
        Debug.Log($"Player {OwnerClientId} ready status: {ready}");
    }

    public int GetHandCount()
    {
        return playerHand.Count;
    }

    public NetworkCardData GetCardAt(int index)
    {
        if (index >= 0 && index < playerHand.Count)
        {
            return playerHand[index];
        }
        return default;
    }
}
