using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private int targetScore = 300;
    [SerializeField] private int maxRounds = 8;
    [SerializeField] private int handsPerRound = 3;
    [SerializeField] private int discardsPerRound = 3;

    [SerializeField] private int currentRound = 1;
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int handsRemaining;
    [SerializeField] private int discardsRemaining;

    private int activeScoreMultiplier = 1;
    private int activeDiscardBonus = 0;

    private List<Card> selectedCardsToPlay = new List<Card>();

    public static RoundManager Instance { get; private set; }

    public System.Action<int> OnScoreChanged;
    public System.Action<int, int> OnRoundChanged;
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
        StartNewRound();
    }

    public void StartNewRound()
    {
        handsRemaining = handsPerRound;
        discardsRemaining = discardsPerRound;
        activeScoreMultiplier = 1;
        activeDiscardBonus = 0;

        Debug.Log($"=== ROUND {currentRound} START ===");
        Debug.Log($"Target Score: {targetScore}");
        Debug.Log($"Current Score: {currentScore}");
        Debug.Log($"Hands: {handsRemaining} | Discards: {discardsRemaining}");

        OnRoundChanged?.Invoke(currentRound, handsRemaining);
    }

    public void ToggleCardSelection(Card card)
    {
        if (selectedCardsToPlay.Contains(card))
        {
            selectedCardsToPlay.Remove(card);
            card.SetSelected(false);
            Debug.Log($"Removed from play selection: {card.CardData.GetDisplayName()}");
        }
        else
        {
            selectedCardsToPlay.Add(card);
            card.SetSelected(true);
            Debug.Log($"Added to play selection: {card.CardData.GetDisplayName()}");
        }
    }

    public void PlaySelectedCards()
    {
        if (selectedCardsToPlay.Count == 0)
        {
            Debug.Log("No cards selected to play!");
            return;
        }

        if (handsRemaining <= 0)
        {
            Debug.Log("No hands remaining this round!");
            return;
        }

        HandResult result = HandEvaluator.EvaluateHand(selectedCardsToPlay);
        result.finalScore *= activeScoreMultiplier;
        currentScore += result.finalScore;
        handsRemaining--;

        Debug.Log($"=== HAND PLAYED ===");
        Debug.Log($"{result.description}");
        Debug.Log($"Base: {result.baseScore} × Mult: {result.multiplier} = {result.finalScore}");
        Debug.Log($"Total Score: {currentScore}/{targetScore}");

        OnScoreChanged?.Invoke(currentScore);
        OnHandPlayed?.Invoke(result);

        foreach (Card card in selectedCardsToPlay)
        {
            if (HandManager.Instance != null)
            {
                HandManager.Instance.RemoveCardFromHand(card);
            }
        }

        selectedCardsToPlay.Clear();

        if (currentScore >= targetScore)
        {
            EndGame(true);
            return;
        }

        if (handsRemaining <= 0)
        {
            EndRound();
        }
    }

    public void DiscardSelectedCards()
    {
        if (selectedCardsToPlay.Count == 0)
        {
            Debug.Log("No cards selected to discard!");
            return;
        }

        if (discardsRemaining <= 0)
        {
            Debug.Log("No discards remaining this round!");
            return;
        }

        int cardsDiscarded = selectedCardsToPlay.Count;

        if (activeDiscardBonus > 0)
        {
            int bonusPoints = cardsDiscarded * activeDiscardBonus;
            currentScore += bonusPoints;
            Debug.Log($"Discard bonus: +{bonusPoints} points!");
            OnScoreChanged?.Invoke(currentScore);
        }

        foreach (Card card in selectedCardsToPlay)
        {
            if (HandManager.Instance != null)
            {
                DeckManager.Instance.DiscardCard(card.CardData);
                HandManager.Instance.RemoveCardFromHand(card);
                HandManager.Instance.DrawCard();
            }
        }

        discardsRemaining--;
        selectedCardsToPlay.Clear();

        Debug.Log($"Discarded {cardsDiscarded} cards. Discards remaining: {discardsRemaining}");
    }

    private void EndRound()
    {
        Debug.Log($"=== ROUND {currentRound} END ===");
        Debug.Log($"Score: {currentScore}/{targetScore}");

        currentRound++;

        if (currentRound > maxRounds)
        {
            EndGame(false);
        }
        else
        {
            StartCoroutine(StartNextRoundDelayed());
        }
    }

    private IEnumerator StartNextRoundDelayed()
    {
        yield return new WaitForSeconds(2f);

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.InitializeDeck();
        }

        if (HandManager.Instance != null)
        {
            HandManager.Instance.ClearHand();
            HandManager.Instance.DrawInitialHand();
        }

        StartNewRound();
    }

    private void EndGame(bool victory)
    {
        Debug.Log($"=== GAME END ===");
        Debug.Log(victory ? "VICTORY!" : "DEFEAT!");
        Debug.Log($"Final Score: {currentScore}");

        OnGameEnded?.Invoke(victory);
    }

    public void AddScoreMultiplier(int multiplier)
    {
        activeScoreMultiplier += multiplier;
        Debug.Log($"Score multiplier now: {activeScoreMultiplier}x");
    }

    public void AddDiscardBonus(int bonus)
    {
        activeDiscardBonus += bonus;
        Debug.Log($"Discard bonus now: +{activeDiscardBonus} per card");
    }

    public int GetCurrentScore() => currentScore;
    public int GetTargetScore() => targetScore;
    public int GetCurrentRound() => currentRound;
    public int GetHandsRemaining() => handsRemaining;
    public int GetDiscardsRemaining() => discardsRemaining;
    public List<Card> GetSelectedCards() => selectedCardsToPlay;
}
