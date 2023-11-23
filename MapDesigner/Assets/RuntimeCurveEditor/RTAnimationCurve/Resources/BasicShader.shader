Shader "Custom/BasicShader" {
	Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	}
    SubShader {
         Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            SetTexture [_MainTex] { }
        }
    }
}
