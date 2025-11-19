using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float thinkingTimeMin = 1f;
    [SerializeField] private float thinkingTimeMax = 2.5f;
    [SerializeField] private AIDifficulty difficulty = AIDifficulty.Medium;

    [Header("AI Stats")]
    [SerializeField] private int aiScore = 0;
    [SerializeField] private List<CardData> aiHand = new List<CardData>();

    public enum AIDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public static AIPlayer Instance { get; private set; }

    public System.Action<int> OnAIScoreChanged;
    public System.Action<string, int> OnAIPlayedHand;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeAI(int handSize)
    {
        aiHand.Clear();
        aiScore = 0;

        for (int i = 0; i < handSize; i++)
        {
            DrawCard();
        }

        Debug.Log($"[AI] Initialized with {aiHand.Count} cards");
    }

    public void DrawCard()
    {
        if (DeckManager.Instance == null) return;

        CardData card = DeckManager.Instance.DrawCard();
        if (card != null)
        {
            aiHand.Add(card);
        }
    }

    public IEnumerator TakeAITurn(System.Action onTurnComplete)
    {
        Debug.Log("[AI] Thinking...");

        float thinkTime = Random.Range(thinkingTimeMin, thinkingTimeMax);
        yield return new WaitForSeconds(thinkTime);

        bool shouldPlay = ShouldPlayHand();

        if (shouldPlay)
        {
            List<CardData> cardsToPlay = SelectCardsToPlay();
            PlayCards(cardsToPlay);
        }
        else
        {
            List<CardData> cardsToDiscard = SelectCardsToDiscard();
            DiscardCards(cardsToDiscard);
        }

        onTurnComplete?.Invoke();
    }

    private bool ShouldPlayHand()
    {
        if (difficulty == AIDifficulty.Easy)
        {
            return Random.value > 0.3f;
        }

        List<CardData> regularCards = aiHand.Where(c => c.cardType == CardType.Regular).ToList();

        if (regularCards.Count < 2)
        {
            return false;
        }

        bool hasPair = HasPair(regularCards);

        if (difficulty == AIDifficulty.Medium)
        {
            return hasPair || Random.value > 0.4f;
        }

        return hasPair || aiScore > 150;
    }

    private List<CardData> SelectCardsToPlay()
    {
        List<CardData> regularCards = aiHand.Where(c => c.cardType == CardType.Regular).ToList();
        List<CardData> selectedCards = new List<CardData>();

        if (difficulty == AIDifficulty.Easy)
        {
            int numCards = Random.Range(2, Mathf.Min(4, regularCards.Count + 1));
            for (int i = 0; i < numCards && i < regularCards.Count; i++)
            {
                selectedCards.Add(regularCards[i]);
            }
        }
        else if (difficulty == AIDifficulty.Medium)
        {
            selectedCards = FindBestPair(regularCards);

            if (selectedCards.Count == 0)
            {
                int numCards = Random.Range(2, Mathf.Min(4, regularCards.Count + 1));
                selectedCards = regularCards.Take(numCards).ToList();
            }
        }
        else
        {
            selectedCards = FindBestHand(regularCards);
        }

        return selectedCards;
    }

    private List<CardData> FindBestPair(List<CardData> cards)
    {
        var rankGroups = cards.GroupBy(c => c.rank).Where(g => g.Count() >= 2);

        if (rankGroups.Any())
        {
            var bestPair = rankGroups.OrderByDescending(g => (int)g.Key).First();
            return bestPair.Take(2).ToList();
        }

        return new List<CardData>();
    }

    private List<CardData> FindBestHand(List<CardData> cards)
    {
        if (cards.Count < 2) return cards;

        var threeOfKind = cards.GroupBy(c => c.rank).FirstOrDefault(g => g.Count() >= 3);
        if (threeOfKind != null)
        {
            return threeOfKind.Take(3).ToList();
        }

        if (cards.Count >= 5)
        {
            var flushSuit = cards.GroupBy(c => c.suit).FirstOrDefault(g => g.Count() >= 5);
            if (flushSuit != null)
            {
                return flushSuit.Take(5).ToList();
            }
        }

        if (cards.Count >= 5)
        {
            var straight = FindStraight(cards);
            if (straight.Count >= 5)
            {
                return straight;
            }
        }

        var pairs = cards.GroupBy(c => c.rank).Where(g => g.Count() >= 2).ToList();
        if (pairs.Count >= 2)
        {
            List<CardData> twoPair = new List<CardData>();
            twoPair.AddRange(pairs[0].Take(2));
            twoPair.AddRange(pairs[1].Take(2));
            return twoPair;
        }

        var bestPair = FindBestPair(cards);
        if (bestPair.Count > 0) return bestPair;

        return cards.OrderByDescending(c => (int)c.rank).Take(3).ToList();
    }

    private List<CardData> FindStraight(List<CardData> cards)
    {
        var sortedCards = cards.OrderBy(c => (int)c.rank).ToList();
        List<CardData> straight = new List<CardData>();

        for (int i = 0; i < sortedCards.Count - 1; i++)
        {
            if (straight.Count == 0)
            {
                straight.Add(sortedCards[i]);
            }

            int currentRank = (int)sortedCards[i].rank;
            int nextRank = (int)sortedCards[i + 1].rank;

            if (nextRank == currentRank + 1)
            {
                straight.Add(sortedCards[i + 1]);

                if (straight.Count >= 5)
                {
                    return straight.Take(5).ToList();
                }
            }
            else if (nextRank != currentRank)
            {
                straight.Clear();
                straight.Add(sortedCards[i + 1]);
            }
        }

        return straight.Count >= 5 ? straight.Take(5).ToList() : new List<CardData>();
    }

    private bool HasPair(List<CardData> cards)
    {
        return cards.GroupBy(c => c.rank).Any(g => g.Count() >= 2);
    }

    private void PlayCards(List<CardData> cardsToPlay)
    {
        if (cardsToPlay.Count == 0)
        {
            Debug.Log("[AI] No cards to play");
            return;
        }

        List<Card> tempCards = new List<Card>();
        foreach (CardData cardData in cardsToPlay)
        {
            GameObject tempObj = new GameObject("TempAICard");
            Card tempCard = tempObj.AddComponent<Card>();
            tempCard.Initialize(cardData);
            tempCards.Add(tempCard);
        }

        HandResult result = HandEvaluator.EvaluateHand(tempCards);

        foreach (Card card in tempCards)
        {
            Destroy(card.gameObject);
        }

        aiScore += result.finalScore;

        foreach (CardData card in cardsToPlay)
        {
            aiHand.Remove(card);
            DeckManager.Instance.DiscardCard(card);
        }

        Debug.Log($"[AI] Played {cardsToPlay.Count} cards: {result.description} (+{result.finalScore})");

        OnAIScoreChanged?.Invoke(aiScore);
        OnAIPlayedHand?.Invoke(result.description, result.finalScore);
    }

    private List<CardData> SelectCardsToDiscard()
    {
        List<CardData> regularCards = aiHand.Where(c => c.cardType == CardType.Regular).ToList();
        List<CardData> toDiscard = new List<CardData>();

        if (regularCards.Count == 0) return toDiscard;

        int numToDiscard = Random.Range(1, Mathf.Min(3, regularCards.Count + 1));
        toDiscard = regularCards.OrderBy(c => (int)c.rank).Take(numToDiscard).ToList();

        return toDiscard;
    }

    private void DiscardCards(List<CardData> cardsToDiscard)
    {
        if (cardsToDiscard.Count == 0) return;

        foreach (CardData card in cardsToDiscard)
        {
            aiHand.Remove(card);
            DeckManager.Instance.DiscardCard(card);
        }

        Debug.Log($"[AI] Discarded {cardsToDiscard.Count} cards");

        for (int i = 0; i < cardsToDiscard.Count; i++)
        {
            DrawCard();
        }
    }

    public int GetScore() => aiScore;
    public int GetHandSize() => aiHand.Count;
}
