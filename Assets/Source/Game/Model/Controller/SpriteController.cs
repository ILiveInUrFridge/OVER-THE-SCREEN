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
        
        [Header("Inspector Test Controls")]
        [SerializeField] private EyeEmotion testEyeEmotion = EyeEmotion.DEFAULT;
        [SerializeField] private EyebrowEmotion testEyebrowEmotion = EyebrowEmotion.NEUTRAL;
        [SerializeField] private MouthEmotion testMouthEmotion = MouthEmotion.NEUTRAL;
        [SerializeField] private BlushStrength testBlushStrength = BlushStrength.NONE;
        [SerializeField] private float testTalkDuration = 2f;

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
            if (baseBodyRenderer == null) {
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
    }
}
