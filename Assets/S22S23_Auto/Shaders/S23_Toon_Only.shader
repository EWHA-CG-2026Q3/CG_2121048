Shader "CG/S23_DemoToon_Only"
{
    Properties { _BaseColor ("Base Color", Color) = (0.8, 0.9, 1, 1) _ToonSteps ("Steps", Range(1, 8)) = 3 }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
              float4 _BaseColor;
              float _ToonSteps;
            CBUFFER_END
            struct A { float3 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; };
            V vert(A a){ V v; v.positionCS=TransformObjectToHClip(a.positionOS); v.normalWS=TransformObjectToWorldNormal(a.normalOS); return v; }
            half4 frag(V v):SV_Target
            { float d=saturate(dot(normalize(v.normalWS),GetMainLight().direction)); return half4(_BaseColor.rgb * (0.2+floor(d*_ToonSteps)/max(1.,_ToonSteps)),1); }
            ENDHLSL
        }
    }
}
