Shader "OTS/2DOverlay/Screen"
{
    Properties {
        _MainTex ("Overlay (Sprite) Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0,1)) = 1
    }
    SubShader {
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Universal2D"="True" }
        ZWrite Off Cull Off Blend One Zero

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "PhotoshopBlendModes.cginc"

            TEXTURE2D(_MainTex);         SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);

            float4 _MainTex_ST;
            float _Opacity;

            struct VIn  { float4 pos:POSITION; float2 uv:TEXCOORD0; };
            struct VOut { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float4 sp: TEXCOORD1; };

            VOut vert (VIn v){
                VOut o;
                o.pos = TransformObjectToHClip(v.pos.xyz);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.sp  = ComputeScreenPos(o.pos);
                return o;
            }

            float4 frag (VOut i) : SV_Target {
                // Sample sprite texture
                float4 S = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                // Early exit if sprite is completely transparent
                if (S.a <= 0.001) discard;
                
                // Sample background from camera sorting layer texture
                float2 suv = i.sp.xy / i.sp.w;
                half3 B = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, suv).rgb;

                // Apply screen blend mode
                float a = saturate(S.a * _Opacity);
                half3 blended = Screen(S.rgb, B);
                half3 outRGB = lerp(B, blended, a);
                
                return float4(outRGB, 1);
            }
            ENDHLSL
        }
    }
}
