Shader "Ripples/Add" {
    Properties {
        
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL

        Pass {
            Name "Add"
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


			TEXTURE2D(_InteractiveTex);
			SAMPLER(sampler_InteractiveTex);

			TEXTURE2D(_CurrentRT);
            SAMPLER(sampler_CurrentRT);

			float _isRenderMousePointer;
			float4 _PositionPoint;

            v2f vert(a2v v) {
                v2f o;

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target {

				float c =  SAMPLE_TEXTURE2D(_InteractiveTex, sampler_InteractiveTex, i.uv).r + //交互相机RT
				SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT,i.uv).r +  //上一帧RT
				max(_PositionPoint.z - length(i.uv - _PositionPoint.xy)/_PositionPoint.z,0) * _isRenderMousePointer; //鼠标交互
				//float c = SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT,i.uv).r +  //上一帧RT
				//		  max(_PositionPoint.z - length(i.uv - _PositionPoint.xy)/_PositionPoint.z,0) * _isRenderMousePointer; //鼠标交互
				return c;
				
				//return length(i.uv - _PositionPoint.xy);
				//return max(_PositionPoint.z - length(i.uv - _PositionPoint.xy),0);

				//return max(_PositionPoint.z - length(i.uv - _PositionPoint.xy)/_PositionPoint.z,0);
				//return  SAMPLE_TEXTURE2D(_CurrentRT, sampler_CurrentRT,i.uv).r + max(_PositionPoint.z - length(i.uv - _PositionPoint.xy)/_PositionPoint.z,0);
            }
            ENDHLSL
        }

    }
}