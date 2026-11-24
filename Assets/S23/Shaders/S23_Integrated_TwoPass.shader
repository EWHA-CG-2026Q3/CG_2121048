// Two URP passes: Pass 1 Toon + Vertex Color, Pass 2 Inverted Hull Outline.
// S23 homework: derived from the professor's exact S20_Toon_Finish,
// S20_VertexColor_Finish, S20_Outline_Finish, S20_ToonOutline_Finish.
// Three effects, one Material slot, two hand-written URP Passes.
Shader "CG/S23Integrated/ToonOutlineVertexColor_TwoPass"
{
    Properties
    {
        _BaseColor ("Tint (multiply with vertex color)", Color) = (1, 1, 1, 1)
        _Ambient ("Ambient", Range(0, 1)) = 0.2
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Shininess ("Shininess", Range(1, 256)) = 32
        _ToonSteps ("Toon Steps", Range(1, 8)) = 3
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

        // PASS 1: S20 Toon lighting + S20 vertex color + Phong specular.
        Pass
        {
            Name "ToonAndVertexColor"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Ambient;
                float4 _SpecularColor;
                float _Shininess;
                float _ToonSteps;
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 color : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                Light mainLight = GetMainLight();
                float3 L = mainLight.direction;
                float3 V = normalize(GetCameraPositionWS() - input.positionWS);
                float3 R = reflect(-L, N);

                // S20_Toon_Finish: quantize the Lambertian diffuse term.
                float diffuse = max(0, dot(N, L));
                diffuse = floor(diffuse * _ToonSteps) / _ToonSteps;

                // S20_VertexColor_Finish: surface color from vertex COLOR.
                float3 surfaceColor = input.color.rgb * _BaseColor.rgb;

                // S20_Toon_Finish: preserve Phong specular highlight.
                float specular = pow(max(0, dot(R, V)), _Shininess);
                float3 color = surfaceColor * (_Ambient + diffuse * mainLight.color)
                    + _SpecularColor.rgb * specular * mainLight.color;
                return float4(color, 1);
            }
            ENDHLSL
        }

        // PASS 2: S20_Outline_Finish, exactly the same extrusion approach.
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Ambient;
                float4 _SpecularColor;
                float _Shininess;
                float _ToonSteps;
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 pushed = input.positionOS + input.normalOS * _OutlineWidth;
                output.positionCS = TransformObjectToHClip(pushed);
                return output;
            }
            float4 frag(Varyings input) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }
    }
}
