Shader "Ripples/RippleShader"
{
     Properties {
		_Attenuation("Attenuation" , Range(0,1)) = 0.99
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

  //      CBUFFER_START(UnityPerMaterial)
		//float _Attenuation;
  //      CBUFFER_END
        ENDHLSL

        Pass {
            Name "Ripple"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct a2v {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct v2f {
                float4 positionCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

			TEXTURE2D(_PrevRT);
            SAMPLER(sampler_PrevRT);

			TEXTURE2D(_CurrentRT);
            SAMPLER(sampler_CurrentRT);

			float4 _CurrentRT_TexelSize;
			float _Attenuation;

            v2f vert(a2v v) {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target {

				//最小偏移单位
				float3 e = float3(_CurrentRT_TexelSize.xy,0);
				float2 uv =i.uv;
				//获取上下左右四个值
				float p10 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv - e.zy).x;//下
				float p01 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv - e.xz).x;//左
				float p21 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv + e.xz).x;//右
				float p12 = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT, uv + e.zy).x;//上
				//中心值
				float p11 =  SAMPLE_TEXTURE2D(_PrevRT, sampler_PrevRT, uv).x;
				//计算结果
				float d = (p10 + p01 + p21 + p12) / 2 - p11;
				//衰减
				d *= _Attenuation;

				return d;
            }
            ENDHLSL
        }
    }
}
