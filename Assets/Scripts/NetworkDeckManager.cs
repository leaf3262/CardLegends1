using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class NetworkDeckManager : NetworkBehaviour
{
    [Header("Card Data")]
    [SerializeField] private List<CardData> allCards = new List<CardData>();

    private List<int> drawPile = new List<int>();
    private List<int> discardPile = new List<int>();

    public static NetworkDeckManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeDeck();
        }
        base.OnNetworkSpawn();
    }

    private void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        for (int i = 0; i < allCards.Count; i++)
        {
            if (allCards[i] != null)
            {
                drawPile.Add(i);
            }
        }

        ShuffleDeck();
        Debug.Log($"[Server] Deck initialized with {drawPile.Count} cards");
    }

    private void ShuffleDeck()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }

        Debug.Log("[Server] Deck shuffled");
    }

    public NetworkCardData DrawCard()
    {
        if (!IsServer)
        {
            Debug.LogError("Only server can draw cards!");
            return default;
        }

        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                Debug.LogWarning("[Server] No cards left!");
                return default;
            }

            drawPile.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }

        int cardIndex = drawPile[0];
        drawPile.RemoveAt(0);

        CardData cardData = allCards[cardIndex];
        NetworkCardData networkCard = new NetworkCardData(cardData, cardIndex);

        Debug.Log($"[Server] Drew card: {cardData.GetDisplayName()}");
        return networkCard;
    }

    public void DiscardCard(int cardIndex)
    {
        if (!IsServer) return;

        discardPile.Add(cardIndex);
        Debug.Log($"[Server] Discarded card index: {cardIndex}");
    }

    public CardData GetCardDataFromIndex(int index)
    {
        if (index >= 0 && index < allCards.Count)
        {
            return allCards[index];
        }
        return null;
    }

    public int GetDrawPileCount() => drawPile.Count;
    public int GetDiscardPileCount() => discardPile.Count;
}
