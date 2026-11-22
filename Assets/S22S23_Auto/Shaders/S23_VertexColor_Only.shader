Shader "CG/S23_DemoVertexColor_Only"
{
    Properties { _BaseColor ("Base Color", Color) = (1, 1, 1, 1) }
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
            CBUFFER_START(UnityPerMaterial)
              float4 _BaseColor;
            CBUFFER_END
            struct A { float3 positionOS:POSITION; float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float4 color:COLOR; };
            V vert(A a){V v;v.positionCS=TransformObjectToHClip(a.positionOS);v.color=a.color;return v;}
            half4 frag(V v):SV_Target{return half4(v.color.rgb*_BaseColor.rgb,1);}
            ENDHLSL
        }
    }
}
