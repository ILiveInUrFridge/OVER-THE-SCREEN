using UnityEngine;
using System.Collections.Generic;

/// <summary>
///     Randomly changes the sprite, without repeating the same sprite twice in a row.
///     Usually used to test mouth animations.
/// </summary>
public class ImageRandomizer : MonoBehaviour
{
    public List<Sprite> sprites;
    public float intervalMilliseconds = 110; // Time between changes
    private SpriteRenderer spriteRenderer;
    private int lastIndex = -1;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogError("No sprites assigned!");
            enabled = false;
            return;
        }

        InvokeRepeating(nameof(ChangeSprite), 0f, intervalMilliseconds / 1000f);
    }

    private void ChangeSprite()
    {
        if (sprites.Count == 1)
        {
            spriteRenderer.sprite = sprites[0];
            return;
        }

        int newIndex;
        do
        {
            newIndex = Random.Range(0, sprites.Count);
        } while (newIndex == lastIndex);

        lastIndex = newIndex;
        spriteRenderer.sprite = sprites[newIndex];
    }
}
