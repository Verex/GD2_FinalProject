Shader "Canvas Flow Examples/Directional Colors"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		_Light0Color ("Light 0 Color", Color) = (1.0, 1.0, 1.0)
		_Light1Color ("Light 1 Color", Color) = (1.0, 1.0, 1.0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			static const float3 kLight0Direction = half3(1, -1, 1);
			static const float3 kLight1Direction = half3(-1, -1, 1);

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed3 litColor : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Light0Color;
			fixed4 _Light1Color;

			float alignmentFactorBetweenVectors(float3 direction0, float3 direction1)
			{
				return saturate(dot(direction0, direction1) * -1.0);
			}

			fixed3 lightingContribution(fixed3 lightColor, float3 lightDirection, float3 vertexWorldNormal)
			{
				float lightToVertexAlignmentFactor = alignmentFactorBetweenVectors(lightDirection, vertexWorldNormal);
				return lightToVertexAlignmentFactor * lightColor;
			}

			v2f vert (appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 vertexWorldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 light0Color = lightingContribution(_Light0Color, kLight0Direction, vertexWorldNormal);
				fixed3 light1Color = lightingContribution(_Light1Color, kLight1Direction, vertexWorldNormal);

				o.litColor = light0Color + light1Color;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 result = tex2D(_MainTex, i.uv) * fixed4(i.litColor, 1.0);
				return result;
			}
			ENDCG
		}
	}
}
