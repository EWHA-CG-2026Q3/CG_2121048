Shader "CG/S23_DemoOutline_Only"
{
    Properties { _OutlineWidth ("Outline Width", Range(0, 0.2)) = 0.07 _OutlineColor ("Color", Color) = (0.01, 0.01, 0.03, 1) }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
              float _OutlineWidth;
              float4 _OutlineColor;
            CBUFFER_END
            struct A { float3 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; };
            V vert(A a){V v;v.positionCS=TransformObjectToHClip(a.positionOS+a.normalOS*_OutlineWidth);return v;}
            half4 frag(V v):SV_Target{return half4(_OutlineColor);}
            ENDHLSL
        }
    }
}
