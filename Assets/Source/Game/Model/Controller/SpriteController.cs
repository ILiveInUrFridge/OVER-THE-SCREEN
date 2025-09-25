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
        [SerializeField] private SpriteType spriteType = SpriteType.A;

        public SpriteRenderer baseBodyRenderer;
        public Sprite baseBodySprite;
        
        [Header("Controllers")]
        [SerializeField] private EyeController eyeController;
        [SerializeField] private EyebrowController eyebrowController;
        [SerializeField] private MouthController mouthController;
        [SerializeField] private BlushController blushController;
        [SerializeField] private ClothingController clothingController;

        [Header("Emotions")]
        [SerializeField] private EyeEmotion eyeEmotion = EyeEmotion.DEFAULT;
        [SerializeField] private EyebrowEmotion eyebrowEmotion = EyebrowEmotion.NEUTRAL;
        [SerializeField] private MouthEmotion mouthEmotion = MouthEmotion.NEUTRAL;
        [SerializeField] private BlushStrength blushStrength = BlushStrength.NONE;

        [Header("Clothing Selection")]
        [SerializeField] private string topA;
        [SerializeField] private string topB;
        [SerializeField] private string bottom;
        [SerializeField] private string bra;
        [SerializeField] private string panties;
        [SerializeField] private string accessoryHair;
        [SerializeField] private string accessoryHead;
        [SerializeField] private string accessoryNeck;
        [SerializeField] private string accessoryBreasts;
        [SerializeField] private string accessoryCrotch;
        [SerializeField] private string extra;
        [SerializeField] private bool hideBra = false;
        [SerializeField] private bool hidePanties = false;

        [Header("Mouth Talking")]
        [SerializeField] private float talkDurationSeconds = 2f;

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
                case SpriteType.A:
                    return "A";
                case SpriteType.B:
                    return "B";
                default:
                    return "A";
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
        
        #region Inspector Controls

        [ContextMenu("Apply All Changes")]
        public void ApplyAllChanges()
        {
            ApplyEmotions();
            ApplyClothing();
            this.Log("Applied emotions and clothing selections");
        }

        private void ApplyEmotions()
        {
            if (eyeController != null)
            {
                eyeController.SetEmotion(eyeEmotion);
            }

            if (eyebrowController != null)
            {
                eyebrowController.SetEmotion(eyebrowEmotion);
            }

            if (mouthController != null)
            {
                mouthController.SetEmotion(mouthEmotion);
            }

            if (blushController != null)
            {
                blushController.SetStrength(blushStrength);
            }
        }

        private void ApplyClothing()
        {
            if (clothingController == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(topA))
            {
                clothingController.SetTopA(topA, hideBra);
            }

            if (!string.IsNullOrEmpty(topB))
            {
                clothingController.SetTopB(topB);
            }

            if (!string.IsNullOrEmpty(bottom))
            {
                clothingController.SetBottom(bottom, hidePanties);
            }

            if (!string.IsNullOrEmpty(bra))
            {
                clothingController.SetBra(bra);
            }

            if (!string.IsNullOrEmpty(panties))
            {
                clothingController.SetPanties(panties);
            }

            if (!string.IsNullOrEmpty(accessoryHair))
            {
                clothingController.SetAccessoryHair(accessoryHair);
            }

            if (!string.IsNullOrEmpty(accessoryHead))
            {
                clothingController.SetAccessoryHead(accessoryHead);
            }

            if (!string.IsNullOrEmpty(accessoryNeck))
            {
                clothingController.SetAccessoryNeck(accessoryNeck);
            }

            if (!string.IsNullOrEmpty(accessoryBreasts))
            {
                clothingController.SetAccessoryBreasts(accessoryBreasts);
            }

            if (!string.IsNullOrEmpty(accessoryCrotch))
            {
                clothingController.SetAccessoryCrotch(accessoryCrotch);
            }

            if (!string.IsNullOrEmpty(extra))
            {
                clothingController.SetExtra(extra);
            }
        }

        [ContextMenu("Start Talking")]
        public void StartTalking()
        {
            if (mouthController != null)
            {
                mouthController.StartTalking();
                this.Log("Started talking");
            }
        }

        [ContextMenu("Stop Talking")]
        public void StopTalking()
        {
            if (mouthController != null)
            {
                mouthController.StopTalking();
                this.Log("Stopped talking");
            }
        }

        [ContextMenu("Talk For Set Duration")]
        public void TalkForSetDuration()
        {
            if (mouthController != null)
            {
                mouthController.Talk(talkDurationSeconds);
                this.Log($"Talking for {talkDurationSeconds} seconds");
            }
        }

        [ContextMenu("Close Eyes")]
        public void CloseEyes()
        {
            if (eyeController != null)
            {
                eyeController.CloseEyes();
            }
        }

        [ContextMenu("Open Eyes")]
        public void OpenEyes()
        {
            if (eyeController != null)
            {
                eyeController.OpenEyes();
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
