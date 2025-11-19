using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int targetScore = 300;
    [SerializeField] private int maxRounds = 8;
    [SerializeField] private int handsPerRound = 3;
    [SerializeField] private int discardsPerRound = 3;
    [SerializeField] private int initialHandSize = 5;

    [Header("Current State")]
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int playerScore = 0;
    [SerializeField] private int handsRemaining = 3;
    [SerializeField] private int discardsRemaining = 3;
    [SerializeField] private bool isPlayerTurn = true;

    public static AIGameManager Instance { get; private set; }

    public System.Action<int> OnPlayerScoreChanged;
    public System.Action<int> OnAIScoreChanged;
    public System.Action<int, int> OnRoundChanged;
    public System.Action<bool> OnTurnChanged;
    public System.Action<HandResult> OnHandPlayed;
    public System.Action<bool> OnGameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        currentRound = 1;
        playerScore = 0;

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.InitializeDeck();
        }

        if (AIPlayer.Instance != null)
        {
            AIPlayer.Instance.InitializeAI(initialHandSize);
            AIPlayer.Instance.OnAIScoreChanged += OnAIScoreUpdated;
        }

        if (HandManager.Instance != null)
        {
            HandManager.Instance.DrawInitialHand();
        }

        StartNewRound();
    }

    private void StartNewRound()
    {
        handsRemaining = handsPerRound;
        discardsRemaining = discardsPerRound;
        isPlayerTurn = true;

        Debug.Log($"=== ROUND {currentRound} START ===");
        Debug.Log($"Player: {playerScore} | AI: {(AIPlayer.Instance != null ? AIPlayer.Instance.GetScore() : 0)}");

        OnRoundChanged?.Invoke(currentRound, handsRemaining);
        OnTurnChanged?.Invoke(isPlayerTurn);
    }

    public void PlayerPlayHand(List<Card> selectedCards)
    {
        if (!isPlayerTurn) return;

        if (handsRemaining <= 0) return;

        if (selectedCards.Count == 0) return;

        HandResult result = HandEvaluator.EvaluateHand(selectedCards);
        playerScore += result.finalScore;
        handsRemaining--;

        Debug.Log($"[Player] {result.description} (+{result.finalScore})");

        OnPlayerScoreChanged?.Invoke(playerScore);
        OnHandPlayed?.Invoke(result);

        if (HandManager.Instance != null)
        {
            foreach (Card card in selectedCards)
            {
                HandManager.Instance.RemoveCardFromHand(card);
            }
        }

        if (playerScore >= targetScore)
        {
            EndGame(true);
            return;
        }

        if (handsRemaining <= 0)
        {
            StartCoroutine(EndRoundDelayed());
        }
        else
        {
            SwitchTurn();
        }
    }

    public void PlayerDiscardCards(List<Card> selectedCards)
    {
        if (!isPlayerTurn) return;

        if (discardsRemaining <= 0) return;

        if (selectedCards.Count == 0) return;

        foreach (Card card in selectedCards)
        {
            DeckManager.Instance.DiscardCard(card.CardData);
            HandManager.Instance.RemoveCardFromHand(card);
            HandManager.Instance.DrawCard();
        }

        discardsRemaining--;
        Debug.Log($"[Player] Discarded {selectedCards.Count} cards");
    }

    private void SwitchTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        OnTurnChanged?.Invoke(isPlayerTurn);

        if (!isPlayerTurn)
        {
            StartCoroutine(ExecuteAITurn());
        }
    }

    private IEnumerator ExecuteAITurn()
    {
        yield return new WaitForSeconds(0.5f);

        if (AIPlayer.Instance != null)
        {
            yield return StartCoroutine(AIPlayer.Instance.TakeAITurn(() =>
            {
                handsRemaining--;

                if (AIPlayer.Instance.GetScore() >= targetScore)
                {
                    EndGame(false);
                    return;
                }

                if (handsRemaining <= 0)
                {
                    StartCoroutine(EndRoundDelayed());
                }
                else
                {
                    SwitchTurn();
                }
            }));
        }
    }

    private void OnAIScoreUpdated(int score)
    {
        OnAIScoreChanged?.Invoke(score);
    }

    private IEnumerator EndRoundDelayed()
    {
        Debug.Log($"=== ROUND {currentRound} END ===");
        yield return new WaitForSeconds(2f);

        currentRound++;

        if (currentRound > maxRounds)
        {
            int aiScore = AIPlayer.Instance != null ? AIPlayer.Instance.GetScore() : 0;
            bool playerWon = playerScore > aiScore;
            EndGame(playerWon);
        }
        else
        {
            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.InitializeDeck();
            }

            if (HandManager.Instance != null)
            {
                HandManager.Instance.ClearHand();
                HandManager.Instance.DrawInitialHand();
            }

            if (AIPlayer.Instance != null)
            {
                AIPlayer.Instance.InitializeAI(initialHandSize);
            }

            StartNewRound();
        }
    }

    private void EndGame(bool playerWon)
    {
        Debug.Log($"=== GAME END ===");
        Debug.Log(playerWon ? "PLAYER WINS!" : "AI WINS!");

        OnGameEnded?.Invoke(playerWon);

        if (AudioManager.Instance != null)
        {
            if (playerWon)
                AudioManager.Instance.PlayWin();
            else
                AudioManager.Instance.PlayLose();
        }
    }

    public int GetPlayerScore() => playerScore;
    public int GetAIScore() => AIPlayer.Instance != null ? AIPlayer.Instance.GetScore() : 0;
    public int GetTargetScore() => targetScore;
    public int GetCurrentRound() => currentRound;
    public int GetHandsRemaining() => handsRemaining;
    public int GetDiscardsRemaining() => discardsRemaining;
    public bool IsPlayerTurn() => isPlayerTurn;
}
