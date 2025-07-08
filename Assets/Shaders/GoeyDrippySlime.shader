Shader "Custom/GooeyDrippingSlime"
{
    Properties
    {
        _SlimeColor("Slime Color", Color) = (0.2, 1.0, 0.5, 1)
        _DeepColor("Deep Color", Color) = (0.1, 0.4, 0.2, 1)
        _RimPower("Rim Power", Range(1, 10)) = 4
        _RimStrength("Rim Strength", Range(0, 2)) = 1
        _GooFreq("Goo Frequency", Float) = 3
        _GooAmp("Goo Amplitude", Float) = 0.2
        _DropSpeed("Drop Speed", Float) = 2.0
        _DropIntensity("Drop Intensity", Float) = 0.2
        _Smoothness("Smoothness", Range(0, 1)) = 0.8
        _Thickness("Thickness", Range(0, 3)) = 1.5
        _Alpha("Alpha", Range(0,1)) = 0.6
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _SlimeColor;
            float4 _DeepColor;
            float _RimPower;
            float _RimStrength;
            float _GooFreq;
            float _GooAmp;
            float _DropSpeed;
            float _DropIntensity;
            float _Smoothness;
            float _Thickness;
            float _Alpha;

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float rim : TEXCOORD3;
            };

            // Smooth noise function using sine waves
            float SmoothGooNoise(float3 p)
            {
                float n = sin(p.x * _GooFreq + _Time.y * _DropSpeed) * 0.5;
                n += sin(p.y * _GooFreq * 1.3 + _Time.y * _DropSpeed * 0.7) * 0.25;
                n += sin(p.z * _GooFreq * 0.7 + _Time.y * _DropSpeed * 0.3) * 0.25;
                return n * 0.5 + 0.5; // Normalize to 0-1 range
            }

            // Smoother dripping function
            float SmoothDrip(float3 worldPos)
            {
                float yDrop = frac(worldPos.x * 2 + worldPos.z * 3 + _Time.y * _DropSpeed);
                float drip = smoothstep(0.8, 1.0, yDrop); // Smooth falloff
                return drip * _DropIntensity;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(IN.normalOS);

                // Get base noise value
                float noise = SmoothGooNoise(worldPos) * 2.0 - 1.0; // Remap to -1 to 1
                
                // Apply noise with normal direction
                worldPos += worldNormal * noise * _GooAmp;
                
                // Add dripping effect
                float drip = SmoothDrip(worldPos);
                worldPos.y -= drip * (1.0 + noise * 0.3); // Vary drip intensity with noise

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPos = worldPos;
                OUT.worldNormal = normalize(worldNormal);
                OUT.viewDir = normalize(GetWorldSpaceViewDir(worldPos));
                
                // Pre-calculate rim in vertex shader
                OUT.rim = 1.0 - saturate(dot(OUT.viewDir, OUT.worldNormal));

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Normalize vectors
                float3 normal = normalize(IN.worldNormal);
                float3 viewDir = normalize(IN.viewDir);
                
                // Enhanced rim lighting
                float rim = pow(IN.rim, _RimPower) * _RimStrength;
                float thickRim = pow(IN.rim, _Thickness);
                
                // Color variation based on thickness
                float3 slimeCol = lerp(_DeepColor.rgb, _SlimeColor.rgb, thickRim);
                
                // Basic lighting
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(normal, lightDir));
                float3 lighting = mainLight.color * NdotL;
                
                // Specular highlight
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normal, halfDir));
                float specular = pow(NdotH, exp2(10 * _Smoothness + 1)) * _Smoothness;
                
                // Combine all effects
                float3 finalColor = slimeCol * lighting + specular;
                finalColor += rim * _SlimeColor.rgb; // Rim glow
                
                // Alpha based on thickness
                float alpha = lerp(_Alpha * 0.5, _Alpha, thickRim);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Transparent"
}