using UnityEngine;
using UnityEditor;
using System.Linq;

namespace OTS.Rendering
{
#if UNITY_EDITOR
    public class BlendModeTools : EditorWindow
    {
        private BlendMode _selectedBlendMode = BlendMode.Normal;
        private float _selectedOpacity = 1f;
        private bool _applyToSelected = true;
        private bool _applyToChildren = false;

        [MenuItem("Tools/OTS/Blend Mode Tools")]
        public static void ShowWindow()
        {
            GetWindow<BlendModeTools>("Blend Mode Tools");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Blend Mode Tools", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Settings
            _selectedBlendMode = (BlendMode)EditorGUILayout.EnumPopup("Blend Mode", _selectedBlendMode);
            _selectedOpacity = EditorGUILayout.Slider("Opacity", _selectedOpacity, 0f, 1f);
            
            EditorGUILayout.Space();
            _applyToSelected = EditorGUILayout.Toggle("Apply to Selected Objects", _applyToSelected);
            _applyToChildren = EditorGUILayout.Toggle("Include Children", _applyToChildren);

            EditorGUILayout.Space();

            // Batch operations
            EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Apply Blend Mode to Selection"))
            {
                ApplyBlendModeToSelection();
            }

            if (GUILayout.Button("Add BlendModeController to Selection"))
            {
                AddBlendModeControllersToSelection();
            }

            if (GUILayout.Button("Remove BlendModeController from Selection"))
            {
                RemoveBlendModeControllersFromSelection();
            }

            EditorGUILayout.Space();

            // Quick presets
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Multiply\nDarken")) { _selectedBlendMode = BlendMode.Multiply; ApplyBlendModeToSelection(); }
            if (GUILayout.Button("Screen\nLighten")) { _selectedBlendMode = BlendMode.Screen; ApplyBlendModeToSelection(); }
            if (GUILayout.Button("Overlay\nContrast")) { _selectedBlendMode = BlendMode.Overlay; ApplyBlendModeToSelection(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Soft Light\nSubtle")) { _selectedBlendMode = BlendMode.SoftLight; ApplyBlendModeToSelection(); }
            if (GUILayout.Button("Hard Light\nHarsh")) { _selectedBlendMode = BlendMode.HardLight; ApplyBlendModeToSelection(); }
            if (GUILayout.Button("Difference\nInvert")) { _selectedBlendMode = BlendMode.Difference; ApplyBlendModeToSelection(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Scene analysis
            EditorGUILayout.LabelField("Scene Analysis", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Find All Blend Mode Controllers"))
            {
                FindAllBlendModeControllers();
            }

            if (GUILayout.Button("Create Layer Group from Selection"))
            {
                CreateLayerGroupFromSelection();
            }

            // Info
            EditorGUILayout.Space();
            var controllers = FindObjectsOfType<BlendModeController>();
            EditorGUILayout.HelpBox($"Scene contains {controllers.Length} objects with blend modes", MessageType.Info);
        }

        private void ApplyBlendModeToSelection()
        {
            var objects = GetTargetObjects();
            int applied = 0;

            foreach (var obj in objects)
            {
                var controller = obj.GetComponent<BlendModeController>();
                if (controller == null)
                {
                    // Only add if object has SpriteRenderer
                    if (obj.GetComponent<SpriteRenderer>() != null)
                    {
                        controller = obj.AddComponent<BlendModeController>();
                    }
                }

                if (controller != null)
                {
                    // Ensure controller is properly initialized before setting values
                    controller.EnsureInitialized();
                    controller.CurrentBlendMode = _selectedBlendMode;
                    controller.Opacity = _selectedOpacity;
                    EditorUtility.SetDirty(controller);
                    applied++;
                }
            }

            Debug.Log($"Applied blend mode {_selectedBlendMode} to {applied} objects");
        }

        private void AddBlendModeControllersToSelection()
        {
            var objects = GetTargetObjects();
            int added = 0;

            foreach (var obj in objects)
            {
                if (obj.GetComponent<BlendModeController>() == null && obj.GetComponent<SpriteRenderer>() != null)
                {
                    obj.AddComponent<BlendModeController>();
                    EditorUtility.SetDirty(obj);
                    added++;
                }
            }

            Debug.Log($"Added BlendModeController to {added} objects");
        }

        private void RemoveBlendModeControllersFromSelection()
        {
            var objects = GetTargetObjects();
            int removed = 0;

            foreach (var obj in objects)
            {
                var controller = obj.GetComponent<BlendModeController>();
                if (controller != null)
                {
                    DestroyImmediate(controller);
                    EditorUtility.SetDirty(obj);
                    removed++;
                }
            }

            Debug.Log($"Removed BlendModeController from {removed} objects");
        }

        private GameObject[] GetTargetObjects()
        {
            var selected = Selection.gameObjects;
            
            if (!_applyToChildren)
            {
                return selected;
            }

            // Include children
            var allObjects = new System.Collections.Generic.List<GameObject>();
            foreach (var obj in selected)
            {
                allObjects.Add(obj);
                allObjects.AddRange(obj.GetComponentsInChildren<Transform>().Select(t => t.gameObject));
            }

            return allObjects.Distinct().ToArray();
        }

        private void FindAllBlendModeControllers()
        {
            var controllers = FindObjectsOfType<BlendModeController>();
            var gameObjects = controllers.Select(c => c.gameObject).ToArray();
            Selection.objects = gameObjects;
            
            Debug.Log($"Found and selected {controllers.Length} objects with blend modes");
        }

        private void CreateLayerGroupFromSelection()
        {
            var selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("No objects selected");
                return;
            }

            // Create group parent
            GameObject groupObj = new GameObject("Blend Mode Group");
            var group = groupObj.AddComponent<BlendModeGroup>();

            // Parent selected objects to group
            foreach (var obj in selected)
            {
                obj.transform.SetParent(groupObj.transform);
            }

            // Setup the group
            group.FindChildSprites();
            group.SetupControllers();

            Selection.activeGameObject = groupObj;
            Debug.Log($"Created blend mode group with {selected.Length} objects");
        }
    }
#endif
}
