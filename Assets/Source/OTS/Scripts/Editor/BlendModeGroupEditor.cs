using UnityEngine;
using UnityEditor;

namespace OTS.Rendering
{
#if UNITY_EDITOR
    [CustomEditor(typeof(BlendModeGroup))]
    public class BlendModeGroupEditor : Editor
    {
        private BlendModeGroup _target;

        private void OnEnable()
        {
            _target = (BlendModeGroup)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Children"))
            {
                _target.FindChildSprites();
                EditorUtility.SetDirty(_target);
            }
            
            if (GUILayout.Button("Setup Controllers"))
            {
                _target.SetupControllers();
                EditorUtility.SetDirty(_target);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Layer Stack"))
            {
                _target.CreateLayerStack();
            }
            
            if (GUILayout.Button("Apply Group Settings"))
            {
                _target.ApplyGroupSettings();
            }
            EditorGUILayout.EndHorizontal();

            // Quick blend mode buttons
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Blend Modes", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Normal")) _target.BatchSetBlendMode(BlendMode.Normal);
            if (GUILayout.Button("Multiply")) _target.BatchSetBlendMode(BlendMode.Multiply);
            if (GUILayout.Button("Screen")) _target.BatchSetBlendMode(BlendMode.Screen);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Overlay")) _target.BatchSetBlendMode(BlendMode.Overlay);
            if (GUILayout.Button("Soft Light")) _target.BatchSetBlendMode(BlendMode.SoftLight);
            if (GUILayout.Button("Hard Light")) _target.BatchSetBlendMode(BlendMode.HardLight);
            EditorGUILayout.EndHorizontal();

            // Individual sprite controls
            if (_target.SpriteCount > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Individual Sprite Controls", EditorStyles.boldLabel);
                
                for (int i = 0; i < _target.SpriteCount; i++)
                {
                    var controller = _target.GetController(i);
                    if (controller != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Sprite {i}:", GUILayout.Width(60));
                        
                        BlendMode newMode = (BlendMode)EditorGUILayout.EnumPopup(controller.CurrentBlendMode, GUILayout.Width(100));
                        if (newMode != controller.CurrentBlendMode)
                        {
                            controller.CurrentBlendMode = newMode;
                        }
                        
                        float newOpacity = EditorGUILayout.Slider(controller.Opacity, 0f, 1f);
                        if (!Mathf.Approximately(newOpacity, controller.Opacity))
                        {
                            controller.Opacity = newOpacity;
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            // Info display
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Managing {_target.SpriteCount} sprites with blend modes", MessageType.Info);
        }
    }
#endif
}
