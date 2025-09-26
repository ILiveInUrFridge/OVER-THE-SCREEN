using UnityEngine;
using System.Collections.Generic;

namespace OTS.Rendering
{
    /// <summary>
    ///     Manages blend mode materials and provides caching for performance
    /// </summary>
    public class BlendModeManager : MonoBehaviour
    {
        [Header("Blend Mode Setup")]
        [SerializeField] private bool _enableCameraSortingLayerTexture = true;
        
        private static BlendModeManager _instance;
        private Dictionary<BlendMode, Material> _materialCache = new Dictionary<BlendMode, Material>();
        private Camera _mainCamera;

        public static BlendModeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BlendModeManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("BlendModeManager");
                        _instance = go.AddComponent<BlendModeManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
                _mainCamera = FindObjectOfType<Camera>();

            SetupCameraSortingLayerTexture();
        }

        private void SetupCameraSortingLayerTexture()
        {
            if (_mainCamera == null || !_enableCameraSortingLayerTexture) return;

            // Enable Camera Sorting Layer Texture in URP 2D Renderer
            var cameraData = _mainCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (cameraData != null)
            {
                // This would need to be set in the URP 2D Renderer asset
                Debug.Log("Make sure 'Camera Sorting Layer Texture' is enabled in your URP 2D Renderer asset for blend modes to work properly.");
            }
        }

        public Material GetBlendModeMaterial(BlendMode blendMode, Texture2D texture = null)
        {
            if (_materialCache.TryGetValue(blendMode, out Material cachedMaterial))
            {
                if (texture != null && cachedMaterial.mainTexture != texture)
                {
                    // Create a new instance if texture is different
                    Material newMaterial = new Material(cachedMaterial);
                    newMaterial.mainTexture = texture;
                    return newMaterial;
                }
                return cachedMaterial;
            }

            // Create new material
            string shaderName = GetShaderNameForBlendMode(blendMode);
            Shader shader = Shader.Find(shaderName);
            
            if (shader == null)
            {
                Debug.LogWarning($"Shader '{shaderName}' not found for blend mode {blendMode}. Using default sprite shader.");
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader);
            material.name = $"BlendMode_{blendMode}";
            
            if (texture != null)
                material.mainTexture = texture;

            _materialCache[blendMode] = material;
            return material;
        }

        private string GetShaderNameForBlendMode(BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Normal: return "OTS/2DOverlay/Normal";
                case BlendMode.Multiply: return "OTS/2DOverlay/Multiply";
                case BlendMode.Screen: return "OTS/2DOverlay/Screen";
                case BlendMode.Overlay: return "OTS/2DOverlay/Overlay";
                case BlendMode.SoftLight: return "OTS/2DOverlay/SoftLight";
                case BlendMode.HardLight: return "OTS/2DOverlay/HardLight";
                case BlendMode.ColorDodge: return "OTS/2DOverlay/ColorDodge";
                case BlendMode.ColorBurn: return "OTS/2DOverlay/ColorBurn";
                // Add more cases as you create more shaders
                default: return "OTS/2DOverlay/UniversalBlend";
            }
        }

        public void ClearMaterialCache()
        {
            foreach (var material in _materialCache.Values)
            {
                if (material != null)
                    DestroyImmediate(material);
            }
            _materialCache.Clear();
        }

        private void OnDestroy()
        {
            ClearMaterialCache();
        }

        // Utility methods for common blend mode operations
        public void SetGlobalBlendMode(BlendMode mode, float opacity = 1f)
        {
            var controllers = FindObjectsOfType<BlendModeController>();
            foreach (var controller in controllers)
            {
                controller.SetBlendMode(mode, opacity);
            }
        }

        public void FadeAllBlendModes(float targetOpacity, float duration)
        {
            var controllers = FindObjectsOfType<BlendModeController>();
            foreach (var controller in controllers)
            {
                controller.AnimateOpacity(targetOpacity, duration);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test All Blend Modes")]
        private void TestAllBlendModes()
        {
            var controllers = FindObjectsOfType<BlendModeController>();
            if (controllers.Length > 0)
            {
                StartCoroutine(TestBlendModesCoroutine(controllers));
            }
            else
            {
                Debug.LogWarning("No BlendModeControllers found in scene.");
            }
        }

        private System.Collections.IEnumerator TestBlendModesCoroutine(BlendModeController[] controllers)
        {
            var modes = System.Enum.GetValues(typeof(BlendMode));
            foreach (BlendMode mode in modes)
            {
                Debug.Log($"Testing blend mode: {mode}");
                foreach (var controller in controllers)
                {
                    controller.CurrentBlendMode = mode;
                }
                yield return new WaitForSeconds(1f);
            }
        }
#endif
    }
}
