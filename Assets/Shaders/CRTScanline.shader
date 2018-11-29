Shader "FX/CRTScanline"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[Toggle(USE_SCANLINES)] _isScanLinesEnabled("Use Scanlines", Float) = 0
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

			#pragma shader_feature USE_SCANLINES

			#define pi        3.14159265358
			#define normalGauss(x) ((exp(-(x)*(x)*0.5))/sqrt(2.0*pi))
			
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

			uniform float signalResolution;
			uniform float signalResolutionI;
			uniform float signalResolutionQ;

			uniform float2 videoSize;
			uniform float2 textureSize;
			uniform float2 outputSize;

			uniform float blackLevel;
			uniform float contrast;
			uniform float tvVerticalResolution;
			
			sampler2D _MainTex;

			float normalGaussIntegral(float x)
			{
				float a1 = 0.4361836;
				float a2 = -0.1201676;
				float a3 = 0.9372980;
				float p = 0.3326700;
				float t = 1.0 / (1.0 + p*abs(x));
				return (0.5-normalGauss(x) * (t*(a1 + t*(a2 + a3*t))))*sign(x);
			}
			float3 scanlines( float x , float3 c, float2 videoSize, float2 outputSize){
				float temp=sqrt(2*pi)*(tvVerticalResolution/videoSize.y);
				float rrr=0.5*(videoSize.y/outputSize.y);
				float x1=(x+rrr)*temp;
				float x2=(x-rrr)*temp;
				c.r=(c.r*(normalGaussIntegral(x1)-normalGaussIntegral(x2)));
				c.g=(c.g*(normalGaussIntegral(x1)-normalGaussIntegral(x2)));
				c.b=(c.b*(normalGaussIntegral(x1)-normalGaussIntegral(x2)));
				c*=(outputSize.y/videoSize.y);
				return c;
			}

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

				signalResolution=256.0;
				signalResolutionI=100.0;
				signalResolutionQ=25.0;

				videoSize = float2(256.0, 240.0);
				textureSize = float2(256.0, 240.0);
				outputSize = float2(256.0 * 2, 240.0 * 2);

				blackLevel = 0.00575;
				contrast=0.95;
				tvVerticalResolution=512.0;

				float2 offset=frac((i.uv.xy*textureSize.xy)-0.5);
				float3 tempColor=float3(0.0,0.0,0.0);
				float3 Cj;
				float range=ceil(0.5+videoSize.y/tvVerticalResolution);

				#ifdef USE_SCANLINES
				for(float itr=-range;itr<range+2.0;itr++) {
					Cj = tex2D(_MainTex, float2(i.uv.x,i.uv.y - (offset.y-(itr))/textureSize.y)).rgb;
					tempColor+=scanlines(offset.y-(itr), Cj, videoSize, outputSize);
				}
				#else
				for(float itr=-range;itr<range+2.0;itr++) {
					Cj = tex2D(_MainTex, float2(i.uv.x,i.uv.y - (offset.y-(itr))/textureSize.y)).rgb;
					tempColor+=Cj*STU(offset.y-(itr),(tvVerticalResolution/videoSize.y));
				}
				#endif

				tempColor -= float3(blackLevel, blackLevel,blackLevel);
				tempColor *= (contrast/float3(1.0-blackLevel, 1.0-blackLevel, 1.0-blackLevel));

				return fixed4(tempColor, 1.0);
			}

			ENDCG
		}
	}
}