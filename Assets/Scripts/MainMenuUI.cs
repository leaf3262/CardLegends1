using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private CustomizationManager customizationManager;
    [SerializeField] private Button customizeButton;
    [SerializeField] private StatsUI statsUI;
    [SerializeField] private Button statsButton;

    [Header("Settings Panel (Optional)")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        if (customizeButton != null)
            customizeButton.onClick.AddListener(OnCustomizeClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Debug.Log("Main Menu initialized");
        if (statsButton != null)
            statsButton.onClick.AddListener(OnStatsClicked);
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
        if (statsButton != null)
            statsButton.onClick.RemoveListener(OnStatsClicked);
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play button clicked - Loading Lobby");
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("Lobby");
        }
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        if (settingsManager != null)
        {
            settingsManager.ShowSettings();
        }
        else
        {
            Debug.LogWarning("SettingsManager not assigned!");
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit button clicked");
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.QuitGame();
        }
        else
        {
            Application.Quit();
        }
    }
    private void OnCustomizeClicked()
    {
        Debug.Log("Customize button clicked");
        if (customizationManager != null)
        {
            customizationManager.ShowCustomization();
        }
    }
    private void OnStatsClicked()
    {
        Debug.Log("Stats button clicked");
        if (statsUI != null)
        {
            statsUI.ShowStats();
        }
    }
}
