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
            "Queue"="Transparent" // UI存在透明图片，所以渲染队列是 Transparent 
		    "IgnoreProjector"="True" // 忽略投影器Projector
		    "RenderType"="Transparent" // 渲染模式
		    "PreviewType"="Plane" // UI预览的正常都是一个平面(Plane) 
		    "CanUseSpriteAtlas"="True" // UI经常使用图集,所以要设置图集可以使用
	    }

        Stencil
		{
			// UI 模板测试，把在Properties中定义的模板参数导入，Unity会自动修改
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off // 剔除关闭
		Lighting Off // 光照关闭
		ZWrite Off // 深度写入关闭
		ZTest[unity_GUIZTestMode] // 用于UI组件的shader都要包含一句：ZTest [unity_GUIZTestMode]，以确保UI能在前层显示
		Blend SrcAlpha OneMinusSrcAlpha // 混合模式是 OneMinusSrcAlpha 正常模式(透明度混合)
		// ColorMask[_ColorMask] // 将定义的_ColorMask参数导入

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
