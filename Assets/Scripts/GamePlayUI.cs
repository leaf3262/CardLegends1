using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GamePlayUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playHandButton;
    [SerializeField] private Button discardButton;

    [Header("Info Displays")]
    [SerializeField] private TextMeshProUGUI deckInfoText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roundInfoText;
    [SerializeField] private TextMeshProUGUI localScoreText;
    [SerializeField] private TextMeshProUGUI opponentScoreText;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button continueButton;

    public static GamePlayUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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

        if (AIGameManager.Instance != null)
        {
            AIGameManager.Instance.OnPlayerScoreChanged += UpdatePlayerScore;
            AIGameManager.Instance.OnAIScoreChanged += UpdateAIScore;
            AIGameManager.Instance.OnRoundChanged += UpdateRoundInfo;
            AIGameManager.Instance.OnHandPlayed += ShowHandResult;
            AIGameManager.Instance.OnGameEnded += ShowGameEndScreen;
            AIGameManager.Instance.OnTurnChanged += UpdateTurnIndicator;
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

        if (AIGameManager.Instance != null)
        {
            AIGameManager.Instance.OnPlayerScoreChanged -= UpdatePlayerScore;
            AIGameManager.Instance.OnAIScoreChanged -= UpdateAIScore;
            AIGameManager.Instance.OnRoundChanged -= UpdateRoundInfo;
            AIGameManager.Instance.OnHandPlayed -= ShowHandResult;
            AIGameManager.Instance.OnGameEnded -= ShowGameEndScreen;
            AIGameManager.Instance.OnTurnChanged -= UpdateTurnIndicator;
        }
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

    private void OnPlayHandClicked()
    {
        Debug.Log("=== PLAY HAND BUTTON CLICKED ===");

        if (AIGameManager.Instance != null)
        {
            Debug.Log("AI Mode detected");

            if (HandManager.Instance != null)
            {
                Debug.Log("HandManager found, getting selected cards...");

                List<Card> selectedCards = HandManager.Instance.GetSelectedCards();

                if (selectedCards.Count == 0)
                {
                    Debug.Log("ERROR: No cards selected to play!");
                    return;
                }

                Debug.Log($"SUCCESS: Playing {selectedCards.Count} cards");
                AIGameManager.Instance.PlayerPlayHand(selectedCards);
            }
            else
            {
                Debug.LogError("HandManager.Instance is NULL!");
            }
        }
        else if (RoundManager.Instance != null)
        {
            Debug.Log("RoundManager mode detected");
            RoundManager.Instance.PlaySelectedCards();
        }
        else
        {
            Debug.LogError("NO GAME MANAGER FOUND!");
        }
    }

    private void OnDiscardClicked()
    {
        if (AIGameManager.Instance != null)
        {
            if (HandManager.Instance != null)
            {
                List<Card> selectedCards = HandManager.Instance.GetSelectedCards();
                AIGameManager.Instance.PlayerDiscardCards(selectedCards);
            }
        }
        else if (RoundManager.Instance != null)
        {
            RoundManager.Instance.DiscardSelectedCards();
        }
    }

    private void OnContinueClicked()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void UpdateScoreDisplay(int currentScore)
    {
        if (scoreText != null && RoundManager.Instance != null)
        {
            int targetScore = RoundManager.Instance.GetTargetScore();
            scoreText.text = $"Score: {currentScore} / {targetScore}";
        }
    }

    private void UpdatePlayerScore(int score)
    {
        if (localScoreText != null && AIGameManager.Instance != null)
        {
            int target = AIGameManager.Instance.GetTargetScore();
            localScoreText.text = $"Your Score: {score} / {target}";
        }
    }

    private void UpdateAIScore(int score)
    {
        if (opponentScoreText != null && AIGameManager.Instance != null)
        {
            int target = AIGameManager.Instance.GetTargetScore();
            opponentScoreText.text = $"AI Score: {score} / {target}";
        }
    }

    private void UpdateRoundInfo(int round, int handsRemaining)
    {
        if (roundInfoText != null)
        {
            int discards = 0;

            if (RoundManager.Instance != null)
            {
                discards = RoundManager.Instance.GetDiscardsRemaining();
            }
            else if (AIGameManager.Instance != null)
            {
                discards = AIGameManager.Instance.GetDiscardsRemaining();
            }

            roundInfoText.text = $"Round: {round}\nHands: {handsRemaining}\nDiscards: {discards}";
        }
    }

    private void UpdateTurnIndicator(bool isPlayerTurn)
    {
        if (turnIndicatorText != null)
        {
            if (isPlayerTurn)
            {
                turnIndicatorText.text = "<color=green>YOUR TURN</color>";
            }
            else
            {
                turnIndicatorText.text = "<color=red>AI THINKING...</color>";
            }
        }

        if (playHandButton != null)
            playHandButton.interactable = isPlayerTurn;

        if (discardButton != null)
            discardButton.interactable = isPlayerTurn;
    }

    private void ShowHandResult(HandResult result)
    {
        if (resultPanel == null || resultText == null) return;

        if (StatsManager.Instance != null)
        {
            if (result.handType == HandType.Flush ||
                result.handType == HandType.Straight ||
                result.handType == HandType.FourOfAKind ||
                result.handType == HandType.StraightFlush ||
                result.handType == HandType.RoyalFlush)
            {
                StatsManager.Instance.RecordSpecialHand(result.handType);
            }
        }

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
                             $"Final Score: {(RoundManager.Instance != null ? RoundManager.Instance.GetCurrentScore() : AIGameManager.Instance.GetPlayerScore())}";
        }
        else
        {
            resultText.text = "<size=60><color=red>DEFEAT</color></size>\n\n" +
                             $"You ran out of rounds!\n" +
                             $"Final Score: {(RoundManager.Instance != null ? RoundManager.Instance.GetCurrentScore() : AIGameManager.Instance.GetPlayerScore())}";
        }

        resultPanel.SetActive(true);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ReturnToMainMenu);

            var buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "MAIN MENU";
            }
        }
    }

    private void ReturnToMainMenu()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("MainMenu");
        }
    }
}
