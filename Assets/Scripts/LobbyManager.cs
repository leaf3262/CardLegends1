using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject playerListPanel;
    [SerializeField] private TextMeshProUGUI playerListContent;

    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found! Make sure it exists in the scene.");
            SetStatus("ERROR: NetworkManager missing!", Color.red);
            return;
        }

        if (hostButton != null)
            hostButton.onClick.AddListener(OnVsAIClicked);

        if (joinButton != null)
            joinButton.onClick.AddListener(OnMultiplayerClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            startGameButton.gameObject.SetActive(false);
        }

        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        SetStatus("Choose your game mode", Color.white);

        if (playerListPanel != null)
            playerListPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (hostButton != null)
            hostButton.onClick.RemoveListener(OnVsAIClicked);

        if (joinButton != null)
            joinButton.onClick.RemoveListener(OnMultiplayerClicked);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);

        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);

        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnVsAIClicked()
    {
        Debug.Log("Starting VS AI mode...");
        SetStatus("Starting AI game...", Color.green);

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("GamePlay");
        }
    }

    private void OnMultiplayerClicked()
    {
        Debug.Log("Entering multiplayer lobby...");

        if (connectionPanel != null)
            connectionPanel.SetActive(false);

        ShowMultiplayerOptions();
    }

    private void ShowMultiplayerOptions()
    {
        if (playerListPanel != null)
            playerListPanel.SetActive(true);

        SetStatus("Host or Join a game", Color.white);

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);
    }

    private void OnBackClicked()
    {
        if (networkManager != null && networkManager.IsListening)
            networkManager.Shutdown();

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("MainMenu");
    }

    private void OnStartGameClicked()
    {
        if (!networkManager.IsHost)
        {
            Debug.LogWarning("Only host can start the game!");
            return;
        }

        Debug.Log("Host starting game...");
        SetStatus("Starting game...", Color.green);
        networkManager.SceneManager.LoadScene("GamePlay", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");

        if (networkManager.IsHost)
            SetStatus($"Player joined! ({networkManager.ConnectedClients.Count} players)", Color.green);
        else if (clientId == networkManager.LocalClientId)
        {
            SetStatus("Connected! Waiting for host to start...", Color.green);
            ShowPlayerList();
        }

        UpdatePlayerList();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");

        if (networkManager.IsHost)
            SetStatus($"Player left. ({networkManager.ConnectedClients.Count} players)", Color.yellow);

        UpdatePlayerList();
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log($"[Lobby] {message}");
    }

    public void OnHostMultiplayerClicked()
    {
        Debug.Log("Starting as Host...");
        SetStatus("Starting server...", Color.yellow);

        bool success = networkManager.StartHost();

        if (success)
        {
            SetStatus("Hosting! Waiting for players...", Color.green);

            if (startGameButton != null)
                startGameButton.gameObject.SetActive(true);
        }
        else
        {
            SetStatus("Failed to start host!", Color.red);
        }
    }

    public void OnJoinMultiplayerClicked()
    {
        Debug.Log("Joining as Client...");
        SetStatus("Connecting to host...", Color.yellow);

        bool success = networkManager.StartClient();

        if (success)
            SetStatus("Connecting...", Color.yellow);
        else
            SetStatus("Failed to connect!", Color.red);
    }

    private void ShowPlayerList()
    {
        if (connectionPanel != null)
            connectionPanel.SetActive(false);

        if (playerListPanel != null)
            playerListPanel.SetActive(true);

        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        if (playerListContent == null || networkManager == null) return;

        string playerList = "";
        int playerNumber = 1;

        foreach (var kvp in networkManager.ConnectedClients)
        {
            ulong clientId = kvp.Key;
            string role = (clientId == networkManager.LocalClientId && networkManager.IsHost) ? "Host" : "Client";
            playerList += $"Player {playerNumber}: {role} (ID: {clientId})\n";
            playerNumber++;
        }

        if (string.IsNullOrEmpty(playerList))
            playerList = "No players connected";

        playerListContent.text = playerList;
    }
}
