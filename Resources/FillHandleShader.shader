Shader "Handle/FillHandleShader" {
	Properties {}
	SubShader
	{
		Tags {
			"Queue" = "Transparent+25000"
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
			"PreviewType" = "Plane"
		}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZTest Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


			struct vertInput {
				float4 pos		: POSITION;
				half4 color		: COLOR;
			};

			struct vertOutput {
				float4 pos		: SV_POSITION;
				half4 color		: COLOR;
			};

			vertOutput vert(vertInput IN) {
				vertOutput OUT;
				OUT.pos = UnityObjectToClipPos(IN.pos);
				OUT.color = IN.color;
				return OUT;
			}

			half4 frag(vertOutput IN) : SV_Target {
				return IN.color;
			}
			ENDCG
		}
	}
}
