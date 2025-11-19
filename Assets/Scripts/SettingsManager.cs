using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        LoadSettings();

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);

        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        if (musicToggle != null)
            musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);

        if (sfxToggle != null)
            sfxToggle.onValueChanged.RemoveListener(OnSFXToggleChanged);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseSettings);
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        SaveSettings();
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    private void OnMusicToggleChanged(bool enabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic(enabled);
        }
    }

    private void OnSFXToggleChanged(bool enabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSFX(enabled);
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider != null ? musicVolumeSlider.value : 0.5f);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider != null ? sfxVolumeSlider.value : 0.7f);
        PlayerPrefs.SetInt("MusicEnabled", musicToggle != null && musicToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", sfxToggle != null && sfxToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Settings saved");
    }

    private void LoadSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;

        if (musicToggle != null)
            musicToggle.isOn = musicEnabled;

        if (sfxToggle != null)
            sfxToggle.isOn = sfxEnabled;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
            AudioManager.Instance.ToggleMusic(musicEnabled);
            AudioManager.Instance.ToggleSFX(sfxEnabled);
        }

        Debug.Log("Settings loaded");
    }
}
