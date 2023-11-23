Shader "Custom/LineShader" {
	Properties {
		_Color ("Color", Color) = (1,0,0,1)
	}
	SubShader {
		Pass { 
	            Blend SrcAlpha OneMinusSrcAlpha 
			    Colormask RGBA Lighting Off 
			    ZTest LEqual  ZWrite Off Cull Off Fog { Mode Off }  Color[_Color]
	     } 
	} 
}
