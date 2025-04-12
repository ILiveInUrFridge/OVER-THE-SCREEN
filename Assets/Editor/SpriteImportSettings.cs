using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.PSD;
using Utilities;

public class SpriteImportSettings : AssetPostprocessor, ILoggable
{
    /// <summary>
    ///     Called before importing any texture asset, allowing us to customize import settings.
    /// </summary>
    private void OnPreprocessTexture()
    {
        // If the asset isn't under "Assets/Resources", skip it
        if (!assetPath.StartsWith("Assets/Resources"))
        {
            return;
        }

        // Not sure if this is properly working, so I'll just do it manually?
        // // Check the extension of the asset uploaded
        // string extension = System.IO.Path.GetExtension(assetPath).ToLowerInvariant();
        // bool isPhotoshopDocument = (extension == ".psd" || extension == ".psb");

        // if (isPhotoshopDocument)
        // {
        //     // If the importer at this path is NOT a PSDImporter, override it
        //     var importerAtPath = AssetImporter.GetAtPath(assetPath);
        //     if (!(importerAtPath is PSDImporter))
        //     {
        //         // Force Reimport
        //         AssetDatabase.SetImporterOverride<PSDImporter>(assetPath);
        //         AssetDatabase.WriteImportSettingsIfDirty(assetPath);
        //         AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        //         this.Log($"Overrode importer with PSDImporter for: {assetPath}");
        //     }            
        //     return;
        // }

        TextureImporter textureImporter = (TextureImporter)assetImporter;

        // Single sprite import mode (no sprite sheet, because who the fuck?)
        textureImporter.spriteImportMode = SpriteImportMode.Single;

        // Max texture size (4K Native)
        textureImporter.maxTextureSize = 4096;

        // Log the change for debugging purposes
        this.Log($"Applied settings to: {assetPath}");
    }
}