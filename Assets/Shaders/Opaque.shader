//
//texcoord0.xy = uv for texturing 
//texcoord1.xy = uv for position/rotation buffer
//
Shader "BentParticles/Opaque" {
	Properties {

		_PositionBuffer("-", 2D) = "black"{}
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard vertex:vert nolightmap addshadow
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _PositionBuffer;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float2 _BufferOffset;

		struct Input {
			float2 uv_MainTex;
			half4 color : COLOR;
		};


		void vert(inout appdata_full v)
		{
			//uv coords 
			float uv = float4(v.texcoord1.xy + _BufferOffset, 0, 0);

			float4 p = tex2Dlod(_PositionBuffer, uv);
			float s = 1;

			v.vertex.xyz *= s + p.xyz;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
