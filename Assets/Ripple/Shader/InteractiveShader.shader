Shader "Ripples/InteractiveShader" {
    Properties {
        _InteractiveStength("InteractiveStrength",Range(0.001,1)) = 0.01
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float _InteractiveStength;
        CBUFFER_END
        ENDHLSL

        Pass {
            Name "Interactive"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag



            struct a2v {
                float4 positionOS   : POSITION;
            };

            struct v2f {
                float4 positionCS  : SV_POSITION;
            };

            v2f vert(a2v v) {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            half4 frag(v2f i) : SV_Target {

				return _InteractiveStength;
            }
            ENDHLSL
        }


    }
}