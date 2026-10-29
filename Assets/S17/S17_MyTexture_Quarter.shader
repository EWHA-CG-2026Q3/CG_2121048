Shader "CG/S17_MyTexture_Quarter"
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
                // Mission 2: show the full image only in the lower-left quarter
                float2 p = uv * 2;
                if (p.x > 1 || p.y > 1) return float4(0.5, 0.5, 0.5, 1);
                return tex2D(_BaseMap, p);
            }
            ENDHLSL
        }
    }
}

