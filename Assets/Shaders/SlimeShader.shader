Shader "Custom/SlimeShader"
{
    Properties
    {
        _Color ("Slime Color", Color) = (0.2, 1, 0.5, 1)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Float) = 2
        _Smoothness ("Smoothness", Range(0,1)) = 0.9
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _WobbleSpeed ("Wobble Speed", Float) = 2
        _WobbleStrength ("Wobble Strength", Float) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            float4 _Color;
            float4 _RimColor;
            float _RimPower;
            float _Smoothness;
            float _Transparency;
            float _WobbleSpeed;
            float _WobbleStrength;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float wobble = sin(worldPos.x * 4 + _Time.y * _WobbleSpeed) * _WobbleStrength;
                worldPos += IN.normalOS * wobble;

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);
                OUT.worldPos = worldPos;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(IN.viewDirWS);

                float rim = pow(1 - saturate(dot(viewDir, normal)), _RimPower);
                float3 rimLight = rim * _RimColor.rgb;

                float3 finalColor = _Color.rgb + rimLight;

                return float4(finalColor, 1 - _Transparency);
            }
            ENDHLSL
        }
    }
}
