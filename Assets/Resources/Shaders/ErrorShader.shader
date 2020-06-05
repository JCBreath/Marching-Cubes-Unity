// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ErrorShader" {
	Properties{
		_Volume("Volume", 3D) = "white" {}
		_Threshold("Threshold", Float) = .0
		_Position("Position", Vector) = (.0, .0, .0)
	}

		SubShader{
			Tags { "Queue" = "Transparent" "Render" = "Transparent" "IgnoreProjector" = "True"}
			LOD 200

			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			Pass{
				CGPROGRAM

				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata {
					float4 vertex : POSITION;

				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float3 worldPos : TEXCOORD0;
					float3 objectPos : TEXCOORD1;
				};

				uniform float3 _Position;
				uniform sampler3D _Volume;
				uniform float _Threshold;

				v2f vert(appdata v) {
					v2f o;
					
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					o.vertex = UnityObjectToClipPos(v.vertex);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target{
					fixed4 tex = tex3D(_Volume, i.worldPos - _Position);
					fixed4 col = float4(1., 1., 1., 1.);
					float diff = abs(tex.r - _Threshold);
					col.g -= diff;
					col.b -= diff;
					return col;
				}

				ENDCG
			}


	}
		FallBack "Diffuse"
}