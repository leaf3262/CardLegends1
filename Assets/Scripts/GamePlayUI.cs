using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playHandButton;
    [SerializeField] private Button discardButton;

    [Header("Info Displays")]
    [SerializeField] private TextMeshProUGUI deckInfoText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roundInfoText;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button continueButton;

    private void Start()
    {
        if (playHandButton != null)
            playHandButton.onClick.AddListener(OnPlayHandClicked);

        if (discardButton != null)
            discardButton.onClick.AddListener(OnDiscardClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            RoundManager.Instance.OnRoundChanged += UpdateRoundInfo;
            RoundManager.Instance.OnHandPlayed += ShowHandResult;
            RoundManager.Instance.OnGameEnded += ShowGameEndScreen;
        }

        if (resultPanel != null)
            resultPanel.SetActive(false);

        UpdateScoreDisplay(0);
        UpdateRoundInfo(1, 3);
    }

    private void OnDestroy()
    {
        if (playHandButton != null)
            playHandButton.onClick.RemoveListener(OnPlayHandClicked);

        if (discardButton != null)
            discardButton.onClick.RemoveListener(OnDiscardClicked);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            RoundManager.Instance.OnRoundChanged -= UpdateRoundInfo;
            RoundManager.Instance.OnHandPlayed -= ShowHandResult;
            RoundManager.Instance.OnGameEnded -= ShowGameEndScreen;
        }
    }

    private void OnPlayHandClicked()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.PlaySelectedCards();
    }

    private void OnDiscardClicked()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.DiscardSelectedCards();
    }

    private void OnContinueClicked()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void UpdateScoreDisplay(int currentScore)
    {
        if (scoreText != null && RoundManager.Instance != null)
        {
            int targetScore = RoundManager.Instance.GetTargetScore();
            scoreText.text = $"Score: {currentScore} / {targetScore}";
        }
    }

    private void UpdateRoundInfo(int round, int handsRemaining)
    {
        if (roundInfoText != null && RoundManager.Instance != null)
        {
            int discards = RoundManager.Instance.GetDiscardsRemaining();
            roundInfoText.text = $"Round: {round}\nHands: {handsRemaining}\nDiscards: {discards}";
        }
    }

    private void ShowHandResult(HandResult result)
    {
        if (resultPanel == null || resultText == null) return;

        string resultMessage = $"{result.description}\n\n";
        resultMessage += $"Base: {result.baseScore}\n";
        resultMessage += $"Multiplier: {result.multiplier}x\n";
        resultMessage += $"<size=48><color=yellow>Score: {result.finalScore}</color></size>";

        resultText.text = resultMessage;
        resultPanel.SetActive(true);
    }

    private void ShowGameEndScreen(bool victory)
    {
        if (resultPanel == null || resultText == null) return;

        if (victory)
        {
            resultText.text = "<size=60><color=green>VICTORY!</color></size>\n\n" +
                             $"You reached the target score!\n" +
                             $"Final Score: {RoundManager.Instance.GetCurrentScore()}";
        }
        else
        {
            resultText.text = "<size=60><color=red>DEFEAT</color></size>\n\n" +
                             $"You ran out of rounds!\n" +
                             $"Final Score: {RoundManager.Instance.GetCurrentScore()}";
        }

        resultPanel.SetActive(true);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ReturnToMainMenu);

            var buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "MAIN MENU";
        }
    }

    private void ReturnToMainMenu()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("MainMenu");
    }
    private void Update()
    {
        if (deckInfoText != null && DeckManager.Instance != null)
        {
            int drawPile = DeckManager.Instance.GetDrawPileCount();
            int discardPile = DeckManager.Instance.GetDiscardPileCount();
            deckInfoText.text = $"Deck: {drawPile}\nDiscard: {discardPile}";
        }
    }

}
