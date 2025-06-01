Shader "Custom/CRTEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineCount ("Scanline Count", Range(1, 1000)) = 240
        _LineWidth ("Scanline Width", Range(0, 1)) = 0.5
        _Offset ("Scanline Offset", Range(0, 1)) = 0
        _Intensity ("Scanline Intensity", Range(0, 1)) = 0.5
        _Alpha ("Overall Alpha", Range(0, 1)) = 0.3
        
        // Glitch effect properties
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _GlitchSpeed ("Glitch Speed", Range(0, 10)) = 3.0
        
        // Fish-eye/CRT distortion
        _CurvatureX ("Curvature X", Range(0, 10)) = 0.1
        _CurvatureY ("Curvature Y", Range(0, 10)) = 0.1
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.2
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.1)) = 0.003
    }
    SubShader
    {
        Tags { "RenderType"="Overlay" "Queue"="Overlay" "IgnoreProjector"="True" }
        LOD 100

        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        
        // Remove GrabPass and use _CameraOpaqueTexture instead
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            // We'll use the screen texture directly instead of _GrabTexture
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraOpaqueTexture);
            float4 _MainTex_ST;
            float _LineCount;
            float _LineWidth;
            float _Offset;
            float _Intensity;
            float _Alpha;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _CurvatureX;
            float _CurvatureY;
            float _VignetteIntensity;
            float _ChromaticAberration;

            // Hash function for random values
            float hash(float2 p)
            {
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453123);
            }

            // Random noise function
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Time-based variables for animation
                float time = _Time.y;
                
                // Calculate fish-eye/CRT distortion
                float2 center = i.uv * 2.0 - 1.0;
                
                // Calculate distortion
                float r2 = center.x * center.x + center.y * center.y;
                float f = 1.0 + r2 * (_CurvatureX * center.x * center.x + _CurvatureY * center.y * center.y);
                
                // Apply distortion to the UVs
                float2 distortedUV = center * f;
                distortedUV = distortedUV * 0.5 + 0.5; // Back to 0-1 range
                
                // Glitch effect
                float glitchTime = floor(time * _GlitchSpeed) * 0.1;
                float glitchNoise = noise(float2(glitchTime, 2.0 * glitchTime));
                
                // Apply glitch to the UVs
                float glitchAmount = pow(glitchNoise, 20.0) * _GlitchIntensity;
                float lineNoise = pow(noise(float2(glitchTime * 5.0, glitchTime * 10.0 + distortedUV.y * 30.0)), 5.0);
                float glitchLine = step(0.98, lineNoise) * glitchAmount;
                
                // Horizontal glitch
                distortedUV.x += glitchLine * 0.1;
                
                // Random block glitches
                if (glitchNoise > 0.75 && glitchAmount > 0.05) {
                    float blockNoiseX = floor(distortedUV.x * 20.0) / 20.0;
                    float blockNoiseY = floor(distortedUV.y * 20.0) / 20.0;
                    float blockNoise = noise(float2(blockNoiseX, blockNoiseY + glitchTime));
                    
                    if (blockNoise > 0.8) {
                        distortedUV.x = frac(distortedUV.x + 0.1 * blockNoise);
                    }
                }
                
                // Chromatic aberration for the main texture
                float2 redOffset = distortedUV + float2(_ChromaticAberration, 0);
                float2 greenOffset = distortedUV;
                float2 blueOffset = distortedUV - float2(_ChromaticAberration, 0);
                
                // Sample the main texture with chromatic aberration
                fixed4 mainColR = tex2D(_MainTex, redOffset);
                fixed4 mainColG = tex2D(_MainTex, greenOffset);
                fixed4 mainColB = tex2D(_MainTex, blueOffset);
                fixed4 mainCol = fixed4(mainColR.r, mainColG.g, mainColB.b, mainColG.a);
                
                // Calculate screen position with chromatic aberration
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 screenRedOffset = screenUV + float2(_ChromaticAberration, 0) * 0.5;
                float2 screenGreenOffset = screenUV;
                float2 screenBlueOffset = screenUV - float2(_ChromaticAberration, 0) * 0.5;
                
                // Sample screen texture directly - fallback to regular UVs if _CameraOpaqueTexture not available
                fixed4 screenR = tex2D(_MainTex, screenRedOffset); // Fallback
                fixed4 screenG = tex2D(_MainTex, screenGreenOffset); // Fallback
                fixed4 screenB = tex2D(_MainTex, screenBlueOffset); // Fallback
                
                #if !defined(SHADER_API_GLES)
                    // On desktop/console, use screen texture when available
                    screenR = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraOpaqueTexture, screenRedOffset);
                    screenG = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraOpaqueTexture, screenGreenOffset);
                    screenB = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraOpaqueTexture, screenBlueOffset);
                #endif
                
                fixed4 screenCol = fixed4(screenR.r, screenG.g, screenB.b, 1.0);
                
                // Blend the textures - use main texture if it has alpha, otherwise use screen
                fixed4 col = mainCol.a > 0.01 ? mainCol : screenCol;
                
                // If using the main texture, apply its alpha
                if (mainCol.a > 0.01) {
                    col.a = mainCol.a;
                } else {
                    col.a = _Alpha; // Use the global alpha setting for screen portions
                }
                
                col *= i.color; // Multiply by vertex color to support Image color changes
                
                // Calculate scanline effect
                float scanline = frac(distortedUV.y * _LineCount + _Offset + time * 0.5);
                float scanlineEffect = smoothstep(0.0, _LineWidth, abs(scanline - 0.5) * 2.0);
                
                // Apply scanline effect with some random jitter
                float scanlineJitter = noise(float2(time * 10.0, distortedUV.y * 200.0)) * 0.02 * _GlitchIntensity;
                col.rgb = lerp(col.rgb, col.rgb * (1.0 - _Intensity + scanlineJitter), scanlineEffect);
                
                // Add some vertical noise bands to simulate interference
                float verticalNoise = noise(float2(floor(distortedUV.x * 50.0) / 50.0, time)) * _GlitchIntensity;
                col.rgb = lerp(col.rgb, col.rgb * (1.0 - verticalNoise * 2.0), verticalNoise > 0.7 ? 0.2 : 0.0);
                
                // Vignette effect
                float vignette = 1.0 - r2 * _VignetteIntensity;
                col.rgb *= vignette;
                
                // CRT phosphor glow effect
                float glow = 0.05 * _Intensity;
                col.rgb += col.rgb * glow;
                
                // CRT noise (static)
                float staticNoise = noise(float2(distortedUV.x * 100.0, distortedUV.y * 100.0 + time * 5.0)) * 0.03;
                col.rgb += staticNoise * _GlitchIntensity;
                
                // Ensure the image stays visible by boosting RGB values slightly
                col.rgb = saturate(col.rgb * 1.1);
                
                return col;
            }
            ENDCG
        }
    }
    
    // Fallback for older systems
    Fallback "UI/Default"
} 