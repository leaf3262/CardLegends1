using UnityEngine;
using System.Collections.Generic;

public class ThemeManager : MonoBehaviour
{
    [SerializeField] private List<CardTheme> allThemes = new List<CardTheme>();

    [SerializeField] private CardTheme currentTheme;

    public static ThemeManager Instance { get; private set; }

    public System.Action<CardTheme> OnThemeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSavedTheme();
    }

    private void Start()
    {
        if (currentTheme == null && allThemes.Count > 0)
        {
            currentTheme = allThemes[0];
        }
    }

    public CardTheme GetCurrentTheme()
    {
        return currentTheme;
    }

    public void SetTheme(CardTheme theme)
    {
        if (theme == null || !theme.isUnlocked)
        {
            Debug.LogWarning("Cannot set theme - theme is null or locked");
            return;
        }

        currentTheme = theme;
        SaveTheme();

        Debug.Log($"Theme changed to: {theme.themeName}");
        OnThemeChanged?.Invoke(theme);
    }

    public List<CardTheme> GetAllThemes()
    {
        return new List<CardTheme>(allThemes);
    }

    public void UnlockTheme(string themeName)
    {
        CardTheme theme = allThemes.Find(t => t.themeName == themeName);
        if (theme != null)
        {
            theme.isUnlocked = true;
            SaveTheme();
            Debug.Log($"Unlocked theme: {themeName}");
        }
    }

    public void ApplyThemeToCard(Card card)
    {
        if (card == null || currentTheme == null) return;

        CardData cardData = card.CardData;
        if (cardData == null) return;

        if (cardData.cardType == CardType.Regular)
        {
            Color themeColor = currentTheme.GetColorForSuit(cardData.suit);
            cardData.cardColor = themeColor;
        }
        else
        {
            cardData.cardColor = currentTheme.powerCardColor;
        }
    }

    private void SaveTheme()
    {
        if (currentTheme != null)
        {
            PlayerPrefs.SetString("CurrentTheme", currentTheme.themeName);
            PlayerPrefs.Save();
        }
    }

    private void LoadSavedTheme()
    {
        string savedThemeName = PlayerPrefs.GetString("CurrentTheme", "");

        if (!string.IsNullOrEmpty(savedThemeName))
        {
            CardTheme savedTheme = allThemes.Find(t => t.themeName == savedThemeName);
            if (savedTheme != null && savedTheme.isUnlocked)
            {
                currentTheme = savedTheme;
                Debug.Log($"Loaded saved theme: {savedThemeName}");
            }
        }
    }
}
