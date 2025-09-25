Shader "OTS/2DOverlay/UniversalBlend"
{
    Properties {
        _MainTex ("Overlay (Sprite) Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0,1)) = 1
        [KeywordEnum(Normal, Multiply, Screen, Overlay, SoftLight, HardLight, ColorDodge, ColorBurn, Darken, Lighten, Difference, Exclusion, VividLight, LinearLight, PinLight, HardMix, LinearBurn, LinearDodge, DarkerColor, LighterColor, Subtract, Divide, Add, Hue, Saturation, Color, Luminosity)] 
        _BlendMode ("Blend Mode", Float) = 0
    }
    
    SubShader {
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Universal2D"="True" }
        ZWrite Off Cull Off Blend One Zero

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _BLENDMODE_NORMAL _BLENDMODE_MULTIPLY _BLENDMODE_SCREEN _BLENDMODE_OVERLAY _BLENDMODE_SOFTLIGHT _BLENDMODE_HARDLIGHT _BLENDMODE_COLORDODGE _BLENDMODE_COLORBURN _BLENDMODE_DARKEN _BLENDMODE_LIGHTEN _BLENDMODE_DIFFERENCE _BLENDMODE_EXCLUSION _BLENDMODE_VIVIDLIGHT _BLENDMODE_LINEARLIGHT _BLENDMODE_PINLIGHT _BLENDMODE_HARDMIX _BLENDMODE_LINEARBURN _BLENDMODE_LINEARDODGE _BLENDMODE_DARKERCOLOR _BLENDMODE_LIGHTERCOLOR _BLENDMODE_SUBTRACT _BLENDMODE_DIVIDE _BLENDMODE_ADD _BLENDMODE_HUE _BLENDMODE_SATURATION _BLENDMODE_COLOR _BLENDMODE_LUMINOSITY
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "PhotoshopBlendModes.cginc"

            // Overlay sprite
            TEXTURE2D(_MainTex);         SAMPLER(sampler_MainTex);

            // Provided by URP 2D Renderer when "Camera Sorting Layer Texture" is enabled
            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);

            float4 _MainTex_ST;
            float _Opacity;
            float _BlendMode;

            struct VIn  { float4 pos:POSITION; float2 uv:TEXCOORD0; };
            struct VOut { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float4 sp: TEXCOORD1; };

            VOut vert (VIn v){
                VOut o;
                o.pos = TransformObjectToHClip(v.pos.xyz);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                // screen position for sampling the camera sorting layer texture
                o.sp  = ComputeScreenPos(o.pos);
                return o;
            }

            float3 ApplyBlendMode(float3 source, float3 destination, int blendMode)
            {
                switch(blendMode)
                {
                    case 0: return destination; // Normal (handled by alpha blending)
                    case 1: return destination * source; // Multiply
                    case 2: return Screen(source, destination);
                    case 3: return Overlay(source, destination);
                    case 4: return SoftLight(source, destination);
                    case 5: return HardLight(source, destination);
                    case 6: return ColorDodge(source, destination);
                    case 7: return ColorBurn(source, destination);
                    case 8: return min(source, destination); // Darken
                    case 9: return Lighten(source, destination);
                    case 10: return Difference(source, destination);
                    case 11: return Exclusion(source, destination);
                    case 12: return VividLight(source, destination);
                    case 13: return LinearLight(source, destination);
                    case 14: return PinLight(source, destination);
                    case 15: return HardMix(source, destination);
                    case 16: return LinearBurn(source, destination);
                    case 17: return LinearDodge(source, destination);
                    case 18: return DarkerColor(source, destination);
                    case 19: return LighterColor(source, destination);
                    case 20: return Subtract(source, destination);
                    case 21: return Divide(source, destination);
                    case 22: return Add(source, destination);
                    case 23: return Hue(source, destination);
                    case 24: return Saturation(source, destination);
                    case 25: return Color(source, destination);
                    case 26: return Luminosity(source, destination);
                    default: return destination;
                }
            }

            float4 frag (VOut i) : SV_Target {
                // Sprite color (overlay)
                float4 S = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                // Scene color below this sprite (includes other transparent sprites below by sorting layer)
                float2 suv = i.sp.xy / i.sp.w;
                float3 B = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, suv).rgb;

                // Apply blend mode
                float a = saturate(S.a * _Opacity);
                int blendModeInt = (int)_BlendMode;
                
                float3 blended = ApplyBlendMode(S.rgb, B, blendModeInt);
                float3 outRGB = lerp(B, blended, a);
                
                return float4(outRGB, 1);
            }
            ENDHLSL
        }
    }
}
