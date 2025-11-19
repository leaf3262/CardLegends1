using UnityEngine;
using System.Collections;

public class CardAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float drawDuration = 0.5f;
    [SerializeField] private float hoverUpDistance = 40f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float selectUpDistance = 30f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private bool isAnimating = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
            originalScale = transform.localScale;
        }
    }

    public void PlayDrawAnimation(Vector2 startPosition)
    {
        if (rectTransform == null) return;

        rectTransform.anchoredPosition = startPosition;
        transform.localScale = Vector3.zero;

        StartCoroutine(AnimateDrawCoroutine());
    }

    private IEnumerator AnimateDrawCoroutine()
    {
        isAnimating = true;
        float elapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = transform.localScale;

        while (elapsed < drawDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / drawDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, easedT);
            transform.localScale = Vector3.Lerp(startScale, originalScale, easedT);

            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        transform.localScale = originalScale;
        isAnimating = false;
    }

    public void AnimateHover(bool isHovering)
    {
        if (isAnimating) return;

        StopAllCoroutines();
        StartCoroutine(AnimateHoverCoroutine(isHovering));
    }

    private IEnumerator AnimateHoverCoroutine(bool hover)
    {
        float targetY = hover ? originalPosition.y + hoverUpDistance : originalPosition.y;
        float startY = rectTransform.anchoredPosition.y;
        float elapsed = 0f;

        while (elapsed < hoverDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hoverDuration;
            float easedT = hover ? Mathf.Sin(t * Mathf.PI * 0.5f) : 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = Mathf.Lerp(startY, targetY, easedT);
            rectTransform.anchoredPosition = pos;

            yield return null;
        }

        Vector2 finalPos = rectTransform.anchoredPosition;
        finalPos.y = targetY;
        rectTransform.anchoredPosition = finalPos;
    }

    public void AnimateSelection(bool selected)
    {
        if (isAnimating) return;

        StopAllCoroutines();
        StartCoroutine(AnimateSelectionCoroutine(selected));
    }

    private IEnumerator AnimateSelectionCoroutine(bool selected)
    {
        float targetY = selected ? originalPosition.y + selectUpDistance : originalPosition.y;
        float startY = rectTransform.anchoredPosition.y;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = selected ? Mathf.Sin(t * Mathf.PI * 0.5f) : 1f - Mathf.Pow(1f - t, 2f);

            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = Mathf.Lerp(startY, targetY, easedT);
            rectTransform.anchoredPosition = pos;

            yield return null;
        }

        Vector2 finalPos = rectTransform.anchoredPosition;
        finalPos.y = targetY;
        rectTransform.anchoredPosition = finalPos;
    }

    public void PlayCardAnimation(System.Action onComplete = null)
    {
        StartCoroutine(PlayCardCoroutine(onComplete));
    }

    private IEnumerator PlayCardCoroutine(System.Action onComplete)
    {
        isAnimating = true;
        float duration = 0.4f;
        float elapsed = 0f;

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, startPos.y + 200f);
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t * t);

            yield return null;
        }

        isAnimating = false;
        onComplete?.Invoke();
    }

    public void SetOriginalPosition(Vector2 position)
    {
        originalPosition = position;
    }
}
