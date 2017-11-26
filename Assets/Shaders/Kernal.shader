Shader "BentParticles/Kernal" {
	Properties {
		_PositionBuffer("-", 2D) = "" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	sampler2D _PositionBuffer;

	float3 _EmitterPos;
	float3 _EmitterSize;

	// PRNG function
	float nrand(float2 uv, float salt)
	{
		uv += float2(salt, 0);
		return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
	}


	//Particle Generators
	float4 new_particle_position(float2 uv)
	{
		float3 p = float3(1, 1, 1);
		//float p = float3(nrand(uv, 1), nrand(uv, 2), nrand(uv, 3));
		p = p * _EmitterPos;
		return float4(p, 0.5);
	}

	//Initial Position
	float4 frag_init_position(v2f_img i) : SV_Target
	{
		return new_particle_position(i.uv);
	}

	ENDCG

	SubShader{
		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment frag_init_position
			ENDCG
		}
	}
}
