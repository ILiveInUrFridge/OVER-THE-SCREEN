using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using OTS.Common;

namespace OTS.Scripts.Environment
{
    /// <summary>
    ///     Environment layer that just cross-fades GameObjects.
    ///     Used to control the appearance of the environment based on the time of day.
    /// </summary>
    public class EnvironmentLayer : MonoBehaviour, ILoggable
    {
        [System.Serializable]
        public class TimeVariant
        {
            public TimeOfDay timeOfDay;
            public GameObject variantObject;
        }

        [Header("Layer Configuration")]
        public EnvironmentLayerType layerType = EnvironmentLayerType.Filter;
        public bool enableDebugLogs = true;

        [Header("Transition Settings")]
        public float transitionDuration = 1f;
        public Ease transitionEase = Ease.InOutQuad;

        [Header("Time Variants")]
        [Tooltip("Default variant (always visible)")]
        public GameObject defaultVariant;

        [Tooltip("Time-specific variants")]
        public TimeVariant[] timeVariants = new TimeVariant[0];

        // Current state
        private TimeOfDay currentTime = TimeOfDay.Morning;
        private Sequence currentTransition;
		private Dictionary<SpriteRenderer, float> originalAlphas = new Dictionary<SpriteRenderer, float>();
		private Dictionary<Image, float> originalImageAlphas = new Dictionary<Image, float>();

        void Awake()
        {
            // Store original alphas for all renderers
            StoreOriginalAlphas();

            // Set initial state
            SetTimeInstant(TimeOfDay.Morning); // TODO: Replace later with actual time of the save file from Session class
        }

        /// <summary>
        ///     Store the original alpha values for all sprite renderers
        /// </summary>
        void StoreOriginalAlphas()
        {
			originalAlphas.Clear();
			originalImageAlphas.Clear();

			// Store default variant alphas from the sprite renderer
            if (defaultVariant != null)
            {
				var renderers = defaultVariant.GetComponentsInChildren<SpriteRenderer>(true);
				foreach (var renderer in renderers)
				{
					originalAlphas[renderer] = renderer.color.a;
				}

				var images = defaultVariant.GetComponentsInChildren<Image>(true);
				foreach (var img in images)
				{
					originalImageAlphas[img] = img.color.a;
				}
            }

			// Store time variant alphas from the sprite renderer
            foreach (var variant in timeVariants)
            {
                if (variant?.variantObject != null)
                {
					var renderers = variant.variantObject.GetComponentsInChildren<SpriteRenderer>(true);
					foreach (var renderer in renderers)
					{
						originalAlphas[renderer] = renderer.color.a;
					}

					var images = variant.variantObject.GetComponentsInChildren<Image>(true);
					foreach (var img in images)
					{
						originalImageAlphas[img] = img.color.a;
					}
                }
            }
        }
        
        private bool loggedOneUpdate; // field

        /// <summary>
        ///     Transition to a new time of day
        /// </summary>
        public void TransitionToTime(TimeOfDay newTime)
        {
            if (currentTime == newTime) return;

            if (enableDebugLogs)
                this.Log($"Transitioning from {currentTime} to {newTime}");

            // Kill existing transition
            currentTransition?.Kill();

            // Get what should be active for each time
            var fromActive = GetActiveVariantsFor(currentTime);
            var toActive = GetActiveVariantsFor(newTime);

            if (enableDebugLogs)
            {
                this.Log($"From active: {string.Join(", ", fromActive.Select(v => v.name))}");
                this.Log($"To active: {string.Join(", ", toActive.Select(v => v.name))}");
            }

            currentTime = newTime;

            // Create transition
            currentTransition = DOTween.Sequence()
                .SetAutoKill(true)
                .SetUpdate(UpdateType.Normal, false)
                .OnKill(() => currentTransition = null)
                .Play();
            int tweenCount = 0;

            // Fade out variants that should no longer be active
            foreach (var variant in fromActive)
            {
                if (!toActive.Contains(variant))
                {
                    if (enableDebugLogs)
                        this.Log($"Will fade out: {variant.name}");

                    FadeOutVariant(variant);
                    tweenCount++;
                }
            }

            // Fade in variants that should become active
            foreach (var variant in toActive)
            {
                if (!fromActive.Contains(variant))
                {
                    if (enableDebugLogs)
                        this.Log($"Will fade in: {variant.name}");

                    FadeInVariant(variant);
                    tweenCount++;
                }
            }

            if (enableDebugLogs)
                this.Log($"Created {tweenCount} fade operations");

            // If no tweens were added, add a dummy one to make the sequence work
            if (tweenCount == 0)
            {
                if (enableDebugLogs)
                    this.Log("No transitions needed - adding dummy interval");
                currentTransition.AppendInterval(transitionDuration);
            }

            if (enableDebugLogs)
            {
                currentTransition.OnStart(() =>
                {
                    loggedOneUpdate = false;
                    this.Log($"[ENV] seq START -> {name}, time {currentTime} -> {newTime}, tweenerCount={DOTween.TotalActiveTweeners()}");
                });

                currentTransition.OnUpdate(() =>
                {
                    if (!loggedOneUpdate)
                    {
                        loggedOneUpdate = true;
                        this.Log($"[ENV] seq UPDATE -> dur={currentTransition.Duration()}, elapsed={currentTransition.Elapsed()}");
                    }
                });

                currentTransition.OnComplete(() =>
                {
                    this.Log($"[ENV] seq COMPLETE -> {name}, duration={currentTransition.Duration()}");
                    loggedOneUpdate = false;
                });
            }

            // Deactivate faded out variants when complete
            currentTransition.OnComplete(() =>
            {
                foreach (var variant in fromActive)
                {
                    if (!toActive.Contains(variant))
                    {
                        variant.SetActive(false);
                    }
                }
                if (enableDebugLogs)
                    this.Log($"Transition to {newTime} completed");
            });
        }

        /// <summary>
        ///     Set time instantly without transition
        /// </summary>
        public void SetTimeInstant(TimeOfDay time)
        {
            currentTransition?.Kill();
            currentTime = time;

            // Deactivate all first
            if (defaultVariant) defaultVariant.SetActive(false);
            foreach (var tv in timeVariants)
                if (tv?.variantObject) tv.variantObject.SetActive(false);

            // Activate the right set
            var activeVariants = GetActiveVariantsFor(time);
            
            foreach (var variant in activeVariants)
            {
                variant.SetActive(true);
				foreach (var r in variant.GetComponentsInChildren<SpriteRenderer>(true))
				{
					var c = r.color;
					c.a = GetOriginalAlpha(r);
					r.color = c;
					DOTween.Kill(r); // make sure nothing is attached
				}

				foreach (var img in variant.GetComponentsInChildren<Image>(true))
				{
					var c = img.color;
					c.a = GetOriginalAlpha(img);
					img.color = c;
					DOTween.Kill(img);
				}
            }

            if (enableDebugLogs) this.Log($"Set to {time} instantly");
        }

        /// <summary>
        ///     Get which GameObjects should be active for a given time
        /// </summary>
        List<GameObject> GetActiveVariantsFor(TimeOfDay time)
        {
            var active = new List<GameObject>();

            // Default variant is always active
            if (defaultVariant != null)
                active.Add(defaultVariant);

            // Add time-specific variant if it exists
            foreach (var variant in timeVariants)
            {
                if (variant?.variantObject != null && variant.timeOfDay == time)
                {
                    active.Add(variant.variantObject);
                    break; // Only one variant per time
                }
            }

            return active;
        }

        /// <summary>
        ///     Fade out a variant
        /// </summary>
        void FadeOutVariant(GameObject variant)
        {
            var renderers = variant.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var r in renderers)
            {
                // Always start at original (ensures non-zero delta even if left at 0)
                float original = GetOriginalAlpha(r);
                DOTween.Kill(r);
                var c = r.color; c.a = original; r.color = c;

                currentTransition.Join(r.DOFade(0f, transitionDuration).SetEase(transitionEase));
            }

			var images = variant.GetComponentsInChildren<Image>(true);
			foreach (var img in images)
			{
				float original = GetOriginalAlpha(img);
				DOTween.Kill(img);
				var c = img.color; c.a = original; img.color = c;

				currentTransition.Join(img.DOFade(0f, transitionDuration).SetEase(transitionEase));
			}
        }

        /// <summary>
        ///     Fade in a variant
        /// </summary>
        void FadeInVariant(GameObject variant)
        {
            variant.SetActive(true);

            var renderers = variant.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var r in renderers)
            {
                float target = GetOriginalAlpha(r);

                DOTween.Kill(r);
                var c = r.color; c.a = 0f; r.color = c;     // force start at 0

                currentTransition.Join(r.DOFade(target, transitionDuration).SetEase(transitionEase));
            }

			var images = variant.GetComponentsInChildren<Image>(true);
			foreach (var img in images)
			{
				float target = GetOriginalAlpha(img);

				DOTween.Kill(img);
				var c = img.color; c.a = 0f; img.color = c; // force start at 0

				currentTransition.Join(img.DOFade(target, transitionDuration).SetEase(transitionEase));
			}
        }

        /// <summary>
        ///     Helpers to fetch the original alpha for a sprite renderer and image
        /// </summary>
		float GetOriginalAlpha(SpriteRenderer r) => originalAlphas.TryGetValue(r, out var a) ? a : 1f;
		float GetOriginalAlpha(Image img) => originalImageAlphas.TryGetValue(img, out var a) ? a : 1f;
    }
}
