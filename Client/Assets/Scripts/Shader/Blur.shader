Shader "Yeeeeeeee/Blur"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_BlurSize("BlurSize", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque"  "Queue" = "Geometry"}
		CGINCLUDE
		#include "UnityCG.cginc"
		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		float _BlurSize;

		struct a2v
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float2 offset : TEXCOORD1;
		};

		v2f VerticalBlur (a2v v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;

			o.offset = float2(_MainTex_TexelSize.y * 1.0, _MainTex_TexelSize.y * -1.0) * _BlurSize;

			return o;
		}

		v2f HorizontalBlur (a2v v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;

			o.offset = float2(_MainTex_TexelSize.y * 1.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;

			return o;
		}

		fixed4 frag (v2f i) : SV_Target
		{
			float weight[2] = { 0.4511, 0.2741 };
			fixed3 sum = tex2D(_MainTex, i.uv).rgb * weight[0];
			sum += tex2D(_MainTex, i.uv + i.offset).rgb * weight[1];
			sum += tex2D(_MainTex, i.uv - i.offset).rgb * weight[1];
			return fixed4(sum, 1);
		}
		ENDCG

		ZWrite Off ZTest Off Cull Off

		pass
		{
			NAME "VerticalBlur"
			CGPROGRAM
			#pragma vertex VerticalBlur
			#pragma fragment frag
			ENDCG
		}

		pass
		{
			NAME "HorizontalBlur"
			CGPROGRAM
			#pragma vertex HorizontalBlur
			#pragma fragment frag
			ENDCG
		}
	}
	FallBack "Diffuse"
}
