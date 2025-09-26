using UnityEngine;
using System.Collections.Generic;
using OTS.Model.Controller.Data;

namespace OTS.Model.Controller
{
    /// <summary>
    ///     Controller for the blush of a Purrine sprite.
    /// </summary>
    public class BlushController : MonoBehaviour, ILoggable
    {
        [Header("Blush SpriteRenderer Object")]
        public SpriteRenderer blushRenderer;
        
        [Header("Blush Strength")]
        [SerializeField] private BlushStrength currentStrength = BlushStrength.NONE;

        private SpriteController spriteController;
        private readonly Dictionary<BlushStrength, Sprite> strengthSprites = new();

        /// <summary>
        ///     Initialize the blush controller with sprite controller reference
        /// </summary>
        public void Initialize(SpriteController controller)
        {
            spriteController = controller;
            LoadBlushSprites();
            SetStrength(currentStrength);
        }
        
        /// <summary>
        ///     Load all blush sprites for all available strengths
        /// </summary>
        private void LoadBlushSprites()
        {
            if (spriteController == null) return;
            
            string spriteName = spriteController.GetSpriteName();
            
            // Load sprites for each strength (skip NONE as it doesn't have a sprite file)
            foreach (BlushStrength strength in System.Enum.GetValues(typeof(BlushStrength)))
            {
                if (strength == BlushStrength.NONE)
                {
                    this.Log($"Skipping {strength} - no sprite file needed (handled as null)");
                    continue;
                }
                
                Sprite sprite = LoadSpriteForStrength(spriteName, strength);
                if (sprite != null)
                {
                    strengthSprites[strength] = sprite;
                    this.Log($"Loaded sprite for {strength} blush strength");
                }
                else
                {
                    this.Log($"No sprite found for {strength} blush strength");
                }
            }
        }
        
        /// <summary>
        ///     Load sprite for a specific blush strength
        /// </summary>
        private Sprite LoadSpriteForStrength(string spriteName, BlushStrength strength)
        {
            string strengthName = strength.ToString().Replace("_", " ").ToLower();
            strengthName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(strengthName);
            
            string spritePath = $"Game/Model/{spriteName}/Facial/Blush/{strengthName}";
            
            // Try to load any sprite in the directory
            Sprite[] allSprites = Resources.LoadAll<Sprite>(spritePath);
            
            if (allSprites.Length > 0)
            {
                return allSprites[0]; // Return the first sprite found
            }
            
            return null;
        }
        
        /// <summary>
        ///     Set the current blush strength
        /// </summary>
        public void SetStrength(BlushStrength strength)
        {
            currentStrength = strength;
            
            if (strength == BlushStrength.NONE)
            {
                // Hide blush when strength is NONE
                blushRenderer.sprite = null;
                return;
            }
            
            if (strengthSprites.ContainsKey(strength) && strengthSprites[strength] != null)
            {
                blushRenderer.sprite = strengthSprites[strength];
            }
        }
        
        /// <summary>
        ///     Get the current blush strength
        /// </summary>
        public BlushStrength GetCurrentStrength()
        {
            return currentStrength;
        }
    }
} 