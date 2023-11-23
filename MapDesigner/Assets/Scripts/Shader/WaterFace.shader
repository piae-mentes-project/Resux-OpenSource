// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WaterFace"
{
	Properties
	{
		_Color("Color", Color) = (0.6320754,0.6320754,0.6320754,1)
		_Tiling1("Tiling1", Vector) = (3,5,0,0)
		_Offset1("Offset1", Vector) = (90.32,45.36,0,0)
		_Tiling2("Tiling2", Vector) = (3.2,2.7,0,0)
		_Offset2("Offset2", Vector) = (8.38,9.71,0,0)
		_Scale("Scale", Range( 0.1 , 0.9)) = 0.416
	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" }
		LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha , OneMinusDstColor One
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
			};

			uniform float2 _Tiling1;
			uniform float2 _Offset1;
			uniform float _Scale;
			uniform float2 _Tiling2;
			uniform float2 _Offset2;
			uniform float4 _Color;
			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				float3 vertexValue =  float3(0,0,0) ;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 finalColor;
				float2 uv011 = i.ase_texcoord.xy * _Tiling1 + _Offset1;
				float simplePerlin2D10 = snoise( uv011 );
				float temp_output_19_0 = ( _SinTime.w + 1.0 );
				float2 uv014 = i.ase_texcoord.xy * _Tiling2 + _Offset2;
				float simplePerlin2D13 = snoise( uv014 );
				
				
				finalColor = ( ( ( step( simplePerlin2D10 , ( 0.2 + ( 0.2 * temp_output_19_0 ) ) ) * _Scale ) + ( _Scale * step( simplePerlin2D13 , ( ( temp_output_19_0 * 0.35 ) + 0.1 ) ) ) ) * _Color );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=16700
0;72.66667;924;501;-323.0026;302.5807;1;True;False
Node;AmplifyShaderEditor.SinTimeNode;18;-1285.418,-143.0005;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;20;-1279.582,-4.676403;Float;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;15;-867.9719,222.6098;Float;False;Property;_Tiling2;Tiling2;3;0;Create;True;0;0;False;0;3.2,2.7;3.2,2.7;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;12;-824.0468,-501.4863;Float;False;Property;_Tiling1;Tiling1;1;0;Create;True;0;0;False;0;3,5;3,5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;16;-854.7732,-382.4398;Float;False;Property;_Offset1;Offset1;2;0;Create;True;0;0;False;0;90.32,45.36;90.32,45.36;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-1093.582,-66.67641;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1093.209,42.01894;Float;False;Constant;_Float2;Float 2;0;0;Create;True;0;0;False;0;0.35;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1079.517,-165.0389;Float;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;False;0;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;17;-867.1852,345.8297;Float;False;Property;_Offset2;Offset2;4;0;Create;True;0;0;False;0;8.38,9.71;8.38,9.71;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-617.2244,-456.9011;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-656.0339,254.5412;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-874.2551,-6.35794;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-875.4161,-144.2389;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-872.6086,106.8985;Float;False;Constant;_Float3;Float 3;0;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-889.8826,-220.6761;Float;False;Constant;_Float4;Float 4;0;0;Create;True;0;0;False;0;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-602.2362,36.66763;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-611.8812,-167.5568;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;13;-367.1369,198.5993;Float;True;Simplex2D;1;0;FLOAT2;256,256;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;10;-360.9904,-495.3004;Float;True;Simplex2D;1;0;FLOAT2;256,256;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;30;14.1462,62.20972;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;29;14.44875,-332.8352;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;1.566649,-84.41289;Float;False;Property;_Scale;Scale;5;0;Create;True;0;0;False;0;0.416;0;0.1;0.9;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;291.1022,23.52782;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;287.145,-226.1942;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;451.5486,-93.13578;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;31;299.5407,150.1908;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;0.6320754,0.6320754,0.6320754,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;643.4749,-91.56686;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;882,-163;Float;False;True;2;Float;ASEMaterialInspector;0;1;WaterFace;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;2;5;False;-1;10;False;-1;5;4;False;-1;1;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Transparent=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;19;0;18;4
WireConnection;19;1;20;0
WireConnection;11;0;12;0
WireConnection;11;1;16;0
WireConnection;14;0;15;0
WireConnection;14;1;17;0
WireConnection;22;0;19;0
WireConnection;22;1;24;0
WireConnection;21;0;23;0
WireConnection;21;1;19;0
WireConnection;26;0;22;0
WireConnection;26;1;27;0
WireConnection;25;0;28;0
WireConnection;25;1;21;0
WireConnection;13;0;14;0
WireConnection;10;0;11;0
WireConnection;30;0;13;0
WireConnection;30;1;26;0
WireConnection;29;0;10;0
WireConnection;29;1;25;0
WireConnection;32;0;36;0
WireConnection;32;1;30;0
WireConnection;33;0;29;0
WireConnection;33;1;36;0
WireConnection;34;0;33;0
WireConnection;34;1;32;0
WireConnection;35;0;34;0
WireConnection;35;1;31;0
WireConnection;2;0;35;0
ASEEND*/
//CHKSM=0E473453722283719E6165802AB25DCDD66FE306