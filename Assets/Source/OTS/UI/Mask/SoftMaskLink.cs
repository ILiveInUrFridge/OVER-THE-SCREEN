using UnityEngine;
using BlendModes;

public class SoftMaskLink : MonoBehaviour
{
    public Texture maskRT;
    public Texture invMaskRT;

    void OnEnable() { Apply(); }
    void Apply()
    {
        var effect = GetComponent<BlendModeEffect>();
        var ext = effect?.GetComponentExtension<ComponentExtension>();
        if (ext == null || effect == null) return;

        // Decide behavior based on which RTs are assigned:
        // - Both assigned  -> use both
        // - Normal-only    -> _MaskTex = maskRT, _InvMaskTex = null
        // - Invert-only    -> _MaskTex = white (keep alpha), _InvMaskTex = invMaskRT
        // - None assigned  -> _MaskTex = white, _InvMaskTex = null (visible)
        Texture maskToApply = null;
        Texture invToApply = null;

        if (maskRT && invMaskRT)
        {
            maskToApply = maskRT;
            invToApply = invMaskRT;
        }
        else if (maskRT)
        {
            maskToApply = maskRT;
            invToApply = null; // no inversion
        }
        else if (invMaskRT)
        {
            maskToApply = Texture2D.whiteTexture; // keep base alpha
            invToApply = invMaskRT;
        }
        else
        {
            maskToApply = Texture2D.whiteTexture;
            invToApply = null;
        }

        ext.GetShaderProperty("_MaskTex")?.SetValue(maskToApply);
        ext.GetShaderProperty("_InvMaskTex")?.SetValue(invToApply);
        effect?.SetMaterialDirty();
    }
}