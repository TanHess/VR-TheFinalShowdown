Shader "VR/VR_Water"
{
	Properties{
			_Color("Color", Color) = (1,1,1,1)
			//_MainTex("Albedo (RGB)", 2D) = "white" {}
			_BumpMap("Normal Map", 2D) = "bump" {}
			_Glossiness("Smoothness", Range(0,1)) = 0.5
			_Metallic("Metallic", Range(0,1)) = 0.0

			_RimColor("Rim Color", Color) = (0.0,0.0,0.0,0.0)
			_RimPower("Rim Edge Intensity", Range(0.1,10.0)) = 3.0

			_ScrollXSpeed("Normal X scroll speed", Range(-10, 10)) = 0.25
			_ScrollYSpeed("Normal Y scroll speed", Range(-10, 10)) = 0
	}
		SubShader{
			 Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 200
				CULL OFF
			CGPROGRAM
				// Physically based Standard lighting model, and enable shadows on all light types
				#pragma surface surf Standard fullforwardshadows alpha

				// Use shader model 3.0 target, to get nicer looking lighting
				#pragma target 3.0

				sampler2D _MainTex;
				sampler2D _BumpMap;

				struct Input {
					float2 uv_MainTex;
					float2 uv_BumpMap;
					float3 viewDir;
					float3 worldNormal;
					fixed2 normalOffsetUV;

				};

				half _Glossiness;
				half _Metallic;
				fixed4 _Color;
				float4 _RimColor;
				float _RimPower;

				float _ScrollXSpeed;
				float _ScrollYSpeed;

				// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
				// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
				// #pragma instancing_options assumeuniformscaling
				UNITY_INSTANCING_BUFFER_START(Props)
					// put more per-instance properties here
				UNITY_INSTANCING_BUFFER_END(Props)


				void surf(Input IN, inout SurfaceOutputStandard o) {
					// Albedo comes from a texture tinted by color
					//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
					o.Albedo = _Color;
					// Metallic and smoothness come from slider variables
					o.Metallic = _Metallic;
					o.Smoothness = _Glossiness;
					o.Alpha = _Color.a;


					fixed NormaloffsetX = _ScrollXSpeed * _Time;
					fixed NormaloffsetY = _ScrollYSpeed * _Time;

					IN.normalOffsetUV = IN.uv_BumpMap;
					IN.normalOffsetUV += fixed2(NormaloffsetX, NormaloffsetY);

					o.Normal = UnpackNormal(tex2D(_BumpMap, IN.normalOffsetUV));


					half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
					o.Emission = _RimColor.rgb * pow(rim, _RimPower);



				}
				ENDCG
			}
				FallBack "Diffuse"
}