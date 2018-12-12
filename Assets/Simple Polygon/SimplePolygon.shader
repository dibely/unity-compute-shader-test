Shader "Unlit/SimplePolygon" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			ZTest Always
			Cull Off
			ZWrite Off
			Fog{ Mode Off }

			CGPROGRAM

			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct VertexData {
				float3 position;
				float3 normal;
				float2 uv;
			};

			uniform StructuredBuffer<VertexData> vertexBuffer;

			struct v2f {
				float4 vertex : SV_POSITION;
				//float4 normal : SV_NORMAL;
				//float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (uint id: SV_VertexID) {
				v2f o;
				//float3 position = vertexBuffer[id].position;
				float4 position = float4(vertexBuffer[id].position, 1.0f);
				o.vertex = UnityObjectToClipPos(position);
				//o.normal = float4(normalize(vertexBuffer[id].normal), 1.0f);
				//o.uv = TRANSFORM_TEX(vertexBuffer[id].uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				// sample the texture
				//return tex2D(_MainTex, i.uv);
				return float4(0.0f, 1.0f, 0.0f, 1.0f);
			}
			ENDCG
		}
	}
}
