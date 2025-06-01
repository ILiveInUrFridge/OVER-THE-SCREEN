using UnityEngine;

using Game.Model.Controller.Data;

namespace Game.Model.Controller
{
    /// <summary>
    ///     Main controller for managing different sprite types and their components.
    /// </summary>
    public class SpriteController : MonoBehaviour, ILoggable
    {
        [Header("Sprite Configuration")]
        [SerializeField] private SpriteType spriteType = SpriteType.SPRITE_A;

        public SpriteRenderer baseBodyRenderer;
        public Sprite baseBodySprite;
        
        [Header("Controllers")]
        [SerializeField] private EyeController eyeController;
        [SerializeField] private EyebrowController eyebrowController;
        [SerializeField] private MouthController mouthController;
        [SerializeField] private BlushController blushController;
        [SerializeField] private ClothingController clothingController;

        [Header("Inspector Test Controls")]
        [SerializeField] private EyeEmotion testEyeEmotion = EyeEmotion.DEFAULT;
        [SerializeField] private EyebrowEmotion testEyebrowEmotion = EyebrowEmotion.NEUTRAL;
        [SerializeField] private MouthEmotion testMouthEmotion = MouthEmotion.NEUTRAL;
        [SerializeField] private BlushStrength testBlushStrength = BlushStrength.NONE;
        [SerializeField] private float testTalkDuration = 2f;
        
        [Header("Simple Clothing Test")]
        [SerializeField] private ClothingType simpleTestType = ClothingType.TOP_A;
        [SerializeField] private string simpleTestSpriteName = "basic_shirt";
        [SerializeField] private bool hideUndergarments = false;

        /// <summary>
        ///     Gets the current sprite type
        /// </summary>
        public SpriteType GetSpriteType()
        {
            return spriteType;
        }
        
        /// <summary>
        ///     Gets the sprite name for resource loading
        /// </summary>
        public string GetSpriteName()
        {
            switch (spriteType)
            {
                case SpriteType.SPRITE_A:
                    return "Sprite A";
                default:
                    return "Sprite A";
            }
        }

        private void Start()
        {
            if (baseBodyRenderer == null)
            {
                this.LogWarning($"Base body renderer is not assigned for {gameObject.name}");
            }

            baseBodyRenderer.sprite = baseBodySprite;

            // Initialize all controllers with sprite type information
            if (eyeController != null)
            {
                eyeController.Initialize(this);
            }

            if (eyebrowController != null)
            {
                eyebrowController.Initialize(this);
            }

            if (mouthController != null)
            {
                mouthController.Initialize(this);
            }

            if (blushController != null)
            {
                blushController.Initialize(this);
            }
            
            if (clothingController != null)
            {
                clothingController.Initialize(this);
            }
        }
        
        #region Inspector Test Functions
        
        [ContextMenu("Apply Test Eye Emotion")]
        public void ApplyTestEyeEmotion()
        {
            if (eyeController != null)
            {
                eyeController.SetEmotion(testEyeEmotion);
                this.Log($"Set eye emotion to: {testEyeEmotion}");
            }
        }
        
        [ContextMenu("Apply Test Eyebrow Emotion")]
        public void ApplyTestEyebrowEmotion()
        {
            if (eyebrowController != null)
            {
                eyebrowController.SetEmotion(testEyebrowEmotion);
                this.Log($"Set eyebrow emotion to: {testEyebrowEmotion}");
            }
        }
        
        [ContextMenu("Apply Test Mouth Emotion")]
        public void ApplyTestMouthEmotion()
        {
            if (mouthController != null)
            {
                mouthController.SetEmotion(testMouthEmotion);
                this.Log($"Set mouth emotion to: {testMouthEmotion}");
            }
        }
        
        [ContextMenu("Apply Test Blush Strength")]
        public void ApplyTestBlushStrength()
        {
            if (blushController != null)
            {
                blushController.SetStrength(testBlushStrength);
                this.Log($"Set blush strength to: {testBlushStrength}");
            }
        }
        
        [ContextMenu("Start Talking")]
        public void StartTalking()
        {
            if (mouthController != null)
            {
                mouthController.StartTalking();
                this.Log("Started talking animation");
            }
        }
        
        [ContextMenu("Stop Talking")]
        public void StopTalking()
        {
            if (mouthController != null)
            {
                mouthController.StopTalking();
                this.Log("Stopped talking animation");
            }
        }
        
        [ContextMenu("Talk For Test Duration")]
        public void TalkForTestDuration()
        {
            if (mouthController != null)
            {
                mouthController.Talk(testTalkDuration);
                this.Log($"Started talking for {testTalkDuration} seconds");
            }
        }
        
        [ContextMenu("Trigger Test Blink")]
        public void TriggerTestBlink()
        {
            if (eyeController != null)
            {
                eyeController.StartBlink(false);
                this.Log("Triggered single blink");
            }
        }
        
        [ContextMenu("Trigger Test Double Blink")]
        public void TriggerTestDoubleBlink()
        {
            if (eyeController != null)
            {
                eyeController.StartBlink(true);
                this.Log("Triggered double blink");
            }
        }
        
        [ContextMenu("Apply All Test Emotions")]
        public void ApplyAllTestEmotions()
        {
            ApplyTestEyeEmotion();
            ApplyTestEyebrowEmotion();
            ApplyTestMouthEmotion();
            ApplyTestBlushStrength();
            this.Log("Applied all test emotions");
        }
        
        [ContextMenu("Close Eyes")]
        public void CloseEyes()
        {
            if (eyeController != null)
            {
                eyeController.CloseEyes();
                this.Log("Closed eyes manually");
            }
        }
        
        [ContextMenu("Open Eyes")]
        public void OpenEyes()
        {
            if (eyeController != null)
            {
                eyeController.OpenEyes();
                this.Log("Opened eyes manually");
            }
        }
        
        [ContextMenu("Set Simple Test Clothing")]
        public void SetSimpleTestClothing()
        {
            if (clothingController != null)
            {
                if (simpleTestType == ClothingType.TOP_A || simpleTestType == ClothingType.TOP_B)
                {
                    clothingController.SetTop(simpleTestSpriteName, hideUndergarments);
                    this.Log($"Set top clothing: {simpleTestSpriteName} (Hide Bra: {hideUndergarments})");
                }
                else if (simpleTestType == ClothingType.BOTTOM)
                {
                    clothingController.SetBottom(simpleTestSpriteName, hideUndergarments);
                    this.Log($"Set bottom clothing: {simpleTestSpriteName} (Hide Panties: {hideUndergarments})");
                }
                else
                {
                    clothingController.SetClothingByName(simpleTestType, simpleTestSpriteName);
                    this.Log($"Set clothing: {simpleTestType} = {simpleTestSpriteName}");
                }
            }
        }
        
        [ContextMenu("Remove Simple Test Clothing")]
        public void RemoveSimpleTestClothing()
        {
            if (clothingController != null)
            {
                clothingController.RemoveClothingByLayer(simpleTestType);
                this.Log($"Removed clothing from: {simpleTestType}");
            }
        }
        
        [ContextMenu("Strip All Clothing")]
        public void StripAllClothing()
        {
            if (clothingController != null)
            {
                clothingController.Strip();
                this.Log("Stripped all clothing");
            }
        }
        
        [ContextMenu("List Available Sprites")]
        public void ListAvailableSprites()
        {
            if (clothingController != null)
            {
                foreach (ClothingType type in System.Enum.GetValues(typeof(ClothingType)))
                {
                    var sprites = clothingController.GetAvailableSprites(type);
                    if (sprites.Count > 0)
                    {
                        this.Log($"{type}: {string.Join(", ", sprites)}");
                    }
                }
            }
        }
        
        #endregion
        
        /// <summary>
        /// Get reference to the eye controller
        /// </summary>
        public EyeController GetEyeController()
        {
            return eyeController;
        }
        
        /// <summary>
        /// Get reference to the eyebrow controller
        /// </summary>
        public EyebrowController GetEyebrowController()
        {
            return eyebrowController;
        }
        
        /// <summary>
        /// Get reference to the mouth controller
        /// </summary>
        public MouthController GetMouthController()
        {
            return mouthController;
        }
        
        /// <summary>
        /// Get reference to the blush controller
        /// </summary>
        public BlushController GetBlushController()
        {
            return blushController;
        }
        
        /// <summary>
        /// Get reference to the clothing controller
        /// </summary>
        public ClothingController GetClothingController()
        {
            return clothingController;
        }
    }
}
