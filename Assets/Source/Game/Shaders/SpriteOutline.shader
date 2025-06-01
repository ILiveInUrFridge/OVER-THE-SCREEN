Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Outline)]
        _OutlineSize ("Outline Size", Range(0, 10)) = 2
        _BlurStrength ("Blur Strength", Range(0, 5)) = 1
        _OutlineAlpha ("Outline Alpha", Range(0, 1)) = 0.8
        
        [Header(Rendering)]
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _Stencil ("Stencil ID", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        // Outline Pass - Blurred duplicate
        Pass
        {
            Name "Outline"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _OutlineSize;
            float _BlurStrength;
            float _OutlineAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy;
                float4 blurredColor = float4(0, 0, 0, 0);
                
                // Multiple blur samples for gaussian-like effect
                int samples = 16;
                float sampleStep = 6.28318530718 / samples; // 2*PI / samples
                
                // Sample in concentric circles for better blur
                for (int ring = 1; ring <= _BlurStrength * 2; ring++)
                {
                    for (int j = 0; j < samples; j++)
                    {
                        float angle = sampleStep * j;
                        float2 offset = float2(cos(angle), sin(angle)) * _OutlineSize * texelSize * ring / (_BlurStrength * 2);
                        
                        float2 sampleUV = i.uv + offset;
                        float4 sample = tex2D(_MainTex, sampleUV) * _Color * i.color;
                        
                        // Weight by distance (gaussian-like falloff)
                        float weight = 1.0 / (ring * ring);
                        blurredColor += sample * weight;
                    }
                }
                
                // Normalize the blur
                blurredColor /= (samples * _BlurStrength * 2);
                
                // Apply outline alpha
                blurredColor.a *= _OutlineAlpha;
                
                return blurredColor;
            }
            ENDCG
        }

        // Main Sprite Pass
        Pass
        {
            Name "Main"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color * i.color;
                return c;
            }
            ENDCG
        }
    }
} 