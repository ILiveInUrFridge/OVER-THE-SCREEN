using UnityEngine;
using System.Collections.Generic;
using OTS.Model.Controller.Data;

namespace OTS.Model.Controller
{
    /// <summary>
    ///     Controller for the eyebrows of a Purrine sprite.
    /// </summary>
    public class EyebrowController : MonoBehaviour, ILoggable
    {
        [Header("Eyebrow SpriteRenderer Object")]
        public SpriteRenderer eyebrowRenderer;
        
        [Header("Eyebrow Emotion")]
        [SerializeField] private EyebrowEmotion currentEmotion = EyebrowEmotion.NEUTRAL;

        private SpriteController spriteController;
        private readonly Dictionary<EyebrowEmotion, Sprite> emotionSprites = new();

        /// <summary>
        ///     Initialize the eyebrow controller with sprite controller reference
        /// </summary>
        public void Initialize(SpriteController controller)
        {
            spriteController = controller;
            LoadEyebrowSprites();
            SetEmotion(currentEmotion);
        }
        
        /// <summary>
        ///     Load all eyebrow sprites for all available emotions
        /// </summary>
        private void LoadEyebrowSprites()
        {
            if (spriteController == null) return;
            
            string spriteName = spriteController.GetSpriteName();
            
            // Load sprites for each emotion
            foreach (EyebrowEmotion emotion in System.Enum.GetValues(typeof(EyebrowEmotion)))
            {
                Sprite sprite = LoadSpriteForEmotion(spriteName, emotion);
                if (sprite != null)
                {
                    emotionSprites[emotion] = sprite;
                    this.Log($"Loaded sprite for {emotion} eyebrow emotion");
                }
                else
                {
                    this.Log($"No sprite found for {emotion} eyebrow emotion");
                }
            }
        }
        
        /// <summary>
        ///     Load sprite for a specific emotion
        /// </summary>
        private Sprite LoadSpriteForEmotion(string spriteName, EyebrowEmotion emotion)
        {
            string emotionName = emotion.ToString().Replace("_", " ").ToLower();
            emotionName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(emotionName);
            
            string spritePath = $"Game/Model/{spriteName}/Facial/Eyebrow/{emotionName}";
            
            // Try to load any sprite in the directory
            Sprite[] allSprites = Resources.LoadAll<Sprite>(spritePath);
            
            if (allSprites.Length > 0)
            {
                return allSprites[0]; // Return the first sprite found
            }
            
            return null;
        }
        
        /// <summary>
        ///     Set the current eyebrow emotion
        /// </summary>
        public void SetEmotion(EyebrowEmotion emotion)
        {
            currentEmotion = emotion;
            
            if (emotionSprites.ContainsKey(emotion) && emotionSprites[emotion] != null)
            {
                eyebrowRenderer.sprite = emotionSprites[emotion];
            }
        }
        
        /// <summary>
        ///     Get the current eyebrow emotion
        /// </summary>
        public EyebrowEmotion GetCurrentEmotion()
        {
            return currentEmotion;
        }
    }
} 