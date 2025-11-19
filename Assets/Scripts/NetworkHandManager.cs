using System.Collections.Generic;
using UnityEngine;

public class NetworkHandManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cardSpacing = 130f;

    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;

    private List<Card> cardsInHand = new List<Card>();
    private List<Card> selectedCards = new List<Card>();

    public static NetworkHandManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddCardToHand(NetworkCardData networkCardData)
    {
        if (NetworkDeckManager.Instance == null)
        {
            Debug.LogError("NetworkDeckManager not found!");
            return;
        }

        CardData cardData = NetworkDeckManager.Instance.GetCardDataFromIndex(networkCardData.cardIndex);
        if (cardData == null)
        {
            Debug.LogError($"CardData not found for index {networkCardData.cardIndex}");
            return;
        }

        GameObject cardObj = Instantiate(cardPrefab, handContainer);
        Card card = cardObj.GetComponent<Card>();

        if (card != null)
        {
            card.Initialize(cardData);
            card.OnCardClicked += OnCardClicked;
            cardsInHand.Add(card);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCardDraw();
            }

            RefreshHandLayout();
            Debug.Log($"Added card to hand: {cardData.GetDisplayName()}");
        }
    }

    private void OnCardClicked(Card clickedCard)
    {
        if (selectedCards.Contains(clickedCard))
        {
            selectedCards.Remove(clickedCard);
            clickedCard.SetSelected(false);
            Debug.Log($"Deselected: {clickedCard.CardData.GetDisplayName()}");
        }
        else
        {
            selectedCards.Add(clickedCard);
            clickedCard.SetSelected(true);
            Debug.Log($"Selected: {clickedCard.CardData.GetDisplayName()}");
        }
    }

    public int[] GetSelectedCardIndices()
    {
        List<int> indices = new List<int>();

        foreach (Card selectedCard in selectedCards)
        {
            int index = cardsInHand.IndexOf(selectedCard);
            if (index >= 0)
            {
                indices.Add(index);
            }
        }

        return indices.ToArray();
    }

    public void RemoveCardsAtIndices(int[] indices)
    {
        System.Array.Sort(indices);
        System.Array.Reverse(indices);

        foreach (int index in indices)
        {
            if (index >= 0 && index < cardsInHand.Count)
            {
                Card card = cardsInHand[index];
                cardsInHand.RemoveAt(index);
                selectedCards.Remove(card);
                Destroy(card.gameObject);
            }
        }

        RefreshHandLayout();
    }

    public void ClearSelection()
    {
        foreach (Card card in selectedCards)
        {
            card.SetSelected(false);
        }
        selectedCards.Clear();
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
                Vector2 targetPos = new Vector2(startX + (i * cardSpacing), 0);
                cardRect.anchoredPosition = targetPos;
            }
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
        selectedCards.Clear();
    }

    public int GetHandCount() => cardsInHand.Count;
    public int GetSelectedCount() => selectedCards.Count;
}
