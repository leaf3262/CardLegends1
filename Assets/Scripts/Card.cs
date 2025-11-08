using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Data")]
    [SerializeField] private CardData cardData;

    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI suitText;
    [SerializeField] private TextMeshProUGUI cardNameText;

    [Header("Settings")]
    [SerializeField] private float hoverScale = 1.1f;

    private Button button;
    private Vector3 originalScale;
    private bool isSelected = false;

    public System.Action<Card> OnCardClicked;

    public CardData CardData => cardData;
    public bool IsSelected => isSelected;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;

        if (button != null)
        {
            button.onClick.AddListener(HandleCardClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleCardClick);
        }
    }

    public void Initialize(CardData data)
    {
        cardData = data;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (cardData == null) return;

        if (backgroundImage != null)
        {
            backgroundImage.color = cardData.cardColor;
        }

        if (cardData.cardType == CardType.Regular)
        {
            if (rankText != null)
            {
                rankText.text = GetRankSymbol(cardData.rank);
                rankText.gameObject.SetActive(true);
            }

            if (suitText != null)
            {
                suitText.text = GetSuitSymbol(cardData.suit);
                suitText.gameObject.SetActive(true);
            }

            if (cardNameText != null)
            {
                cardNameText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (rankText != null)
                rankText.gameObject.SetActive(false);

            if (suitText != null)
                suitText.gameObject.SetActive(false);

            if (cardNameText != null)
            {
                cardNameText.text = cardData.cardName;
                cardNameText.gameObject.SetActive(true);
            }
        }
    }

    private string GetRankSymbol(CardRank rank)
    {
        switch (rank)
        {
            case CardRank.Ace: return "A";
            case CardRank.King: return "K";
            case CardRank.Queen: return "Q";
            case CardRank.Jack: return "J";
            default: return ((int)rank).ToString();
        }
    }

    private string GetSuitSymbol(CardSuit suit)
    {
        switch (suit)
        {
            case CardSuit.Hearts: return "?";
            case CardSuit.Diamonds: return "?";
            case CardSuit.Clubs: return "?";
            case CardSuit.Spades: return "?";
            default: return "";
        }
    }

    private void HandleCardClick()
    {
        Debug.Log($"Card clicked: {cardData.GetDisplayName()}");
        OnCardClicked?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 30f, transform.localPosition.z);

            if (cardImage != null)
            {
                cardImage.color = new Color(1f, 1f, 0.7f);
            }
        }
        else
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);

            if (cardImage != null)
            {
                cardImage.color = Color.white;
            }
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}
