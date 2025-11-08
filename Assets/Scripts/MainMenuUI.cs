using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Panel (Optional)")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Debug.Log("Main Menu initialized");
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play button clicked - Loading GamePlay");
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("GamePlay");
        }
        else
        {
            Debug.LogError("SceneLoader instance not found!");
        }
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
        else
        {
            Debug.Log("Settings panel not assigned - coming in Phase 5!");
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
}
