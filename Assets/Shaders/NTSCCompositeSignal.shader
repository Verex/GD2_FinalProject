Shader "FX/NTSCCompositeSignal"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[Toggle(USE_COMPOSITE)] _isCompositeEnabled("Use Composite Signal", Float) = 0
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

			#pragma shader_feature USE_COMPOSITE

			#define RGB_to_YIQ transpose(float3x3( 0.299 , 0.595716 , 0.211456 , 0.587 , -0.274453 , -0.522591 , 0.114 , -0.321263 , 0.311135 ))
			
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

			fixed4 frag (v2f i) : SV_Target
			{
				signalResolution=256.0;
				signalResolutionI=83.0;
				signalResolutionQ=25.0;

				videoSize = float2(256.0, 240.0);
				textureSize = float2(256.0, 240.0);
				outputSize = float2(256.0, 240.0);

				blackLevel = 0.0875;
				contrast=1.0;
				tvVerticalResolution=240.0;

				fixed4 col = tex2D(_MainTex, i.uv);
				// just invert the colors
				#ifdef USE_COMPOSITE
				col.rgb=mul(RGB_to_YIQ, col.rgb);
				#endif
				return col;
			}
			ENDCG
		}
	}
}
