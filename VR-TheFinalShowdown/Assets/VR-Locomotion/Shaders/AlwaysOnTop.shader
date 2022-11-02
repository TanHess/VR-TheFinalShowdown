
Shader "VRBooster/AlwaysOnTop" {
	Properties{
		  _MainTex("Base", 2D) = "white" {}
		  _MainColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}

		CGINCLUDE

#include "UnityCG.cginc"

		  sampler2D _MainTex;
		  fixed4 _MainColor;

		  half4 _MainTex_ST;

		  struct appdata
		  {
			  float4 vertex : POSITION;
			  float2 uv : TEXCOORD0;

			  UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
		  };

		  struct v2f {
			  half4 pos : SV_POSITION;
			  half2 uv : TEXCOORD0;
			  fixed4 vertexColor : COLOR;

			  UNITY_VERTEX_OUTPUT_STEREO //Insert
		  };

		  v2f vert(appdata_full v) {
			  v2f o;

			  UNITY_SETUP_INSTANCE_ID(v); //Insert
			  UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
			  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

			  o.pos = UnityObjectToClipPos(v.vertex);
			  o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
			  o.vertexColor = v.color * _MainColor;

			  return o;
		  }

		  fixed4 frag(v2f i) : COLOR{
			  return tex2D(_MainTex, i.uv.xy) * i.vertexColor;
		  }

			  ENDCG

			  SubShader {
			  Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+100" }
				  Cull Off
				  Lighting Off
				  ZWrite Off
				  ZTest Always
				  Fog{ Mode Off }
				  Blend SrcAlpha OneMinusSrcAlpha

				  Pass{

					  CGPROGRAM

					  #pragma vertex vert
					  #pragma fragment frag
					  #pragma fragmentoption ARB_precision_hint_fastest

					  ENDCG

			  }

		  }
		  FallBack Off
}