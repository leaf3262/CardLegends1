using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class NetworkGamePlayUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playHandButton;
    [SerializeField] private Button discardButton;

    [Header("Info Displays")]
    [SerializeField] private TextMeshProUGUI localScoreText;
    [SerializeField] private TextMeshProUGUI opponentScoreText;
    [SerializeField] private TextMeshProUGUI roundInfoText;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button continueButton;

    public static NetworkGamePlayUI Instance { get; private set; }

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

        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnRoundChanged += UpdateRoundInfo;
            NetworkGameManager.Instance.OnTurnChanged += UpdateTurnIndicator;
            NetworkGameManager.Instance.OnHandPlayed += ShowHandResult;
            NetworkGameManager.Instance.OnGameEnded += ShowGameEndScreen;
        }

        if (resultPanel != null)
            resultPanel.SetActive(false);

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (playHandButton != null)
            playHandButton.onClick.RemoveListener(OnPlayHandClicked);

        if (discardButton != null)
            discardButton.onClick.RemoveListener(OnDiscardClicked);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnRoundChanged -= UpdateRoundInfo;
            NetworkGameManager.Instance.OnTurnChanged -= UpdateTurnIndicator;
            NetworkGameManager.Instance.OnHandPlayed -= ShowHandResult;
            NetworkGameManager.Instance.OnGameEnded -= ShowGameEndScreen;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnPlayHandClicked()
    {
        if (NetworkHandManager.Instance == null || NetworkGameManager.Instance == null)
            return;

        if (!NetworkGameManager.Instance.IsLocalPlayerTurn())
        {
            Debug.Log("Not your turn!");
            return;
        }

        int[] selectedIndices = NetworkHandManager.Instance.GetSelectedCardIndices();

        if (selectedIndices.Length == 0)
        {
            Debug.Log("No cards selected!");
            return;
        }

        NetworkGameManager.Instance.RequestPlayHandServerRpc(selectedIndices);
        NetworkHandManager.Instance.ClearSelection();
    }

    private void OnDiscardClicked()
    {
        if (NetworkHandManager.Instance == null || NetworkGameManager.Instance == null)
            return;

        if (!NetworkGameManager.Instance.IsLocalPlayerTurn())
        {
            Debug.Log("Not your turn!");
            return;
        }

        int[] selectedIndices = NetworkHandManager.Instance.GetSelectedCardIndices();

        if (selectedIndices.Length == 0)
        {
            Debug.Log("No cards selected!");
            return;
        }

        NetworkGameManager.Instance.RequestDiscardCardsServerRpc(selectedIndices);
        NetworkHandManager.Instance.ClearSelection();
    }

    private void OnContinueClicked()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        UpdateScores();
        UpdateButtons();
    }

    private void UpdateScores()
    {
        if (NetworkManager.Singleton == null) return;

        int localScore = 0;
        int opponentScore = 0;
        int targetScore = NetworkGameManager.Instance != null ? NetworkGameManager.Instance.GetTargetScore() : 300;

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            var playerController = client.Value.PlayerObject?.GetComponent<NetworkPlayerController>();
            if (playerController != null)
            {
                if (client.Key == NetworkManager.Singleton.LocalClientId)
                    localScore = playerController.playerScore.Value;
                else
                    opponentScore = playerController.playerScore.Value;
            }
        }

        if (localScoreText != null)
            localScoreText.text = $"Your Score: {localScore} / {targetScore}";

        if (opponentScoreText != null)
            opponentScoreText.text = $"Opponent: {opponentScore} / {targetScore}";
    }

    private void UpdateButtons()
    {
        bool isYourTurn = NetworkGameManager.Instance != null && NetworkGameManager.Instance.IsLocalPlayerTurn();

        if (playHandButton != null)
            playHandButton.interactable = isYourTurn;

        if (discardButton != null)
            discardButton.interactable = isYourTurn;
    }

    private void UpdateRoundInfo(int round, int hands)
    {
        if (roundInfoText != null && NetworkGameManager.Instance != null)
        {
            int discards = NetworkGameManager.Instance.GetDiscardsRemaining();
            roundInfoText.text = $"Round: {round}\nHands: {hands}\nDiscards: {discards}";
        }
    }

    private void UpdateTurnIndicator(ulong currentTurnPlayer)
    {
        if (turnIndicatorText == null || NetworkManager.Singleton == null) return;

        bool isYourTurn = (currentTurnPlayer == NetworkManager.Singleton.LocalClientId);

        if (isYourTurn)
            turnIndicatorText.text = "<color=green>YOUR TURN</color>";
        else
            turnIndicatorText.text = "<color=red>OPPONENT'S TURN</color>";
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

    private void ShowGameEndScreen(ulong winnerId, bool isVictory)
    {
        if (resultPanel == null || resultText == null) return;

        if (isVictory)
            resultText.text = "<size=60><color=green>VICTORY!</color></size>\n\nYou defeated your opponent!";
        else
            resultText.text = "<size=60><color=red>DEFEAT</color></size>\n\nYour opponent won!";

        resultPanel.SetActive(true);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ReturnToLobby);

            var buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "RETURN TO LOBBY";
        }
    }

    private void ReturnToLobby()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("Lobby");
    }
}
