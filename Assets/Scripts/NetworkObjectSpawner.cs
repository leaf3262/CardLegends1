using Unity.Netcode;
using UnityEngine;

public class NetworkObjectSpawner : MonoBehaviour
{
    [Header("Network Prefabs")]
    [SerializeField] private GameObject networkDeckManagerPrefab;
    [SerializeField] private GameObject networkGameManagerPrefab;

    private void Start()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            SpawnNetworkObjects();
        }
    }

    private void SpawnNetworkObjects()
    {
        if (NetworkDeckManager.Instance == null && networkDeckManagerPrefab != null)
        {
            GameObject deckManager = Instantiate(networkDeckManagerPrefab);
            NetworkObject netObj = deckManager.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
                Debug.Log("[Host] Spawned NetworkDeckManager");
            }
        }

        if (NetworkGameManager.Instance == null && networkGameManagerPrefab != null)
        {
            GameObject gameManager = Instantiate(networkGameManagerPrefab);
            NetworkObject netObj = gameManager.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
                Debug.Log("[Host] Spawned NetworkGameManager");
            }
        }
    }
}
