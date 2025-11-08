using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private List<CardData> allCards = new List<CardData>();

    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    public static DeckManager Instance { get; private set; }

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
        InitializeDeck();
    }

    public void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        foreach (CardData card in allCards)
        {
            if (card != null)
            {
                drawPile.Add(card);
            }
        }

        ShuffleDeck();
        Debug.Log($"Deck initialized with {drawPile.Count} cards");
    }

    public void ShuffleDeck()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }

        Debug.Log("Deck shuffled");
    }

    public CardData DrawCard()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                Debug.LogWarning("No cards left to draw!");
                return null;
            }

            Debug.Log("Deck empty - shuffling discard pile back in");
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }

        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);

        Debug.Log($"Drew card: {drawnCard.GetDisplayName()}");
        return drawnCard;
    }

    public void DiscardCard(CardData card)
    {
        if (card != null)
        {
            discardPile.Add(card);
            Debug.Log($"Discarded: {card.GetDisplayName()}");
        }
    }

    public int GetDrawPileCount()
    {
        return drawPile.Count;
    }

    public int GetDiscardPileCount()
    {
        return discardPile.Count;
    }
}
