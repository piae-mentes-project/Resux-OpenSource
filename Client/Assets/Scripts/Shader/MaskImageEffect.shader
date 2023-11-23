// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Resux/MaskMaskEffect"
{
	Properties
	{
		_MainTex("MainTex",2D) = "white"{}
		_Threshold("Threshold", Range(-0.75, 0.75)) = -0.2
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _LOSMaskTexture;
			sampler2D _MainTex;
			float _Threshold;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 mask = tex2D(_LOSMaskTexture,i.uv);
				float4 main = tex2D(_MainTex,i.uv);

				// _Threshold代表不可见区域的可见度。
				// 这里的mask应当是经过了模糊处理的
				float thresholdAlpha = saturate(mask.a + _Threshold);
				return mask * thresholdAlpha + main * (1 - thresholdAlpha);
			}
			ENDCG
		}
	}
}
