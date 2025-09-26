using UnityEngine;

namespace OTS.Rendering
{
    /// <summary>
    ///     Simple setup for blend modes - just the essentials
    /// </summary>
    public class SimpleBlendModeSetup : MonoBehaviour
    {
        [Header("Quick Setup")]
        [TextArea(3, 5)]
        [SerializeField] private string _instructions = 
            "1. Enable 'Camera Sorting Layer Texture' in your URP 2D Renderer Data\n" +
            "2. Set 'Foremost Sorting Layer' to your highest layer\n" +
            "3. Add BlendModeController to sprites that need blend modes";

        [ContextMenu("Create Test Sprites")]
        public void CreateTestSprites()
        {
            // Simple test sprite creation
            CreateSprite("Background", Color.red, BlendMode.Normal, new Vector3(-1, -1, 0));
            CreateSprite("Post Process 1", Color.green, BlendMode.Multiply, new Vector3(0, -1, 0));
            CreateSprite("Post Process 2", Color.blue, BlendMode.Screen, new Vector3(-0.5f, 0, 0));
            CreateSprite("Post Process 3", Color.yellow, BlendMode.Overlay, new Vector3(0.5f, 0, 0));
            
            Debug.Log("Created test sprites. Make sure your sorting layers are set up correctly!");
        }

        private void CreateSprite(string layerName, Color color, BlendMode blendMode, Vector3 position)
        {
            GameObject obj = new GameObject($"TestSprite_{layerName}");
            obj.transform.position = position;
            
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(color);
            sr.sortingLayerName = layerName;
            
            BlendModeController controller = obj.AddComponent<BlendModeController>();
            controller.CurrentBlendMode = blendMode;
            controller.Opacity = 0.8f;
        }

        private Sprite CreateCircleSprite(Color color)
        {
            int size = 256;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.4f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = distance <= radius ? 1f : 0f;
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.DontSave;
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }
    }
}
