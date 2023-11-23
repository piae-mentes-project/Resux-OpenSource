// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Green"
{
    Properties
    {
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Texture", 2D) = "white" {}
		_Cutoff("alpha test cutoff", Range(0,2)) = 2
    }
    SubShader
    {
        Tags { "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
        LOD 200

        Pass
        {
			//Cull Off
            Tags{"LightMode" = "ForwardBase"}
			CGPROGRAM
            
			#pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1);
                float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				SHADOW_COORDS(2)
            };

			float4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _Cutoff;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                //UNITY_TRANSFER_FOG(o,o.vertex);
				TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				//fixed3 atten;
				//UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
				
				float gray = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;
				//float gray = col.r * 0.863 + col.g * 0.871 + col.b * 0.867;
				float rate = (float)((col.g + col.r) / col.b);
				
				clip(rate - _Cutoff);
				
                return col*_Color;
            }
            ENDCG
        }
		// Pass to render object as a shadow caster
		Pass{
				Name "Caster"
				Tags{ "LightMode" = "ShadowCaster" }

				CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0
#pragma multi_compile_shadowcaster
#pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
				float2  uv : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float4 _MainTex_ST;

			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			uniform sampler2D _MainTex;
			uniform fixed _Cutoff;
			uniform fixed4 _Color;

			float4 frag(v2f i) : SV_Target
			{
				fixed4 texcol = tex2D(_MainTex, i.uv);
				float rate = (float)((texcol.g + texcol.r) / texcol.b);

				clip(rate - _Cutoff);

			SHADOW_CASTER_FRAGMENT(i)
			}
				ENDCG

			}
    }
		Fallback "Transparent/Cutout/VertexLit"
}
