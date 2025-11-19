using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CustomizationManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject customizationPanel;

    [Header("Theme Selection")]
    [SerializeField] private Transform themeButtonContainer;
    [SerializeField] private GameObject themeButtonPrefab;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    private List<GameObject> themeButtons = new List<GameObject>();

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (customizationPanel != null)
            customizationPanel.SetActive(false);

        PopulateThemes();
    }

    private void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
    }

    public void ShowCustomization()
    {
        if (customizationPanel != null)
        {
            customizationPanel.SetActive(true);
        }

        PopulateThemes();
    }

    private void OnBackClicked()
    {
        if (customizationPanel != null)
        {
            customizationPanel.SetActive(false);
        }
    }

    private void PopulateThemes()
    {
        if (ThemeManager.Instance == null) return;

        foreach (GameObject btn in themeButtons)
        {
            if (btn != null)
                Destroy(btn);
        }
        themeButtons.Clear();

        List<CardTheme> themes = ThemeManager.Instance.GetAllThemes();

        foreach (CardTheme theme in themes)
        {
            CreateThemeButton(theme);
        }
    }

    private void CreateThemeButton(CardTheme theme)
    {
        if (themeButtonPrefab == null || themeButtonContainer == null) return;

        GameObject buttonObj = Instantiate(themeButtonPrefab, themeButtonContainer);
        themeButtons.Add(buttonObj);

        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = theme.themeName;
        }

        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = theme.heartsColor;
        }

        Transform previewPanel = buttonObj.transform.Find("ThemePreview");
        if (previewPanel != null)
        {
            Image previewImage = previewPanel.GetComponent<Image>();
            if (previewImage != null)
            {
                previewImage.color = theme.GetColorForSuit(CardSuit.Hearts);
            }
        }

        Transform lockIcon = buttonObj.transform.Find("LockIcon");
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(!theme.isUnlocked);
        }

        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnThemeButtonClicked(theme));
            button.interactable = theme.isUnlocked;
        }

        if (ThemeManager.Instance.GetCurrentTheme() == theme)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 0.7f);
            button.colors = colors;
        }
    }

    private void OnThemeButtonClicked(CardTheme theme)
    {
        if (theme == null || !theme.isUnlocked) return;

        Debug.Log($"Selected theme: {theme.themeName}");

        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.SetTheme(theme);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        PopulateThemes();
    }
}
