Shader "CG/Mid_MoonGardenMoonSoft"
{
    Properties
    {
        _MainTex ("Painted Moon", 2D) = "white" {}
        _Tint ("Moon Light", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _Tint;
            struct Attributes { float3 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS=TransformObjectToHClip(v.positionOS);
                o.uv=v.uv;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                half4 color=SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return half4(color.rgb*_Tint.rgb, color.a*_Tint.a);
            }
            ENDHLSL
        }
    }
}

