using UnityEngine;

public enum CardSuit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum CardRank
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14
}

public enum CardType
{
    Regular,
    Power
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card Legends/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    public CardType cardType;

    [Header("Regular Card Properties")]
    public CardSuit suit;
    public CardRank rank;

    [Header("Power Card Properties")]
    [TextArea(3, 5)]
    public string powerDescription;
    public int powerCost;

    [Header("Visual")]
    public Sprite cardSprite;
    public Color cardColor = Color.white;

    public string GetDisplayName()
    {
        if (cardType == CardType.Power)
            return cardName;

        return $"{rank} of {suit}";
    }

    public int GetValue()
    {
        return (int)rank;
    }
}
