using UnityEngine;
using UnityEditor;

public class SpriteImportSettings : AssetPostprocessor, ILoggable
{
    /// <summary>
    ///     Called before importing any texture asset, allowing us to customize import settings.
    /// </summary>
    private void OnPreprocessTexture()
    {
        // Check if the asset path starts with "Assets/Resources"
        if (!assetPath.StartsWith("Assets/Resources")) {
            return; // Skip if the asset is not under Assets/Resources
        }

        TextureImporter textureImporter = (TextureImporter) assetImporter;

        // Set the desired settings
        textureImporter.spriteImportMode = SpriteImportMode.Single;
        textureImporter.maxTextureSize = 4096;

        // Log the change for debugging purposes
        this.Log($"[SpriteImportSettings] Applied settings to: {assetPath}");
    }
}