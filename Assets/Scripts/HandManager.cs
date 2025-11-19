using System.Collections.Generic;
using UnityEngine;


public class HandManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHandSize = 5;
    [SerializeField] private float cardSpacing = 130f;

    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;

    private List<Card> cardsInHand = new List<Card>();
    private Card selectedCard = null;

    public static HandManager Instance { get; private set; }

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
        DrawInitialHand();
    }

    public void DrawInitialHand()
    {
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.Log("Hand is full!");
            return;
        }

        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager not found!");
            return;
        }

        CardData cardData = DeckManager.Instance.DrawCard();

        if (cardData == null)
        {
            Debug.Log("No more cards to draw");
            return;
        } 
        GameObject cardObj = Instantiate(cardPrefab);

        if (cardObj != null && handContainer != null)
        {
      
            cardObj.transform.SetParent(handContainer, false);

            Card card = cardObj.GetComponent<Card>();

            if (card != null)
            {
                card.Initialize(cardData);
                card.OnCardClicked += OnCardClicked;
                cardsInHand.Add(card);

                RefreshHandLayout();
            }
        }
    }
    private void OnCardClicked(Card clickedCard)
    {
        if (clickedCard.IsSelected)
        {
            clickedCard.SetSelected(false);
            Debug.Log($"Deselected: {clickedCard.CardData.GetDisplayName()}");
        }
        else
        {
            clickedCard.SetSelected(true);
            Debug.Log($"Selected: {clickedCard.CardData.GetDisplayName()}");
        }
    }
    public void DiscardSelectedCard()
    {
        if (selectedCard == null)
        {
            Debug.Log("No card selected to discard");
            return;
        }

        DeckManager.Instance.DiscardCard(selectedCard.CardData);
        cardsInHand.Remove(selectedCard);
        Destroy(selectedCard.gameObject);
        selectedCard = null;

        DrawCard();
        RefreshHandLayout();
    }

    private void RefreshHandLayout()
    {
        int cardCount = cardsInHand.Count;
        float totalWidth = (cardCount - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            RectTransform cardRect = cardsInHand[i].GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchoredPosition = new Vector2(startX + (i * cardSpacing), 0);
            }
        }
    }

    public List<Card> GetCardsInHand()
    {
        return new List<Card>(cardsInHand);
    }
    public void RemoveCardFromHand(Card card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            Destroy(card.gameObject);
            RefreshHandLayout();
        }
    }

    public void ClearHand()
    {
        foreach (Card card in cardsInHand)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        cardsInHand.Clear();
    }

    public List<Card> GetSelectedCards()
    {
        List<Card> selected = new List<Card>();

        Debug.Log($"[HandManager] Checking {cardsInHand.Count} cards in hand");

        foreach (Card card in cardsInHand)
        {
            if (card != null)
            {
                Debug.Log($"  Card: {card.CardData.GetDisplayName()} - Selected: {card.IsSelected}");

                if (card.IsSelected)
                {
                    selected.Add(card);
                }
            }
        }
        Debug.Log($"[HandManager] Found {selected.Count} selected cards");
        return selected;
    }
}
