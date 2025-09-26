using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using OTS.Model.Controller.Data;

namespace OTS.Model.Controller
{
    /// <summary>
    ///     Controller for the eyes of a Purrine sprite.
    /// </summary>
    public class EyeController : MonoBehaviour, ILoggable
    {
        [Header("Eye SpriteRenderer Object")]
        public SpriteRenderer eyeRenderer;
        
        [Header("Blinking Configuration")]
        [SerializeField] private bool enableBlinking = true;
        [SerializeField] private Vector2 blinkIntervalRange = new(3f, 4f); // range for random blink interval
        [SerializeField] private float doubleBinkChance = 0.3f; // 30% chance for double blink
        
        [Header("Eye Emotion")]
        [SerializeField] private EyeEmotion currentEmotion = EyeEmotion.DEFAULT;

        private SpriteController spriteController;
        private Coroutine blinkCoroutine;
        private Coroutine blinkTimerCoroutine;
        private readonly Dictionary<EyeEmotion, List<Sprite>> emotionSprites = new();
        private bool isBlinking = false;
        private bool isManuallyClosed = false;
        
        // Frame timing constants (converted from 30FPS to seconds)
        private const float FRAME_TIME = 1f / 30f;
        
        // Single blink timing
        private readonly float[] singleBlinkFrameTimes = { 3f * FRAME_TIME, 2f * FRAME_TIME, 2f * FRAME_TIME, 1f * FRAME_TIME, 1f * FRAME_TIME };
        
        // Double blink timing (first blink is twice as fast)
        private readonly float[] doubleBlinkFirstFrameTimes = { 1.5f * FRAME_TIME, 1f * FRAME_TIME, 1f * FRAME_TIME, 0.5f * FRAME_TIME, 0.5f * FRAME_TIME, 1f * FRAME_TIME };
        private readonly float[] doubleBlinkSecondFrameTimes = { 3f * FRAME_TIME, 2f * FRAME_TIME, 2f * FRAME_TIME, 1f * FRAME_TIME, 1f * FRAME_TIME };
        
        // Frame indices for blinking sequence: 0, 50, 80, 110, 105, 100
        private readonly int[] blinkFrameSequence = { 0, 50, 80, 110, 105, 100 };

        /// <summary>
        ///     Initialize the eye controller with sprite controller reference
        /// </summary>
        public void Initialize(SpriteController controller)
        {
            spriteController = controller;
            LoadEyeSprites();
            SetEmotion(currentEmotion);
            
            if (enableBlinking)
            {
                StartBlinkTimer();
            }
        }
        
        /// <summary>
        ///     Load all eye sprites for all available emotions
        /// </summary>
        private void LoadEyeSprites()
        {
            if (spriteController == null) return;
            
            string spriteName = spriteController.GetSpriteName();
            
            // Load sprites for each emotion
            foreach (EyeEmotion emotion in System.Enum.GetValues(typeof(EyeEmotion)))
            {
                List<Sprite> sprites = LoadSpritesForEmotion(spriteName, emotion);
                emotionSprites[emotion] = sprites;
                
                this.Log($"Loaded {sprites.Count} sprites for {emotion} emotion");
            }
        }
        
        /// <summary>
        ///     Load sprites for a specific emotion
        /// </summary>
        private List<Sprite> LoadSpritesForEmotion(string spriteName, EyeEmotion emotion)
        {
            string emotionName = emotion.ToString().Replace("_", " ").ToLower();
            emotionName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(emotionName);
            
            string basePath = $"Game/Model/{spriteName}/Facial/Eye/{emotionName}";
            
            List<Sprite> sprites = new();
            
            // Try to load each frame in the blink sequence
            foreach (int frameIndex in blinkFrameSequence)
            {
                string spritePath = $"{basePath}/{frameIndex}";
                Sprite sprite = Resources.Load<Sprite>(spritePath);
                
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }
            
            // If no specific frames found, try to load any sprites in the directory
            if (sprites.Count == 0)
            {
                Sprite[] allSprites = Resources.LoadAll<Sprite>(basePath);
                sprites.AddRange(allSprites);
            }
            
            return sprites;
        }
        
        /// <summary>
        ///     Set the current eye emotion
        /// </summary>
        public void SetEmotion(EyeEmotion emotion)
        {
            currentEmotion = emotion;
            
            // If eyes are manually closed, keep them at the closed frame (index 0)
            if (isManuallyClosed)
            {
                if (emotionSprites.ContainsKey(emotion) && emotionSprites[emotion].Count > 0)
                {
                    eyeRenderer.sprite = emotionSprites[emotion][0];
                }
                return;
            }
            
            if (emotionSprites.ContainsKey(emotion) && emotionSprites[emotion].Count > 0)
            {
                // Set to the open eye frame (frame 100, which should be the last in sequence)
                List<Sprite> sprites = emotionSprites[emotion];
                if (sprites.Count > 1)
                {
                    // Use the last sprite as the open eye (frame 100)
                    eyeRenderer.sprite = sprites[^1];
                }
                else
                {
                    // Single sprite emotion (like SMILE)
                    eyeRenderer.sprite = sprites[0];
                }
            }
        }
        
        /// <summary>
        ///     Start the automatic blink timer
        /// </summary>
        private void StartBlinkTimer()
        {
            if (blinkTimerCoroutine != null)
            {
                StopCoroutine(blinkTimerCoroutine);
            }
            
            blinkTimerCoroutine = StartCoroutine(BlinkTimerCoroutine());
        }
        
        /// <summary>
        ///     Coroutine that handles the timing of automatic blinks
        /// </summary>
        private IEnumerator BlinkTimerCoroutine()
        {
            while (enableBlinking && !isManuallyClosed)
            {
                float randomInterval = Random.Range(blinkIntervalRange.x, blinkIntervalRange.y);
                yield return new WaitForSeconds(randomInterval);
                
                if (!isBlinking && !isManuallyClosed)
                {
                    bool isDoubleBlink = Random.Range(0f, 1f) < doubleBinkChance;
                    StartBlink(isDoubleBlink);
                }
            }
        }
        
        /// <summary>
        ///     Start a blink animation
        /// </summary>
        public void StartBlink(bool isDoubleBlink = false)
        {
            if (isBlinking || isManuallyClosed) return;
            
            // Don't blink if emotion has only one sprite (like SMILE)
            if (emotionSprites.ContainsKey(currentEmotion) && emotionSprites[currentEmotion].Count <= 1)
            {
                return;
            }
            
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            
            blinkCoroutine = StartCoroutine(isDoubleBlink ? DoubleBlinkCoroutine() : SingleBlinkCoroutine());
        }
        
        /// <summary>
        ///     Coroutine for single blink animation
        /// </summary>
        private IEnumerator SingleBlinkCoroutine()
        {
            isBlinking = true;
            
            if (!emotionSprites.ContainsKey(currentEmotion) || emotionSprites[currentEmotion].Count < blinkFrameSequence.Length)
            {
                yield break;
            }
            
            List<Sprite> sprites = emotionSprites[currentEmotion];
            
            // Play blink sequence: 0, 50, 80, 110, 105, then back to 100
            for (int i = 0; i < sprites.Count - 1; i++) // -1 to exclude the final open frame initially
            {
                eyeRenderer.sprite = sprites[i];
                yield return new WaitForSeconds(singleBlinkFrameTimes[i]);
            }
            
            // Return to open eye (frame 100)
            eyeRenderer.sprite = sprites[^1];
            
            isBlinking = false;
        }
        
        /// <summary>
        ///     Coroutine for double blink animation
        /// </summary>
        private IEnumerator DoubleBlinkCoroutine()
        {
            isBlinking = true;
            
            if (!emotionSprites.ContainsKey(currentEmotion) || emotionSprites[currentEmotion].Count < blinkFrameSequence.Length)
            {
                yield break;
            }
            
            List<Sprite> sprites = emotionSprites[currentEmotion];
            
            // First blink (faster)
            for (int i = 0; i < doubleBlinkFirstFrameTimes.Length - 1; i++)
            {
                if (i < sprites.Count)
                {
                    eyeRenderer.sprite = sprites[i];
                    yield return new WaitForSeconds(doubleBlinkFirstFrameTimes[i]);
                }
            }
            
            // Brief pause at open eye
            eyeRenderer.sprite = sprites[^1];
            yield return new WaitForSeconds(doubleBlinkFirstFrameTimes[^1]);
            
            // Second blink (normal speed)
            for (int i = 0; i < sprites.Count - 1; i++)
            {
                eyeRenderer.sprite = sprites[i];
                yield return new WaitForSeconds(doubleBlinkSecondFrameTimes[i]);
            }
            
            // Return to open eye (frame 100)
            eyeRenderer.sprite = sprites[^1];
            
            isBlinking = false;
        }
        
        /// <summary>
        ///     Enable or disable automatic blinking
        /// </summary>
        public void SetBlinkingEnabled(bool enabled)
        {
            enableBlinking = enabled;
            
            if (enabled)
            {
                StartBlinkTimer();
            }
            else
            {
                if (blinkTimerCoroutine != null)
                {
                    StopCoroutine(blinkTimerCoroutine);
                    blinkTimerCoroutine = null;
                }
            }
        }
        
        /// <summary>
        ///     Get the current eye emotion
        /// </summary>
        public EyeEmotion GetCurrentEmotion()
        {
            return currentEmotion;
        }
        
        /// <summary>
        ///     Check if currently blinking
        /// </summary>
        public bool IsBlinking()
        {
            return isBlinking;
        }
        
        /// <summary>
        ///     Manually close the eyes and stop automatic blinking
        /// </summary>
        public void CloseEyes()
        {
            if (isManuallyClosed) return;
            
            isManuallyClosed = true;
            
            // Stop any current blinking animation
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            
            // Stop automatic blinking
            if (blinkTimerCoroutine != null)
            {
                StopCoroutine(blinkTimerCoroutine);
                blinkTimerCoroutine = null;
            }
            
            isBlinking = false;
            
            // Set to closed eye sprite (frame 0)
            if (emotionSprites.ContainsKey(currentEmotion) && emotionSprites[currentEmotion].Count > 0)
            {
                eyeRenderer.sprite = emotionSprites[currentEmotion][0]; // Frame 0 = closed
            }
            
            this.Log("Eyes manually closed, blinking stopped");
        }
        
        /// <summary>
        ///     Manually open the eyes and resume automatic blinking
        /// </summary>
        public void OpenEyes()
        {
            if (!isManuallyClosed) return;
            
            isManuallyClosed = false;
            
            // Restore to current emotion's open eye sprite
            SetEmotion(currentEmotion);
            
            // Resume automatic blinking if enabled
            if (enableBlinking)
            {
                StartBlinkTimer();
            }
            
            this.Log("Eyes manually opened, blinking resumed");
        }
        
        /// <summary>
        ///     Check if eyes are manually closed
        /// </summary>
        public bool IsManuallyClosed()
        {
            return isManuallyClosed;
        }
        
        private void OnDestroy()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            
            if (blinkTimerCoroutine != null)
            {
                StopCoroutine(blinkTimerCoroutine);
            }
        }
    }
}