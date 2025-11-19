using UnityEngine;
using Unity.Netcode;

public class GameModeManager : MonoBehaviour
{
    public enum GameMode
    {
        AI,
        Multiplayer
    }

    [SerializeField] private GameMode currentMode = GameMode.AI;
    [SerializeField] private GameObject aiGameManagerPrefab;
    [SerializeField] private GameObject roundManagerPrefab;

    public static GameModeManager Instance { get; private set; }
    public GameMode CurrentMode => currentMode;

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
        DetectGameMode();
        InitializeGameMode();
    }

    private void DetectGameMode()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            currentMode = GameMode.Multiplayer;
            Debug.Log("[GameMode] Detected: MULTIPLAYER");
        }
        else
        {
            currentMode = GameMode.AI;
            Debug.Log("[GameMode] Detected: AI MODE");
        }
    }

    private void InitializeGameMode()
    {
        if (currentMode == GameMode.AI)
            InitializeAIMode();
        else
            InitializeMultiplayerMode();
    }

    private void InitializeAIMode()
    {
        Debug.Log("[GameMode] Initializing AI Mode...");

        if (DeckManager.Instance == null && roundManagerPrefab == null)
        {
            GameObject deckObj = new GameObject("DeckManager");
            deckObj.AddComponent<DeckManager>();
        }

        if (AIGameManager.Instance == null)
        {
            GameObject aiManagerObj = new GameObject("AIGameManager");
            aiManagerObj.AddComponent<AIGameManager>();
        }

        if (AIPlayer.Instance == null)
        {
            GameObject aiPlayerObj = new GameObject("AIPlayer");
            aiPlayerObj.AddComponent<AIPlayer>();
        }

        DisableNetworkComponents();
    }

    private void InitializeMultiplayerMode()
    {
        Debug.Log("[GameMode] Initializing Multiplayer Mode...");
        DisableAIComponents();
    }

    private void DisableNetworkComponents()
    {
        if (NetworkHandManager.Instance != null)
            NetworkHandManager.Instance.gameObject.SetActive(false);

        if (NetworkGamePlayUI.Instance != null)
            NetworkGamePlayUI.Instance.gameObject.SetActive(false);
    }

    private void DisableAIComponents()
    {
        if (AIGameManager.Instance != null)
            AIGameManager.Instance.gameObject.SetActive(false);

        if (AIPlayer.Instance != null)
            AIPlayer.Instance.gameObject.SetActive(false);

        if (HandManager.Instance != null)
            HandManager.Instance.gameObject.SetActive(false);

        if (GamePlayUI.Instance != null)
            GamePlayUI.Instance.gameObject.SetActive(false);
    }
}
