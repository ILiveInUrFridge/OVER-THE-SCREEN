using UnityEngine;
using System.Collections.Generic;
using Game.Model.Controller.Data;

namespace Game.Model.Controller
{
    /// <summary>
    ///     Controls the character's clothing by managing the sprites and visibility 
    ///     for each layer defined in <see cref="ClothingType" />.
    /// </summary>
    public class ClothingController : MonoBehaviour, ILoggable
    {
        /// <summary>
        ///     Nested class representing a clothing layer with its type and SpriteRenderer.
        /// </summary>
        [System.Serializable]
        public class ClothingLayer
        {
            public ClothingType type;
            public SpriteRenderer renderer;
        }

        /// <summary>
        ///     List of clothing layers, each containing a <see cref="ClothingType" />
        ///     and its associated <see cref="SpriteRenderer" />.
        /// </summary>
        [Header("Clothing Layers")]
        public List<ClothingLayer> layers = new List<ClothingLayer>();

        /// <summary>
        ///     An internal dictionary mapping each <see cref="ClothingType" /> to its <see cref="SpriteRenderer" />.
        /// </summary>
        private Dictionary<ClothingType, SpriteRenderer> layerDictionary;

        [Header("Sprite Folders")]
        private string topAFolder = "Game/Model/{spriteName}/Clothing/TopA";
        private string topBFolder = "Game/Model/{spriteName}/Clothing/TopB";
        private string bottomFolder = "Game/Model/{spriteName}/Clothing/Bottom";
        private string braFolder = "Game/Model/{spriteName}/Clothing/Bra";
        private string pantiesFolder = "Game/Model/{spriteName}/Clothing/Panties";
        private string accessoryHairFolder = "Game/Model/{spriteName}/Clothing/Accessory/Hair";
        private string accessoryHeadFolder = "Game/Model/{spriteName}/Clothing/Accessory/Head";
        private string accessoryNeckFolder = "Game/Model/{spriteName}/Clothing/Accessory/Neck";
        private string accessoryBreastsFolder = "Game/Model/{spriteName}/Clothing/Accessory/Breasts";
        private string accessoryCrotchFolder = "Game/Model/{spriteName}/Clothing/Accessory/Crotch";
        private string extraFolder = "Game/Model/{spriteName}/Clothing/Extra";

        /// <summary>
        ///     An internal dictionary mapping each Clothing Dictionary to its <see cref="ClothingType" />.
        /// </summary>
        private Dictionary<ClothingType, Dictionary<string, Sprite>> spriteDictionaries;

        private SpriteController spriteController;

        [Header("Editor Testing Options")]
        public ClothingType editorTypeToSet;                   // Dropdown to select the layer to modify
        public string editorSpriteNameToSet;                   // Field to input the sprite name to set
        public bool editorHideBraOnTop = false;                // Option to hide the bra when setting Top
        public bool editorHidePantiesOnBottom = false;         // Option to hide panties when setting Bottom

        /// <summary>
        ///     Initialize the clothing controller with sprite controller reference
        /// </summary>
        public void Initialize(SpriteController controller)
        {
            spriteController = controller;
            layerDictionary = InitializeLayerDictionary();
            spriteDictionaries = InitializeSpriteDictionaries();
        }

        /// <summary>
        ///     Sets clothing for the specified type and sprite name using Editor inputs.
        /// </summary>
        [ContextMenu("Apply Editor Clothing")]
        public void ApplyEditorClothing()
        {
            if (editorTypeToSet == ClothingType.TOP_A || editorTypeToSet == ClothingType.TOP_B)
            {
                // Apply top clothing with optional bra hiding
                SetTop(editorSpriteNameToSet, editorHideBraOnTop);
            }
            else if (editorTypeToSet == ClothingType.BOTTOM)
            {
                // Apply bottom clothing with optional panties hiding
                SetBottom(editorSpriteNameToSet, editorHidePantiesOnBottom);
            }
            else
            {
                // Generic setting for other layers
                SetClothingByName(editorTypeToSet, editorSpriteNameToSet);
            }

            this.Log($"Applied clothing '{editorSpriteNameToSet}' to '{editorTypeToSet}'.");
        }

        /// <summary>
        ///     Removes clothing for the specified type using Editor inputs.
        /// </summary>
        [ContextMenu("Remove Editor Clothing")]
        public void RemoveEditorClothing()
        {
            RemoveClothingByLayer(editorTypeToSet);
            this.Log($"Removed clothing from '{editorTypeToSet}'.");
        }

        /// <summary>
        ///     Removes all clothing using Editor inputs.
        /// </summary>
        [ContextMenu("Strip All Clothing")]
        public void StripAllEditorClothing()
        {
            Strip();
            this.Log("Removed all clothing.");
        }

        /// <summary>
        ///     Initializes the layer dictionary for fast renderer lookups.
        /// </summary>
        /// 
        /// <returns>
        ///     A dictionary mapping <see cref="ClothingType" /> to <see cref="SpriteRenderer" />.
        /// </returns>
        private Dictionary<ClothingType, SpriteRenderer> InitializeLayerDictionary()
        {
            var dictionary = new Dictionary<ClothingType, SpriteRenderer>();

            foreach (var layer in layers)
            {
                if (layer.renderer == null) {
                    this.LogWarning($"Layer '{layer.type}' is missing a SpriteRenderer.");
                    continue;
                }

                dictionary[layer.type] = layer.renderer;
                this.Log($"Layer '{layer.type}' initialized successfully.");
            }

            return dictionary;
        }

        /// <summary>
        ///     Initializes the sprite dictionaries for fast name-to-sprite lookups.
        /// </summary>
        /// 
        /// <returns>
        ///     A dictionary mapping <see cref="ClothingType" /> to dictionaries of sprite mappings.
        /// </returns>
        private Dictionary<ClothingType, Dictionary<string, Sprite>> InitializeSpriteDictionaries()
        {
            var spriteDicts = new Dictionary<ClothingType, Dictionary<string, Sprite>>();

            foreach (ClothingType type in System.Enum.GetValues(typeof(ClothingType)))
            {
                string folderName = GetFolderNameForType(type); // Dynamically get folder path based on type
                if (!string.IsNullOrEmpty(folderName))
                {
                    spriteDicts[type] = LoadSpritesFromFolder(folderName);
                }
            }

            return spriteDicts;
        }

        /// <summary>
        ///     Loads all sprites from a specified Resources folder (including subdirectories) and creates a dictionary mapping their names to the sprites.
        /// </summary>
        /// <param name="folderName">
        ///     The folder name inside the Resources folder.
        /// </param>
        /// <returns>
        ///     A dictionary mapping sprite names to their respective sprites.
        /// </returns>
        private Dictionary<string, Sprite> LoadSpritesFromFolder(string folderName)
        {
            // Load all sprites from the specified folder and subdirectories
            Sprite[] sprites = Resources.LoadAll<Sprite>(folderName);

            if (sprites.Length == 0) {
                this.LogWarning($"No sprites found in folder '{folderName}'.");
            }

            // Create a dictionary to map sprite names to sprite objects
            var dictionary = new Dictionary<string, Sprite>();

            foreach (var sprite in sprites)
            {
                // Check for duplicate sprite names
                if (dictionary.ContainsKey(sprite.name))
                {
                    this.LogWarning($"Duplicate sprite name '{sprite.name}' detected in folder '{folderName}'.");
                    continue;
                }

                // Add the sprite to the dictionary
                dictionary[sprite.name] = sprite;
                this.Log($"Loaded sprite '{sprite.name}' from folder '{folderName}'.");
            }

            return dictionary;
        }

        /// <summary>
        ///     Maps a clothing type to its corresponding folder path.
        /// </summary>
        /// <param name="type">The clothing type.</param>
        /// <returns>The folder path, or null if the type is unsupported.</returns>
        private string GetFolderNameForType(ClothingType type)
        {
            if (spriteController == null) return null;
            
            string spriteName = spriteController.GetSpriteName();
            
            return type switch
            {
                ClothingType.TOP_A => topAFolder.Replace("{spriteName}", spriteName),
                ClothingType.TOP_B => topBFolder.Replace("{spriteName}", spriteName),
                ClothingType.BOTTOM => bottomFolder.Replace("{spriteName}", spriteName),
                ClothingType.BRA => braFolder.Replace("{spriteName}", spriteName),
                ClothingType.PANTIES => pantiesFolder.Replace("{spriteName}", spriteName),
                ClothingType.ACCESSORY_HAIR => accessoryHairFolder.Replace("{spriteName}", spriteName),
                ClothingType.ACCESSORY_HEAD => accessoryHeadFolder.Replace("{spriteName}", spriteName),
                ClothingType.ACCESSORY_NECK => accessoryNeckFolder.Replace("{spriteName}", spriteName),
                ClothingType.ACCESSORY_BREASTS => accessoryBreastsFolder.Replace("{spriteName}", spriteName),
                ClothingType.ACCESSORY_CROTCH => accessoryCrotchFolder.Replace("{spriteName}", spriteName),
                ClothingType.EXTRA => extraFolder.Replace("{spriteName}", spriteName),
                _ => null
            };
        }

        /*
         * FUNCTIONS TO SET CLOTHES
         */

        /// <summary>
        ///     Sets a sprite for a specific clothing type by name.
        /// </summary>
        /// 
        /// <param name="type">
        ///     The <see cref="ClothingType" /> identifying which clothing layer to change.
        /// </param>
        /// 
        /// <param name="spriteName">
        ///     The name of the sprite to set from the dictionary.
        /// </param>
        public void SetClothingByName(ClothingType type, string spriteName)
        {
            if (!layerDictionary.TryGetValue(type, out var renderer)) {
                this.LogWarning($"Clothing layer '{type}' not found.");
                return;
            }

            if (!spriteDictionaries.TryGetValue(type, out var spriteDict)) {
                this.LogWarning($"Clothing type '{type}' not found in spriteDictionaries.");
                return;
            }

            if (!spriteDict.TryGetValue(spriteName, out var sprite)) {
                this.Log($"Sprite '{spriteName}' not found in the dictionary for '{type}'. Available sprites: {string.Join(", ", spriteDict.Keys)}");
                return;
            }

            renderer.sprite = sprite;
            this.Log($"Set clothing '{type}' to sprite '{sprite.name}'.");
        }

        /// <summary>
        ///     Sets a clothing layer to nothing.
        /// </summary>
        /// 
        /// <param name="type">
        ///     The <see cref="ClothingType" /> identifying which clothing layer to remove.
        /// </param>
        public void RemoveClothingByLayer(ClothingType type)
        {
            if (!layerDictionary.TryGetValue(type, out var renderer))
            {
                this.LogWarning($"Clothing layer '{type}' not found.");
                return;
            }

            // Remove any dependent layers based on the type
            ClothingType? dependentLayer = type switch
            {
                ClothingType.TOP_A => ClothingType.TOP_B,
                _ => null
            };

            if (dependentLayer.HasValue)
            {
                RemoveClothingByLayer(dependentLayer.Value);
            }

            renderer.sprite = null;
            this.Log($"Removed clothing from layer '{type}'.");
        }

        /// <summary>
        ///     Removes all clothing by setting their sprites to null.
        /// </summary>
        public void Strip()
        {
            foreach (var layer in layers)
            {
                if (layer.renderer == null) {
                    continue;
                }
                
                layer.renderer.sprite = null; // Remove the sprite
                this.Log($"Stripped clothing from layer '{layer.type}'.");
            }
        }

        /// <summary>
        ///     Sets the sprite for Top clothing.
        ///     Internally, this triggers both TOP_A and TOP_B.
        ///     
        ///     IMPORTANT: TOP_A and TOP_B's spriteName must be the same in order for this function to work.
        ///     NOTE:      TOP_A and TOP_B are independent layers, and a sprite mismatch is allowed.
        ///                So if mismatch is wanted, using SetTopA() and SetTopB() separately is recommended.
        /// </summary>
        /// 
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        /// <param name="hideBra">
        ///     Whether to hide the bra sprite or not.
        /// </param>
        public void SetTop(string spriteName, bool hideBra = false)
        {
            SetTopA(spriteName, hideBra);
            SetTopB(spriteName);
        }

        /// NOTE: SetTopA() and SetTopB() are separated, apart from SetTop()
        ///       to have more flexibility and control over the clothing system.
        /// 
        /// <summary>
        ///     Sets the sprite for TOP_A layer.
        /// </summary>
        /// 
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        /// <param name="hideBra">
        ///     Whether to hide the bra sprite or not.
        /// </param>
        public void SetTopA(string spriteName, bool hideBra = false)
        {
            SetClothingByName(ClothingType.TOP_A, spriteName);
            ToggleClothingVisibility(ClothingType.BRA, !hideBra);
        }

        /// <summary>
        ///     Sets the sprite for TOP_B layer.
        /// </summary>
        /// 
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetTopB(string spriteName)
        {
            RemoveClothingByLayer(ClothingType.TOP_B); // This only accounts for TOP_B, since this is the only layer that may or may not have the corresponding replacement.

            SetClothingByName(ClothingType.TOP_B, spriteName);
        }

        /// <summary>
        ///     Sets the sprite for BOTTOM layer.
        /// </summary>
        /// 
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        /// <param name="hidePanties">
        ///     Whether to hide the panties sprite or not.
        /// </param>
        public void SetBottom(string spriteName, bool hidePanties = false)
        {
            SetClothingByName(ClothingType.BOTTOM, spriteName);
            ToggleClothingVisibility(ClothingType.PANTIES, !hidePanties);
        }

        /// <summary>
        ///     Sets the sprite for BRA layer.
        /// </summary>
        /// 
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetBra(string spriteName)
        {
            SetClothingByName(ClothingType.BRA, spriteName);
        }

        /// <summary>
        ///     Sets the sprite for PANTIES layer.
        /// </summary>
        /// 
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetPanties(string spriteName)
        {
            SetClothingByName(ClothingType.PANTIES, spriteName);
        }

        /// <summary>
        ///     Sets the sprite for ACCESSORY_HAIR layer.
        /// </summary>
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetAccessoryHair(string spriteName)
        {
            SetClothingByName(ClothingType.ACCESSORY_HAIR, spriteName);
        }

        /// <summary>
        ///     Sets the sprite for ACCESSORY_HEAD layer.
        /// </summary>
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetAccessoryHead(string spriteName)
        {
            SetClothingByName(ClothingType.ACCESSORY_HEAD, spriteName);
        }

        /// <summary>
        ///     Sets the sprite for ACCESSORY_NECK layer.
        /// </summary>
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetAccessoryNeck(string spriteName)
        {
            SetClothingByName(ClothingType.ACCESSORY_NECK, spriteName);
        }

        /// <summary>
        ///     Sets the sprite for ACCESSORY_BREASTS layer.
        /// </summary>
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetAccessoryBreasts(string spriteName)
        {
            SetClothingByName(ClothingType.ACCESSORY_BREASTS, spriteName);
        }

        /// <summary>
        ///     Sets the sprite for ACCESSORY_CROTCH layer.
        /// </summary>
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetAccessoryCrotch(string spriteName)
        {
            SetClothingByName(ClothingType.ACCESSORY_CROTCH, spriteName);
        }

        /// <summary>
        ///     Sets the sprite for EXTRA layer.
        /// </summary>
        /// <param name="spriteName">
        ///     The name of the sprite to set from the mapping list.
        /// </param>
        public void SetExtra(string spriteName)
        {
            SetClothingByName(ClothingType.EXTRA, spriteName);
        }

        /// <summary>
        ///     Toggles the visibility of a specific clothing layer.
        /// </summary>
        /// 
        /// <param name="type">
        ///     The <see cref="ClothingType" /> identifying which clothing layer to toggle.
        /// </param>
        /// <param name="isVisible">
        ///     If true, the clothing layer is visible; otherwise, it is hidden.
        /// </param>
        public void ToggleClothingVisibility(ClothingType type, bool isVisible)
        {
            if (!layerDictionary.TryGetValue(type, out var renderer)) {
                this.LogWarning($"Clothing layer '{type}' not found.");
                return;
            }

            renderer.enabled = isVisible;
            this.Log($"Toggled '{type}' visibility to {isVisible}.");
        }

        /// <summary>
        ///     Get available sprite names for a clothing type
        /// </summary>
        public List<string> GetAvailableSprites(ClothingType type)
        {
            if (spriteDictionaries.TryGetValue(type, out var spriteDict))
            {
                return new List<string>(spriteDict.Keys);
            }
            return new List<string>();
        }
    }
}