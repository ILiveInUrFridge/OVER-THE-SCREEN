using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SpriteChangeOnHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprite Settings")]
    [Tooltip("The sprite to show when hovering")]
    public Sprite hoverSprite;

    [Header("Animation")]
    [Tooltip("How long the fade transition should take")]
    public float fadeDuration = 0.1f;

    private Image hoverImage;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (hoverSprite == null)
        {
            Debug.LogError("Hover sprite is not assigned!", this);
            return;
        }

        // Create hover image overlay
        GameObject hoverObj = new GameObject("Hover Overlay");
        hoverObj.transform.SetParent(transform, false);
        
        // Setup the RectTransform to cover the entire button
        RectTransform hoverRect = hoverObj.AddComponent<RectTransform>();
        hoverRect.anchorMin = Vector2.zero;
        hoverRect.anchorMax = Vector2.one;
        hoverRect.offsetMin = Vector2.zero;
        hoverRect.offsetMax = Vector2.zero;

        // Setup the hover image
        hoverImage = hoverObj.AddComponent<Image>();
        hoverImage.sprite = hoverSprite;
        hoverImage.color = new Color(1, 1, 1, 0);
        hoverImage.raycastTarget = false;
    }

    void OnEnable()
    {
        // Reset the hover image when enabled
        if (hoverImage != null)
        {
            hoverImage.color = new Color(1, 1, 1, 0);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverImage != null && hoverSprite != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeHoverImage(1f));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverImage != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeHoverImage(0f));
        }
    }

    private System.Collections.IEnumerator FadeHoverImage(float targetAlpha)
    {
        float startAlpha = hoverImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            Color newColor = hoverImage.color;
            newColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            hoverImage.color = newColor;
            
            yield return null;
        }

        // Ensure we end at exactly the target alpha
        Color finalColor = hoverImage.color;
        finalColor.a = targetAlpha;
        hoverImage.color = finalColor;
    }
}
