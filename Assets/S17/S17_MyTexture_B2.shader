Shader "CG/S17_MyTexture_B2"
{
    Properties { _BaseMap ("Base Map", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            sampler2D _BaseMap;
            void vert(float3 positionOS : POSITION, float2 uv : TEXCOORD0,
                      out float2 uvOut : TEXCOORD0, out float4 positionCS : SV_POSITION)
            {
                uvOut = uv;
                positionCS = TransformObjectToHClip(positionOS);
            }
            float4 frag(float2 uv : TEXCOORD0) : SV_Target
            {
                // Mission 1: B2, u in [0.25,0.50], v in [0.50,0.75]
                float2 p = uv * 0.25 + float2(0.25, 0.5);
                return tex2D(_BaseMap, p);
            }
            ENDHLSL
        }
    }
}

