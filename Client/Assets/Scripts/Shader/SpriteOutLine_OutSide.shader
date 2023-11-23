Shader "Resux/Sprites/SpriteOutline_OutSide"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _OutlineWidth("Outline Width", float) = 1
        _OutlineColor("Outline Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _AlphaValue("Alpha Value", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _MainTex_TexelSize;
            float _OutlineWidth;
            float4 _OutlineColor;
            float _AlphaValue;

            struct appdata
            {
                float4 vertex   : POSITION;
                fixed4 color    : COLOR;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 uv  : TEXCOORD0;
                half2 left : TEXCOORD1;
                half2 right : TEXCOORD2;
                half2 up : TEXCOORD3;
                half2 down : TEXCOORD5;
            };

            v2f vert(appdata i)
            {
                v2f o;
                o.vertex = o.vertex + i.normal * _OutlineWidth;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                o.left = o.uv + half2(-1, 0) * _MainTex_TexelSize.xy * _OutlineWidth;
                o.right = o.uv + half2(1, 0) * _MainTex_TexelSize.xy * _OutlineWidth;
                o.up = o.uv + half2(0, 1) * _MainTex_TexelSize.xy * _OutlineWidth;
                o.down = o.uv + half2(0, -1) * _MainTex_TexelSize.xy * _OutlineWidth;
                o.color = i.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float transparent = tex2D(_MainTex, i.left).a + tex2D(_MainTex, i.right).a + tex2D(_MainTex, i.up).a + tex2D(_MainTex, i.down).a;
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 color = col;

                if (col.a < 0.1) {
                    color = step(_AlphaValue, transparent) * _OutlineColor;
                }
                return color * i.color;
            }
            ENDCG
        }
    }
}