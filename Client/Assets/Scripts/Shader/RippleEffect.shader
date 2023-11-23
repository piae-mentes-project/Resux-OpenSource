Shader "Resux/UI/RippleEffect"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

        _Multi("Multiply", Range(0.001, 0.125)) = 0.1
        _WaveLength("WaveLength", Range(0, 0.5)) = 0.1
        _Center("UV Center", Vector) = (0.5, 0.5, 0, 0)
        _TimeMulti("Time Multiply", Range(0.5, 5)) = 1
        _ShadowColor("ShadowColor", Color) = (0.5,0.5,0.5,0)
    }
    SubShader
    {
		Tags
		{
			"Queue" = "Transparent" // UI存在透明图片，所以渲染队列是 Transparent 
			"IgnoreProjector" = "True" // 忽略投影器Projector
			"RenderType" = "Transparent" // 渲染模式
			"PreviewType" = "Plane" // UI预览的正常都是一个平面(Plane) 
			"CanUseSpriteAtlas" = "True" // UI经常使用图集,所以要设置图集可以使用
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
		ColorMask[_ColorMask] // 将定义的_ColorMask参数导入

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_UI_ALPHACLIP
            #include "UnityUI.cginc"
            #include "UnityCG.cginc"

            fixed4 _TextureSampleAdd; // Unity管理：图片格式用Alpha8 
            float4 _ClipRect;// Unity管理：2D剪裁使用
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _Center;// uv中心
            float _Multi;// 系数
            float _WaveLength;// 波长
            float _TimeMulti;// 时间系数
            float4 _ShadowColor;// 阴影色彩

            struct a2v {
                float4 vertex       : POSITION;
                float4 color        : COLOR;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex       : SV_POSITION;
                float4 color        : COLOR;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(a2v IN) {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);// 实例化处理
                OUT.vertex = UnityObjectToClipPos(IN.vertex);// 模型空间到裁剪空间
                OUT.color = IN.color;
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                return OUT;
            }
            fixed4 frag(v2f IN) :SV_Target{
                float radius = length(IN.texcoord - _Center);
                float x = radius / _WaveLength;
                float offset = cos(x * 6.28 - _Time.y * _TimeMulti);
                IN.texcoord = IN.texcoord + normalize(IN.texcoord - _Center) * offset * _Multi;
                // 直接读取纹理颜色 并且乘上 ImageColor
                half4 color = tex2D(_MainTex,IN.texcoord) * IN.color + _ShadowColor * offset;
                //fixed4 color = fixed4(IN.texcoord.x, IN.texcoord.y, 0, 1);
                return color;
            }
             ENDCG
        }
    }
}
