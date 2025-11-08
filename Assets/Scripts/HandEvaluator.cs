using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HandType
{
    HighCard = 0,
    OnePair = 1,
    TwoPair = 2,
    ThreeOfAKind = 3,
    Straight = 4,
    Flush = 5,
    FullHouse = 6,
    FourOfAKind = 7,
    StraightFlush = 8,
    RoyalFlush = 9
}

public class HandResult
{
    public HandType handType;
    public int baseScore;
    public int multiplier;
    public int finalScore;
    public string description;

    public HandResult(HandType type, int score, int mult, string desc)
    {
        handType = type;
        baseScore = score;
        multiplier = mult;
        finalScore = score * mult;
        description = desc;
    }
}

public static class HandEvaluator
{
    private static readonly Dictionary<HandType, int> baseScores = new Dictionary<HandType, int>
    {
        { HandType.HighCard, 5 },
        { HandType.OnePair, 10 },
        { HandType.TwoPair, 20 },
        { HandType.ThreeOfAKind, 30 },
        { HandType.Straight, 30 },
        { HandType.Flush, 35 },
        { HandType.FullHouse, 40 },
        { HandType.FourOfAKind, 60 },
        { HandType.StraightFlush, 100 },
        { HandType.RoyalFlush, 100 }
    };

    private static readonly Dictionary<HandType, int> baseMultipliers = new Dictionary<HandType, int>
    {
        { HandType.HighCard, 1 },
        { HandType.OnePair, 2 },
        { HandType.TwoPair, 2 },
        { HandType.ThreeOfAKind, 3 },
        { HandType.Straight, 4 },
        { HandType.Flush, 4 },
        { HandType.FullHouse, 4 },
        { HandType.FourOfAKind, 7 },
        { HandType.StraightFlush, 8 },
        { HandType.RoyalFlush, 8 }
    };

    public static HandResult EvaluateHand(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            return new HandResult(HandType.HighCard, 0, 1, "No cards played");

        List<Card> regularCards = cards.Where(c => c.CardData.cardType == CardType.Regular).ToList();
        if (regularCards.Count == 0)
            return new HandResult(HandType.HighCard, 0, 1, "No regular cards");

        HandType handType = DetermineHandType(regularCards);
        int baseScore = CalculateBaseScore(regularCards, handType);
        int multiplier = baseMultipliers[handType];
        string description = GetHandDescription(handType, regularCards);

        return new HandResult(handType, baseScore, multiplier, description);
    }

    private static HandType DetermineHandType(List<Card> cards)
    {
        if (cards.Count < 5) return EvaluatePartialHand(cards);

        bool isFlush = IsFlush(cards);
        bool isStraight = IsStraight(cards);

        if (isFlush && isStraight)
        {
            if (IsRoyalFlush(cards))
                return HandType.RoyalFlush;
            return HandType.StraightFlush;
        }

        if (IsFourOfAKind(cards)) return HandType.FourOfAKind;
        if (IsFullHouse(cards)) return HandType.FullHouse;
        if (isFlush) return HandType.Flush;
        if (isStraight) return HandType.Straight;
        if (IsThreeOfAKind(cards)) return HandType.ThreeOfAKind;
        if (IsTwoPair(cards)) return HandType.TwoPair;
        if (IsOnePair(cards)) return HandType.OnePair;

        return HandType.HighCard;
    }

    private static HandType EvaluatePartialHand(List<Card> cards)
    {
        if (IsFourOfAKind(cards)) return HandType.FourOfAKind;
        if (IsThreeOfAKind(cards)) return HandType.ThreeOfAKind;
        if (IsTwoPair(cards)) return HandType.TwoPair;
        if (IsOnePair(cards)) return HandType.OnePair;
        return HandType.HighCard;
    }

    private static bool IsFlush(List<Card> cards)
    {
        if (cards.Count < 5) return false;
        CardSuit firstSuit = cards[0].CardData.suit;
        return cards.All(c => c.CardData.suit == firstSuit);
    }

    private static bool IsStraight(List<Card> cards)
    {
        if (cards.Count < 5) return false;
        List<int> ranks = cards.Select(c => (int)c.CardData.rank).OrderBy(r => r).ToList();
        for (int i = 0; i < ranks.Count - 1; i++)
        {
            if (ranks[i + 1] != ranks[i] + 1)
                return false;
        }
        return true;
    }

    private static bool IsRoyalFlush(List<Card> cards)
    {
        List<int> ranks = cards.Select(c => (int)c.CardData.rank).OrderBy(r => r).ToList();
        return ranks.SequenceEqual(new List<int> { 10, 11, 12, 13, 14 });
    }

    private static bool IsFourOfAKind(List<Card> cards)
    {
        var rankGroups = cards.GroupBy(c => c.CardData.rank);
        return rankGroups.Any(g => g.Count() >= 4);
    }

    private static bool IsFullHouse(List<Card> cards)
    {
        if (cards.Count < 5) return false;
        var rankGroups = cards.GroupBy(c => c.CardData.rank).OrderByDescending(g => g.Count()).ToList();
        return rankGroups.Count >= 2 && rankGroups[0].Count() == 3 && rankGroups[1].Count() == 2;
    }

    private static bool IsThreeOfAKind(List<Card> cards)
    {
        var rankGroups = cards.GroupBy(c => c.CardData.rank);
        return rankGroups.Any(g => g.Count() >= 3);
    }

    private static bool IsTwoPair(List<Card> cards)
    {
        var rankGroups = cards.GroupBy(c => c.CardData.rank);
        return rankGroups.Count(g => g.Count() >= 2) >= 2;
    }

    private static bool IsOnePair(List<Card> cards)
    {
        var rankGroups = cards.GroupBy(c => c.CardData.rank);
        return rankGroups.Any(g => g.Count() >= 2);
    }

    private static int CalculateBaseScore(List<Card> cards, HandType handType)
    {
        int cardValueSum = cards.Sum(c => c.CardData.GetValue());
        int handBonus = baseScores[handType];
        return cardValueSum + handBonus;
    }

    private static string GetHandDescription(HandType handType, List<Card> cards)
    {
        switch (handType)
        {
            case HandType.RoyalFlush:
                return "Royal Flush!";
            case HandType.StraightFlush:
                return "Straight Flush!";
            case HandType.FourOfAKind:
                var fourRank = cards.GroupBy(c => c.CardData.rank).First(g => g.Count() >= 4).Key;
                return $"Four {fourRank}s!";
            case HandType.FullHouse:
                return "Full House!";
            case HandType.Flush:
                return $"Flush of {cards[0].CardData.suit}!";
            case HandType.Straight:
                return "Straight!";
            case HandType.ThreeOfAKind:
                var threeRank = cards.GroupBy(c => c.CardData.rank).First(g => g.Count() >= 3).Key;
                return $"Three {threeRank}s!";
            case HandType.TwoPair:
                return "Two Pair!";
            case HandType.OnePair:
                var pairRank = cards.GroupBy(c => c.CardData.rank).First(g => g.Count() >= 2).Key;
                return $"Pair of {pairRank}s!";
            default:
                return "High Card";
        }
    }
}
