using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Model.Controller.Data;

namespace Game.Model.Controller
{
    /// <summary>
    ///     Controller for the mouth of a Purrine sprite.
    /// </summary>
    public class MouthController : MonoBehaviour, ILoggable
    {
        [Header("Mouth SpriteRenderer Object")]
        public SpriteRenderer mouthRenderer;
        
        [Header("Talking Configuration")]
        [SerializeField] private float frameDuration = 0.09f;
        
        [Header("Mouth Emotion")]
        [SerializeField] private MouthEmotion currentEmotion = MouthEmotion.NEUTRAL;

        private SpriteController spriteController;
        private Coroutine talkCoroutine;
        private readonly Dictionary<MouthEmotion, Dictionary<string, Sprite>> emotionSprites = new();
        private bool isTalking = false;
        
        // Talking frame names
        private readonly string[] talkingFrames = { "Closed", "A", "E", "I", "O" };

        /// <summary>
        ///     Initialize the mouth controller with sprite controller reference
        /// </summary>
        public void Initialize(SpriteController controller)
        {
            spriteController = controller;
            LoadMouthSprites();
            SetEmotion(currentEmotion);
        }
        
        /// <summary>
        ///     Load all mouth sprites for all available emotions
        /// </summary>
        private void LoadMouthSprites()
        {
            if (spriteController == null) return;
            
            string spriteName = spriteController.GetSpriteName();
            
            // Load sprites for each emotion
            foreach (MouthEmotion emotion in System.Enum.GetValues(typeof(MouthEmotion)))
            {
                Dictionary<string, Sprite> sprites = LoadSpritesForEmotion(spriteName, emotion);
                emotionSprites[emotion] = sprites;
                
                this.Log($"Loaded {sprites.Count} sprites for {emotion} mouth emotion");
            }
        }
        
        /// <summary>
        ///     Load sprites for a specific emotion
        /// </summary>
        private Dictionary<string, Sprite> LoadSpritesForEmotion(string spriteName, MouthEmotion emotion)
        {
            string emotionName = emotion.ToString().Replace("_", " ").ToLower();
            emotionName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(emotionName);
            
            string basePath = $"Game/Model/{spriteName}/Facial/Mouth/{emotionName}";
            
            Dictionary<string, Sprite> sprites = new();
            
            // Try to load each talking frame
            foreach (string frameName in talkingFrames)
            {
                string spritePath = $"{basePath}/{frameName}";
                Sprite sprite = Resources.Load<Sprite>(spritePath);
                
                if (sprite != null)
                {
                    sprites[frameName] = sprite;
                }
            }
            
            // If no specific frames found, try to load any sprites in the directory
            if (sprites.Count == 0)
            {
                Sprite[] allSprites = Resources.LoadAll<Sprite>(basePath);
                if (allSprites.Length > 0)
                {
                    // For single sprite emotions, use it as the default/closed state
                    sprites["Closed"] = allSprites[0];
                }
            }
            
            return sprites;
        }
        
        /// <summary>
        ///     Set the current mouth emotion
        /// </summary>
        public void SetEmotion(MouthEmotion emotion)
        {
            currentEmotion = emotion;
            
            if (emotionSprites.ContainsKey(emotion) && emotionSprites[emotion].Count > 0)
            {
                // Set to the closed mouth frame by default
                if (emotionSprites[emotion].ContainsKey("Closed"))
                {
                    mouthRenderer.sprite = emotionSprites[emotion]["Closed"];
                }
                else
                {
                    // If no closed frame, use the first available sprite
                    mouthRenderer.sprite = emotionSprites[emotion].Values.First();
                }
            }
        }
        
        /// <summary>
        ///     Start talking animation
        /// </summary>
        public void StartTalking()
        {
            if (isTalking) return;
            
            // Don't talk if emotion has only one sprite (like single-sprite emotions)
            if (!emotionSprites.ContainsKey(currentEmotion) || 
                emotionSprites[currentEmotion].Count <= 1)
            {
                return;
            }
            
            if (talkCoroutine != null)
            {
                StopCoroutine(talkCoroutine);
            }
            
            talkCoroutine = StartCoroutine(TalkingCoroutine());
        }
        
        /// <summary>
        ///     Stop talking animation and return to closed mouth
        /// </summary>
        public void StopTalking()
        {
            if (talkCoroutine != null)
            {
                StopCoroutine(talkCoroutine);
                talkCoroutine = null;
            }
            
            isTalking = false;
            
            // Return to closed mouth
            if (emotionSprites.ContainsKey(currentEmotion) && 
                emotionSprites[currentEmotion].ContainsKey("Closed"))
            {
                mouthRenderer.sprite = emotionSprites[currentEmotion]["Closed"];
            }
        }
        
        /// <summary>
        ///     Coroutine for talking animation with randomized frames
        /// </summary>
        private IEnumerator TalkingCoroutine()
        {
            isTalking = true;
            
            if (!emotionSprites.ContainsKey(currentEmotion))
            {
                yield break;
            }
            
            Dictionary<string, Sprite> sprites = emotionSprites[currentEmotion];
            
            // Get all available talking frames (including "Closed")
            List<string> availableFrames = sprites.Keys.ToList();
            
            if (availableFrames.Count == 0)
            {
                yield break;
            }
            
            string lastFrame = "";
            
            while (isTalking)
            {
                // Get a random frame that's different from the last one
                List<string> frameOptions = availableFrames.Where(f => f != lastFrame).ToList();
                
                if (frameOptions.Count == 0)
                {
                    frameOptions = availableFrames; // Fallback if somehow we have no options
                }
                
                string randomFrame = frameOptions[Random.Range(0, frameOptions.Count)];
                mouthRenderer.sprite = sprites[randomFrame];
                lastFrame = randomFrame;
                
                yield return new WaitForSeconds(frameDuration);
            }
        }
        
        /// <summary>
        ///     Trigger a single talking sequence for a specified duration
        /// </summary>
        public void Talk(float duration)
        {
            StartCoroutine(TalkForDuration(duration));
        }
        
        /// <summary>
        ///     Coroutine to talk for a specific duration then stop
        /// </summary>
        private IEnumerator TalkForDuration(float duration)
        {
            StartTalking();
            yield return new WaitForSeconds(duration);
            StopTalking();
        }
        
        /// <summary>
        ///     Get the current mouth emotion
        /// </summary>
        public MouthEmotion GetCurrentEmotion()
        {
            return currentEmotion;
        }
        
        /// <summary>
        ///     Check if currently talking
        /// </summary>
        public bool IsTalking()
        {
            return isTalking;
        }
        
        private void OnDestroy()
        {
            if (talkCoroutine != null)
            {
                StopCoroutine(talkCoroutine);
            }
        }
    }
} 