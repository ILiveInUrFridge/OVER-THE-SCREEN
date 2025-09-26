using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OTS.Rendering
{
    /// <summary>
    ///     Validates the entire blend mode setup
    /// </summary>
    public class BlendModeValidator : MonoBehaviour
    {
        [ContextMenu("Validate Complete Setup")]
        public void ValidateCompleteSetup()
        {
            Debug.Log("=== COMPLETE BLEND MODE VALIDATION ===");
            
            CheckCamera();
            CheckURP2DRenderer();
            CheckSprites();
            CheckShaders();
            ProvideSolution();
        }

        private void CheckCamera()
        {
            Debug.Log("--- CAMERA CHECK ---");
            
            Camera cam = Camera.main ?? FindObjectOfType<Camera>();
            if (cam == null)
            {
                Debug.LogError("❌ No camera found!");
                return;
            }

            var cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                Debug.LogError("❌ Camera missing UniversalAdditionalCameraData!");
                return;
            }

            Debug.Log($"✅ Camera: {cam.name}");
            Debug.Log($"Clear Flags: {cam.clearFlags}");
            Debug.Log($"Background: {cam.backgroundColor}");
        }

        private void CheckURP2DRenderer()
        {
            Debug.Log("--- URP 2D RENDERER CHECK ---");
            
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets("t:Renderer2DData");
            if (guids.Length == 0)
            {
                Debug.LogError("❌ NO URP 2D RENDERER DATA FOUND!");
                Debug.LogError("This is why blend modes don't work!");
                return;
            }

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var renderer2D = UnityEditor.AssetDatabase.LoadAssetAtPath<Renderer2DData>(path);
                
                Debug.Log($"✅ Found 2D Renderer: {path}");
                
                // Check if it's actually being used
                var urpAsset = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
                if (urpAsset != null)
                {
                    Debug.Log($"URP Asset: {urpAsset.name}");
                    Debug.Log("⚠️ CHECK: Is this 2D Renderer assigned to your URP Asset?");
                }
            }
#endif
        }

        private void CheckSprites()
        {
            Debug.Log("--- SPRITE CHECK ---");
            
            var controllers = FindObjectsOfType<BlendModeController>();
            if (controllers.Length == 0)
            {
                Debug.LogError("❌ No BlendModeController components found!");
                return;
            }

            foreach (var controller in controllers)
            {
                var sr = controller.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                Debug.Log($"Sprite: {controller.name}");
                Debug.Log($"  Layer: '{sr.sortingLayerName}' (Value: {SortingLayer.GetLayerValueFromID(sr.sortingLayerID)})");
                Debug.Log($"  Blend Mode: {controller.CurrentBlendMode}");
                Debug.Log($"  Material: {sr.material?.name}");
                Debug.Log($"  Shader: {sr.material?.shader?.name}");
                
                // Check if using correct shader
                string expectedShader = GetExpectedShader(controller.CurrentBlendMode);
                if (sr.material?.shader?.name != expectedShader)
                {
                    Debug.LogWarning($"  ⚠️ Expected shader: {expectedShader}");
                }
            }
        }

        private void CheckShaders()
        {
            Debug.Log("--- SHADER CHECK ---");
            
            string[] requiredShaders = {
                "OTS/2DOverlay/Normal",
                "OTS/2DOverlay/Multiply", 
                "OTS/2DOverlay/Screen",
                "OTS/2DOverlay/Overlay"
            };

            foreach (string shaderName in requiredShaders)
            {
                var shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    Debug.Log($"✅ Found: {shaderName}");
                }
                else
                {
                    Debug.LogError($"❌ Missing: {shaderName}");
                }
            }
        }

        private string GetExpectedShader(BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Normal: return "OTS/2DOverlay/Normal";
                case BlendMode.Multiply: return "OTS/2DOverlay/Multiply";
                case BlendMode.Screen: return "OTS/2DOverlay/Screen";
                case BlendMode.Overlay: return "OTS/2DOverlay/Overlay";
                default: return "Unknown";
            }
        }

        private void ProvideSolution()
        {
            Debug.Log("--- SOLUTION ---");
            Debug.Log("If blend modes still don't work, the issue is likely:");
            Debug.Log("1. URP 2D Renderer Data not assigned to URP Asset");
            Debug.Log("2. 'Camera Sorting Layer Texture' not enabled");
            Debug.Log("3. 'Foremost Sorting Layer' not set correctly");
            Debug.Log("");
            Debug.Log("MANUAL STEPS:");
            Debug.Log("1. Go to Project Settings > Graphics");
            Debug.Log("2. Find your URP Asset, click it");
            Debug.Log("3. In Renderer List, make sure 2D Renderer Data is added");
            Debug.Log("4. Click the 2D Renderer Data asset");
            Debug.Log("5. Enable 'Camera Sorting Layer Texture'");
            Debug.Log("6. Set 'Foremost Sorting Layer' to your highest layer");
        }

        [ContextMenu("Force Fix Materials")]
        public void ForceFixMaterials()
        {
            Debug.Log("=== FORCING MATERIAL FIX ===");
            
            var controllers = FindObjectsOfType<BlendModeController>();
            foreach (var controller in controllers)
            {
                // Force refresh the blend mode
                var currentMode = controller.CurrentBlendMode;
                controller.CurrentBlendMode = BlendMode.Normal;
                controller.CurrentBlendMode = currentMode;
                
                Debug.Log($"Refreshed: {controller.name} - {currentMode}");
            }
        }
    }
}
