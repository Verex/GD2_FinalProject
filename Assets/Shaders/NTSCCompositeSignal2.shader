Shader "Hidden/NTSCCompositeSignal2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#define YIQ_to_RGB   transpose(float3x3( 1.0 , 1.0  , 1.0 , 0.9563 , -0.2721 , -1.1070 , 0.6210 , -0.6474 , 1.7046 ))
			#define pi        3.14159265358

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


			float d(float x, float b)
			{
				return pi*b*min(abs(x)+0.5,1.0/b);
			}

			float e(float x, float b)
			{
				return (pi*b*min(max(abs(x)-0.5,-1.0/b),1.0/b));
			}

			float STU(float x, float b)
			{
				return ((d(x,b)+sin(d(x,b))-e(x,b)-sin(e(x,b)))/(2.0*pi));
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float signalResolution=256.0;
				float signalResolutionI=83.0;
				float signalResolutionQ=25.0;

				float2 videoSize = float2(256.0, 240.0);
				float2 textureSize = float2(256.0, 240.0);
				float offset = frac((i.uv.x * textureSize.x) - 0.5);
				float3 tempColor = float3(0, 0, 0);
				float X;
				float3 c;
				float range=ceil(0.5+videoSize.x/min(min(signalResolution,signalResolutionI),signalResolutionQ));
				
				for(float itr=-range;itr<range+2.0;itr++) {
					X = (offset-(itr));
					c = tex2D(_MainTex, float2(i.uv.x - X/textureSize.x, i.uv.y)).rgb;
					tempColor += float3((c.x*STU(X,(signalResolution/videoSize.x))),(c.y*STU(X,(signalResolutionI/videoSize.x))),(c.z*STU(X,(signalResolutionQ/videoSize.x))));
				}


				tempColor=clamp(mul(YIQ_to_RGB, tempColor), 0.0, 1.0);
				// just invert the colors
				return float4(tempColor, 1.0);
			}
			ENDCG
		}
	}
}
