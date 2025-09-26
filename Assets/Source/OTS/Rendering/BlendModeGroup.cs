using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace OTS.Rendering
{
    /// <summary>
    ///     Manages blend modes for multiple sprites at once - like a Photoshop layer group
    /// </summary>
    public class BlendModeGroup : MonoBehaviour
    {
        [Header("Group Settings")]
        [SerializeField] private BlendMode _groupBlendMode = BlendMode.Normal;
        [SerializeField, Range(0f, 1f)] private float _groupOpacity = 1f;
        [SerializeField] private bool _autoFindChildren = true;
        [SerializeField] private bool _inheritGroupSettings = false;

        [Header("Sprite Management")]
        [SerializeField] private List<SpriteRenderer> _sprites = new List<SpriteRenderer>();
        
        private List<BlendModeController> _controllers = new List<BlendModeController>();

        public BlendMode GroupBlendMode
        {
            get => _groupBlendMode;
            set
            {
                _groupBlendMode = value;
                ApplyGroupBlendMode();
            }
        }

        public float GroupOpacity
        {
            get => _groupOpacity;
            set
            {
                _groupOpacity = Mathf.Clamp01(value);
                ApplyGroupOpacity();
            }
        }

        private void Awake()
        {
            if (_autoFindChildren)
            {
                FindChildSprites();
            }
            
            SetupControllers();
        }

        private void Start()
        {
            ApplyGroupSettings();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyGroupSettings();
            }
        }

        /// <summary>
        /// Automatically finds all child SpriteRenderers
        /// </summary>
        [ContextMenu("Find Child Sprites")]
        public void FindChildSprites()
        {
            _sprites.Clear();
            _sprites.AddRange(GetComponentsInChildren<SpriteRenderer>());
            
            Debug.Log($"Found {_sprites.Count} sprites in children");
        }

        /// <summary>
        /// Adds BlendModeController to sprites that don't have one
        /// </summary>
        [ContextMenu("Setup Controllers")]
        public void SetupControllers()
        {
            _controllers.Clear();
            
            foreach (var sprite in _sprites)
            {
                if (sprite == null) continue;
                
                BlendModeController controller = sprite.GetComponent<BlendModeController>();
                if (controller == null)
                {
                    controller = sprite.gameObject.AddComponent<BlendModeController>();
                    Debug.Log($"Added BlendModeController to {sprite.name}");
                }
                
                _controllers.Add(controller);
            }
        }

        /// <summary>
        /// Applies group settings to all controlled sprites
        /// </summary>
        public void ApplyGroupSettings()
        {
            if (_inheritGroupSettings)
            {
                ApplyGroupBlendMode();
                ApplyGroupOpacity();
            }
        }

        private void ApplyGroupBlendMode()
        {
            foreach (var controller in _controllers)
            {
                if (controller != null)
                {
                    controller.CurrentBlendMode = _groupBlendMode;
                }
            }
        }

        private void ApplyGroupOpacity()
        {
            foreach (var controller in _controllers)
            {
                if (controller != null)
                {
                    controller.Opacity = _groupOpacity;
                }
            }
        }

        /// <summary>
        /// Sets blend mode for specific sprites in the group
        /// </summary>
        public void SetBlendModeForSprite(int index, BlendMode mode, float opacity = -1f)
        {
            if (index >= 0 && index < _controllers.Count && _controllers[index] != null)
            {
                _controllers[index].CurrentBlendMode = mode;
                if (opacity >= 0f)
                {
                    _controllers[index].Opacity = opacity;
                }
            }
        }

        /// <summary>
        /// Animates all sprites in the group
        /// </summary>
        public void AnimateGroupOpacity(float targetOpacity, float duration)
        {
            foreach (var controller in _controllers)
            {
                if (controller != null)
                {
                    controller.AnimateOpacity(targetOpacity, duration);
                }
            }
        }

        /// <summary>
        /// Creates a Photoshop-style layer setup with different blend modes
        /// </summary>
        [ContextMenu("Create Layer Stack")]
        public void CreateLayerStack()
        {
            BlendMode[] layerModes = { 
                BlendMode.Normal, 
                BlendMode.Multiply, 
                BlendMode.Screen, 
                BlendMode.Overlay 
            };

            for (int i = 0; i < _controllers.Count && i < layerModes.Length; i++)
            {
                if (_controllers[i] != null)
                {
                    _controllers[i].CurrentBlendMode = layerModes[i];
                    
                    // Set proper sorting order
                    var sprite = _controllers[i].GetComponent<SpriteRenderer>();
                    if (sprite != null)
                    {
                        sprite.sortingOrder = i;
                    }
                }
            }
            
            Debug.Log("Created Photoshop-style layer stack");
        }

        /// <summary>
        /// Batch operations for workflow efficiency
        /// </summary>
        public void BatchSetBlendMode(BlendMode mode)
        {
            foreach (var controller in _controllers)
            {
                if (controller != null)
                {
                    controller.CurrentBlendMode = mode;
                }
            }
        }

        public void BatchSetOpacity(float opacity)
        {
            foreach (var controller in _controllers)
            {
                if (controller != null)
                {
                    controller.Opacity = opacity;
                }
            }
        }

        // Individual sprite access
        public BlendModeController GetController(int index)
        {
            return (index >= 0 && index < _controllers.Count) ? _controllers[index] : null;
        }

        public int SpriteCount => _sprites.Count;

#if UNITY_EDITOR
        /// <summary>
        /// Editor helper to visualize the layer stack
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_sprites == null) return;

            for (int i = 0; i < _sprites.Count; i++)
            {
                if (_sprites[i] != null)
                {
                    Gizmos.color = Color.white;
                    Vector3 pos = _sprites[i].transform.position;
                    UnityEditor.Handles.Label(pos + Vector3.up * 0.5f, $"Layer {i}: {_sprites[i].name}");
                }
            }
        }
#endif
    }
}
