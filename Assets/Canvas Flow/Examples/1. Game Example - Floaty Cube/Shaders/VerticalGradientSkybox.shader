Shader "Canvas Flow Examples/Vertical Gradient Skybox"
{
	Properties
	{
        _Color1 ("Top Color", Color) = (0.97, 0.67, 0.51, 0)
        _Color0 ("Bottom Color", Color) = (0, 0.7, 0.74, 0)

        _Span01 ("Span", Range(0, 1)) = 1.0
        _Offset ("Offset", Range(-1, 1)) = 0.0
	}
	SubShader
	{
        Tags { "RenderType"="Background" "Queue"="Background" }

		Pass
		{
			ZWrite Off
            Cull Off
            Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
       			float3 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
       			float3 texcoord : TEXCOORD0;
			};

			fixed4 _Color0;
			fixed4 _Color1;

			float _Span01;
			float _Offset;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
      			o.texcoord = v.texcoord;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float vertical01 = (i.texcoord.y + 1) * 0.5;

				float halfSpan01 = (_Span01 * 0.5);
				float min01 = 0.5 - halfSpan01;
				float max01 = 0.5 + halfSpan01;
				float gradient01 = smoothstep(min01, max01, vertical01 + _Offset);
                
				fixed4 result = lerp(_Color0, _Color1, gradient01);
				return result;
			}
			ENDCG
		}
	}
}
