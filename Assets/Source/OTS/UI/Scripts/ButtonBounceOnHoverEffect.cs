using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

using OTS.Audio;

/// <summary>
///     Component that adds a bounce animation effect when a button is hovered
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Button))]
public class ButtonBounceOnHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    [Tooltip("How much bigger than original scale when 'bouncing' above normal.")]
    public float overshootScale = 1.3f;
    [Tooltip("How large the button should settle after the bounce.")]
    public float finalScale = 1.1f;

    [Tooltip("Total time for the bounce-in animation (split between up and down).")]
    public float bounceInDuration = 0.15f;
    [Tooltip("Time to revert back to original scale on mouse exit.")]
    public float bounceOutDuration = 0.1f;

    public bool playSoundOnHover = true;

    [Header("Optional - Position Offset")]
    public Vector3 hoverOffset = new Vector3(-25f, 0f, 0f);

    private Vector3 originalScale;
    private Vector3 originalPosition;

    void Awake()
    {
        // Cache the button's original transform data.
        originalScale    = transform.localScale;
        originalPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Stop any running animation to avoid overlap.
        StopAllCoroutines();
        StartCoroutine(BounceInRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Stop any running animation to avoid overlap.
        StopAllCoroutines();
        StartCoroutine(BounceOutRoutine());
    }

    private System.Collections.IEnumerator BounceInRoutine()
    {
        // Phase 1: from current scale → overshoot scale
        float halfDuration = bounceInDuration * 0.5f;
        Vector3 overshootVector = originalScale * overshootScale;
        Vector3 finalVector = originalScale * finalScale;

        // Also move the button slightly left (or wherever hoverOffset indicates).
        Vector3 targetPosition = originalPosition + hoverOffset;
        Vector3 startPosition = transform.localPosition;

        float timer = 0f;
        Vector3 startScale = transform.localScale;

        // First half: scale up to overshoot, move toward target offset
        while (timer < halfDuration)
        {
            float t = timer / halfDuration;
            transform.localScale = Vector3.Lerp(startScale, overshootVector, t);
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = overshootVector;
        transform.localPosition = targetPosition;

        if (AudioManager.SFX != null && playSoundOnHover)
        {
            AudioManager.SFX.Play("hover_4");
        }

        // Phase 2: scale down from overshoot → final (still bigger than original)
        timer = 0f;
        while (timer < halfDuration)
        {
            float t = timer / halfDuration;
            transform.localScale = Vector3.Lerp(overshootVector, finalVector, t);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = finalVector;
    }

    private System.Collections.IEnumerator BounceOutRoutine()
    {
        // Go from current scale → original scale
        float timer = 0f;
        Vector3 startScale    = transform.localScale;
        Vector3 startPosition = transform.localPosition;

        while (timer < bounceOutDuration)
        {
            float t = timer / bounceOutDuration;
            transform.localScale    = Vector3.Lerp(startScale, originalScale, t);
            transform.localPosition = Vector3.Lerp(startPosition, originalPosition, t);
            timer += Time.deltaTime;
            yield return null;
        }

        // Snap to final values
        transform.localScale    = originalScale;
        transform.localPosition = originalPosition;
    }
}