Shader "Resux/UI/UIAnimationByMask"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Threshold("Mask Threshold", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" // UI����͸��ͼƬ��������Ⱦ������ Transparent 
		    "IgnoreProjector"="True" // ����ͶӰ��Projector
		    "RenderType"="Transparent" // ��Ⱦģʽ
		    "PreviewType"="Plane" // UIԤ������������һ��ƽ��(Plane) 
		    "CanUseSpriteAtlas"="True" // UI����ʹ��ͼ��,����Ҫ����ͼ������ʹ��
	    }

        Stencil
		{
			// UI ģ����ԣ�����Properties�ж����ģ��������룬Unity���Զ��޸�
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off // �޳��ر�
		Lighting Off // ���չر�
		ZWrite Off // ���д��ر�
		ZTest[unity_GUIZTestMode] // ����UI�����shader��Ҫ����һ�䣺ZTest [unity_GUIZTestMode]����ȷ��UI����ǰ����ʾ
		Blend SrcAlpha OneMinusSrcAlpha // ���ģʽ�� OneMinusSrcAlpha ����ģʽ(͸���Ȼ��)
		// ColorMask[_ColorMask] // �������_ColorMask��������

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

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _Threshold;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask_col = tex2D(_MaskTex, i.uv);

                float threshold = _Threshold * 2 - 1;
                col.a = col.a * (mask_col.r + threshold);
                return col;
            }
            ENDCG
        }
    }
}
