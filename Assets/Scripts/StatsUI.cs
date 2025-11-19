using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI statsContentText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetButton;

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseStats);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetStats);

        if (statsPanel != null)
            statsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseStats);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetStats);
    }

    public void ShowStats()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
            UpdateStatsDisplay();
        }
    }

    private void CloseStats()
    {
        if (statsPanel != null)
            statsPanel.SetActive(false);
    }

    private void ResetStats()
    {
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.ResetStats();
            UpdateStatsDisplay();
        }
    }

    private void UpdateStatsDisplay()
    {
        if (statsContentText == null || StatsManager.Instance == null) return;

        int played = StatsManager.Instance.GetGamesPlayed();
        int won = StatsManager.Instance.GetGamesWon();
        int lost = StatsManager.Instance.GetGamesLost();
        float winRate = StatsManager.Instance.GetWinRate();
        int highest = StatsManager.Instance.GetHighestScore();
        float average = StatsManager.Instance.GetAverageScore();
        int flushes = StatsManager.Instance.GetFlushesPlayed();
        int straights = StatsManager.Instance.GetStraightsPlayed();
        int fourKinds = StatsManager.Instance.GetFourOfKindsPlayed();

        string statsText = $"<b>Games Played:</b> {played}\n";
        statsText += $"<b>Games Won:</b> <color=green>{won}</color>\n";
        statsText += $"<b>Games Lost:</b> <color=red>{lost}</color>\n";
        statsText += $"<b>Win Rate:</b> {winRate:F1}%\n\n";
        statsText += $"<b>Highest Score:</b> {highest}\n";
        statsText += $"<b>Average Score:</b> {average:F0}\n\n";
        statsText += $"<b><size=32>Special Hands:</size></b>\n";
        statsText += $"  Flushes: {flushes}\n";
        statsText += $"  Straights: {straights}\n";
        statsText += $"  Four of a Kinds: {fourKinds}";

        statsContentText.text = statsText;
    }
}
