using UnityEngine;

[CreateAssetMenu(fileName = "New Card Theme", menuName = "Card Legends/Card Theme")]
public class CardTheme : ScriptableObject
{
    [Header("Theme Info")]
    public string themeName;
    public string description;
    public bool isUnlocked = true;

    [Header("Colors")]
    public Color heartsColor = new Color(1f, 0.2f, 0.2f);
    public Color diamondsColor = new Color(1f, 0.4f, 0.4f);
    public Color clubsColor = new Color(0f, 0.2f, 0.4f);
    public Color spadesColor = new Color(0.1f, 0.1f, 0.1f);
    public Color powerCardColor = new Color(0.6f, 0f, 1f);

    [Header("Card Back")]
    public Color cardBackColor = Color.white;
    public Sprite cardBackSprite;

    [Header("UI")]
    public Sprite themeIcon;

    public Color GetColorForSuit(CardSuit suit)
    {
        switch (suit)
        {
            case CardSuit.Hearts: return heartsColor;
            case CardSuit.Diamonds: return diamondsColor;
            case CardSuit.Clubs: return clubsColor;
            case CardSuit.Spades: return spadesColor;
            default: return Color.white;
        }
    }
}
