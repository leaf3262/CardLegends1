using UnityEngine;

public class StatsManager : MonoBehaviour
{
    [SerializeField] private int gamesPlayed = 0;
    [SerializeField] private int gamesWon = 0;
    [SerializeField] private int gamesLost = 0;
    [SerializeField] private int highestScore = 0;
    [SerializeField] private int totalScore = 0;
    [SerializeField] private int flushesPlayed = 0;
    [SerializeField] private int straightsPlayed = 0;
    [SerializeField] private int fourOfKindsPlayed = 0;

    public static StatsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadStats();
    }

    public void RecordGame(bool won, int finalScore)
    {
        gamesPlayed++;
        totalScore += finalScore;

        if (won)
            gamesWon++;
        else
            gamesLost++;

        if (finalScore > highestScore)
            highestScore = finalScore;

        SaveStats();
        Debug.Log($"[Stats] Game recorded. W:{gamesWon} L:{gamesLost}");
    }

    public void RecordSpecialHand(HandType handType)
    {
        switch (handType)
        {
            case HandType.Flush:
            case HandType.StraightFlush:
            case HandType.RoyalFlush:
                flushesPlayed++;
                break;
            case HandType.Straight:
                straightsPlayed++;
                break;
            case HandType.FourOfAKind:
                fourOfKindsPlayed++;
                break;
        }

        SaveStats();
    }

    public float GetWinRate()
    {
        if (gamesPlayed == 0) return 0f;
        return (float)gamesWon / gamesPlayed * 100f;
    }

    public float GetAverageScore()
    {
        if (gamesPlayed == 0) return 0f;
        return (float)totalScore / gamesPlayed;
    }

    private void SaveStats()
    {
        PlayerPrefs.SetInt("GamesPlayed", gamesPlayed);
        PlayerPrefs.SetInt("GamesWon", gamesWon);
        PlayerPrefs.SetInt("GamesLost", gamesLost);
        PlayerPrefs.SetInt("HighestScore", highestScore);
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.SetInt("FlushesPlayed", flushesPlayed);
        PlayerPrefs.SetInt("StraightsPlayed", straightsPlayed);
        PlayerPrefs.SetInt("FourOfKindsPlayed", fourOfKindsPlayed);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        gamesPlayed = PlayerPrefs.GetInt("GamesPlayed", 0);
        gamesWon = PlayerPrefs.GetInt("GamesWon", 0);
        gamesLost = PlayerPrefs.GetInt("GamesLost", 0);
        highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        flushesPlayed = PlayerPrefs.GetInt("FlushesPlayed", 0);
        straightsPlayed = PlayerPrefs.GetInt("StraightsPlayed", 0);
        fourOfKindsPlayed = PlayerPrefs.GetInt("FourOfKindsPlayed", 0);

        Debug.Log($"[Stats] Loaded. Played:{gamesPlayed} Won:{gamesWon} Lost:{gamesLost}");
    }

    public void ResetStats()
    {
        gamesPlayed = 0;
        gamesWon = 0;
        gamesLost = 0;
        highestScore = 0;
        totalScore = 0;
        flushesPlayed = 0;
        straightsPlayed = 0;
        fourOfKindsPlayed = 0;
        SaveStats();
        Debug.Log("[Stats] All stats reset");
    }

    public int GetGamesPlayed() => gamesPlayed;
    public int GetGamesWon() => gamesWon;
    public int GetGamesLost() => gamesLost;
    public int GetHighestScore() => highestScore;
    public int GetFlushesPlayed() => flushesPlayed;
    public int GetStraightsPlayed() => straightsPlayed;
    public int GetFourOfKindsPlayed() => fourOfKindsPlayed;
}
