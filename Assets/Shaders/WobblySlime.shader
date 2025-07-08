// Shader "Custom/WobblySlime"
// {
//     Properties
//     {
//         _Color ("Slime Color", Color) = (0.2, 1, 0.6, 1)
//         _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
//         _RimPower ("Rim Power", Range(1, 5)) = 2.5
//         _WobbleSpeed ("Wobble Speed", Float) = 2
//         _WobbleAmount ("Wobble Amount", Float) = 0.1
//         _NoiseScale ("Noise Scale", Float) = 2.0
//         _Transparency ("Alpha", Range(0,1)) = 0.2
//     }

//     SubShader
//     {
//         Tags { "RenderType"="Transparent" "Queue"="Transparent" }
//         LOD 200
//         Blend SrcAlpha OneMinusSrcAlpha
//         ZWrite Off
//         Cull Off

//         Pass
//         {
//             HLSLPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//             float4 _Color;
//             float4 _RimColor;
//             float _RimPower;
//             float _WobbleSpeed;
//             float _WobbleAmount;
//             float _NoiseScale;
//             float _Transparency;

//             float hash(float3 p)
//             {
//                 return frac(sin(dot(p ,float3(12.9898,78.233, 37.719))) * 43758.5453);
//             }

//             float noise(float3 p)
//             {
//                 float3 i = floor(p);
//                 float3 f = frac(p);
//                 f = f * f * (3.0 - 2.0 * f);
//                 float n =
//                     lerp(lerp(lerp(hash(i + float3(0,0,0)), hash(i + float3(1,0,0)), f.x),
//                               lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x), f.y),
//                          lerp(lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
//                               lerp(hash(i + float3(0,1,1)), hash(i + float3(1,1,1)), f.x), f.y), f.z);
//                 return n;
//             }

//             struct Attributes
//             {
//                 float4 positionOS : POSITION;
//                 float3 normalOS : NORMAL;
//             };

//             struct Varyings
//             {
//                 float4 positionHCS : SV_POSITION;
//                 float3 worldNormal : TEXCOORD0;
//                 float3 viewDir : TEXCOORD1;
//                 float3 worldPos : TEXCOORD2;
//             };

//             Varyings vert(Attributes IN)
//             {
//                 Varyings OUT;

//                 float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
//                 float3 worldNormal = TransformObjectToWorldNormal(IN.normalOS);

//                 float n = noise(worldPos * _NoiseScale + _Time.y * _WobbleSpeed);
//                 worldPos += worldNormal * (n - 0.5) * _WobbleAmount;

//                 OUT.positionHCS = TransformWorldToHClip(worldPos);
//                 OUT.worldNormal = normalize(worldNormal);
//                 OUT.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
//                 OUT.worldPos = worldPos;
//                 return OUT;
//             }

//             half4 frag(Varyings IN) : SV_Target
//             {
//                 float rim = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _RimPower);
//                 float3 color = _Color.rgb + _RimColor.rgb * rim;
//                 return float4(color, 1.0 - _Transparency);
//             }
//             ENDHLSL
//         }
//     }
// }

Shader "Custom/WobblySlime"
{
    Properties
    {
        _Color ("Slime Color", Color) = (0.3, 1, 0.6, 1)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(1, 5)) = 2.5
        _WobbleSpeed ("Wobble Speed", Float) = 2
        _WobbleAmount ("Wobble Amount", Float) = 0.12
        _DripSpeed ("Drip Speed", Float) = 1.5
        _DripStrength ("Drip Strength", Float) = 0.08
        _NoiseScale ("Noise Scale", Float) = 1.5
        _Transparency ("Alpha", Range(0,1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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

            float4 _Color;
            float4 _RimColor;
            float _RimPower;
            float _WobbleSpeed;
            float _WobbleAmount;
            float _DripSpeed;
            float _DripStrength;
            float _NoiseScale;
            float _Transparency;

            // Smooth noise (3D)
            float hash(float3 p) { return frac(sin(dot(p, float3(127.1, 311.7, 74.7))) * 43758.5453); }

            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float n = lerp(
                    lerp(
                        lerp(hash(i + float3(0, 0, 0)), hash(i + float3(1, 0, 0)), f.x),
                        lerp(hash(i + float3(0, 1, 0)), hash(i + float3(1, 1, 0)), f.x), f.y),
                    lerp(
                        lerp(hash(i + float3(0, 0, 1)), hash(i + float3(1, 0, 1)), f.x),
                        lerp(hash(i + float3(0, 1, 1)), hash(i + float3(1, 1, 1)), f.x), f.y),
                    f.z);
                return n;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // Sticky Wobble Noise
                float n = noise(worldPos * _NoiseScale + _Time.y * _WobbleSpeed);
                worldPos += normalWS * (n - 0.5) * _WobbleAmount;

                // Drip Motion: vertical sine warping
                float drip = sin(_Time.y * _DripSpeed + worldPos.x * 2.0 + worldPos.z * 2.0);
                worldPos.y -= drip * _DripStrength;

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldNormal = normalize(normalWS);
                OUT.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                OUT.worldPos = worldPos;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float rim = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _RimPower);
                float3 color = _Color.rgb + _RimColor.rgb * rim;
                return float4(color, 1.0 - _Transparency);
            }
            ENDHLSL
        }
    }
}
