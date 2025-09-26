using UnityEngine;
using UnityEngine.Rendering;

namespace OTS.Rendering
{
    public enum BlendMode
    {
        Normal,
        Multiply,
        Screen,
        Overlay,
        SoftLight,
        HardLight,
        ColorDodge,
        ColorBurn,
        Darken,
        Lighten,
        Difference,
        Exclusion,
        VividLight,
        LinearLight,
        PinLight,
        HardMix,
        LinearBurn,
        LinearDodge,
        DarkerColor,
        LighterColor,
        Subtract,
        Divide,
        Add,
        Hue,
        Saturation,
        Color,
        Luminosity
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class BlendModeController : MonoBehaviour
    {
        [Header("Blend Settings")]
        [SerializeField] private BlendMode _blendMode = BlendMode.Normal;
        [SerializeField, Range(0f, 1f)] private float _opacity = 1f;
        [SerializeField] private int _sortingLayerOffset = 0;

        private SpriteRenderer _spriteRenderer;
        private Material _originalMaterial;
        private Material _blendMaterial;
        private BlendMode _currentBlendMode;
        private float _currentOpacity;

        private static readonly string[] BlendModeShaderNames = {
            "OTS/2DOverlay/Normal",
            "OTS/2DOverlay/Multiply",
            "OTS/2DOverlay/Screen",
            "OTS/2DOverlay/Overlay",
            "OTS/2DOverlay/SoftLight",
            "OTS/2DOverlay/HardLight",
            "OTS/2DOverlay/ColorDodge",
            "OTS/2DOverlay/ColorBurn",
            "OTS/2DOverlay/Darken",
            "OTS/2DOverlay/Lighten",
            "OTS/2DOverlay/Difference",
            "OTS/2DOverlay/Exclusion",
            "OTS/2DOverlay/VividLight",
            "OTS/2DOverlay/LinearLight",
            "OTS/2DOverlay/PinLight",
            "OTS/2DOverlay/HardMix",
            "OTS/2DOverlay/LinearBurn",
            "OTS/2DOverlay/LinearDodge",
            "OTS/2DOverlay/DarkerColor",
            "OTS/2DOverlay/LighterColor",
            "OTS/2DOverlay/Subtract",
            "OTS/2DOverlay/Divide",
            "OTS/2DOverlay/Add",
            "OTS/2DOverlay/Hue",
            "OTS/2DOverlay/Saturation",
            "OTS/2DOverlay/Color",
            "OTS/2DOverlay/Luminosity"
        };

        public BlendMode CurrentBlendMode
        {
            get => _blendMode;
            set
            {
                if (_blendMode != value)
                {
                    _blendMode = value;
                    if (Application.isPlaying && _spriteRenderer != null)
                    {
                        UpdateBlendMode();
                    }
                }
            }
        }

        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = Mathf.Clamp01(value);
                if (Application.isPlaying && _spriteRenderer != null)
                {
                    UpdateOpacity();
                }
            }
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalMaterial = _spriteRenderer.material;
            
            // Ensure proper sorting for blend modes
            UpdateSortingOrder();
        }

        private void Start()
        {
            UpdateBlendMode();
            UpdateOpacity();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _spriteRenderer != null)
            {
                if (_currentBlendMode != _blendMode)
                {
                    UpdateBlendMode();
                }
                
                if (!Mathf.Approximately(_currentOpacity, _opacity))
                {
                    UpdateOpacity();
                }
            }
        }

        private void UpdateBlendMode()
        {
            // Safety check - ensure components are initialized
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                if (_spriteRenderer == null)
                {
                    Debug.LogError($"BlendModeController on {gameObject.name} requires a SpriteRenderer component!");
                    return;
                }
            }

            if (_originalMaterial == null)
            {
                _originalMaterial = _spriteRenderer.material;
                if (_originalMaterial == null)
                {
                    Debug.LogError($"SpriteRenderer on {gameObject.name} has no material!");
                    return;
                }
            }

            _currentBlendMode = _blendMode;
            
            if (_blendMode == BlendMode.Normal)
            {
                // Use original material for normal blend mode
                _spriteRenderer.material = _originalMaterial;
                if (_blendMaterial != null)
                {
                    if (Application.isPlaying)
                        Destroy(_blendMaterial);
                    else
                        DestroyImmediate(_blendMaterial);
                    _blendMaterial = null;
                }
            }
            else
            {
                // Create or update blend material
                string shaderName = BlendModeShaderNames[(int)_blendMode];
                Shader blendShader = Shader.Find(shaderName);
                
                if (blendShader != null)
                {
                    if (_blendMaterial == null)
                    {
                        _blendMaterial = new Material(blendShader);
                        _blendMaterial.name = $"BlendMaterial_{_blendMode}_{gameObject.name}";
                    }
                    else
                    {
                        _blendMaterial.shader = blendShader;
                    }
                    
                    // Copy texture from original material
                    if (_originalMaterial.HasProperty("_MainTex"))
                    {
                        _blendMaterial.mainTexture = _originalMaterial.mainTexture;
                    }
                    
                    _spriteRenderer.material = _blendMaterial;
                    UpdateOpacity();
                }
                else
                {
                    Debug.LogWarning($"Blend shader '{shaderName}' not found. Using original material.");
                    _spriteRenderer.material = _originalMaterial;
                }
            }
        }

        private void UpdateOpacity()
        {
            _currentOpacity = _opacity;
            
            if (_blendMaterial != null && _blendMaterial.HasProperty("_Opacity"))
            {
                _blendMaterial.SetFloat("_Opacity", _opacity);
            }
        }

        private void UpdateSortingOrder()
        {
            // Adjust sorting order to ensure proper layering for blend modes
            _spriteRenderer.sortingOrder += _sortingLayerOffset;
        }

        public void SetBlendMode(BlendMode mode, float opacity = -1f)
        {
            // Ensure components are initialized before setting blend mode
            EnsureInitialized();
            
            CurrentBlendMode = mode;
            if (opacity >= 0f)
            {
                Opacity = opacity;
            }
        }

        /// <summary>
        /// Ensures the component is properly initialized (useful for editor scripts)
        /// </summary>
        public void EnsureInitialized()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (_originalMaterial == null && _spriteRenderer != null)
            {
                _originalMaterial = _spriteRenderer.material;
            }
        }

        public void AnimateOpacity(float targetOpacity, float duration)
        {
            StartCoroutine(AnimateOpacityCoroutine(targetOpacity, duration));
        }

        private System.Collections.IEnumerator AnimateOpacityCoroutine(float targetOpacity, float duration)
        {
            float startOpacity = _opacity;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                Opacity = Mathf.Lerp(startOpacity, targetOpacity, t);
                yield return null;
            }

            Opacity = targetOpacity;
        }

        private void OnDestroy()
        {
            if (_blendMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(_blendMaterial);
                else
                    DestroyImmediate(_blendMaterial);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test All Blend Modes")]
        private void TestAllBlendModes()
        {
            StartCoroutine(TestBlendModesCoroutine());
        }

        private System.Collections.IEnumerator TestBlendModesCoroutine()
        {
            var modes = System.Enum.GetValues(typeof(BlendMode));
            foreach (BlendMode mode in modes)
            {
                CurrentBlendMode = mode;
                Debug.Log($"Testing blend mode: {mode}");
                yield return new WaitForSeconds(1f);
            }
        }
#endif
    }
}
