///////////////////////////////////////////////////////////////////////////////
//Copyright 2017 Google Inc.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
///////////////////////////////////////////////////////////////////////////////

Shader "Unlit/shadowFilter"
{
	Properties
	{
		_MainTex("Shadowmap", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		ZTest Always
		Cull Off
		ZWrite Off
		Fog{ Mode off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4  _uvTransform;
			sampler _MainTex;
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
			};

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				half2 uv0 = v.texcoord.xy - _uvTransform.xy;

				o.uv0 = uv0;
				o.uv1 = uv0 + _uvTransform.xy;
				o.uv2 = uv0 + _uvTransform.xy * 2.0;
				o.uv3 = uv0 + _uvTransform.xy * 3.0;
				
				return o;
			}

			half min3(half a, half b, half c)
			{
				return min(a, min(b, c));
			}

			half4 frag (v2f i) : SV_Target
			{
				//4 texture samples
				half2 s0 = tex2D(_MainTex, i.uv0).rg;
				half2 s1 = tex2D(_MainTex, i.uv1).rg;
				half2 s2 = tex2D(_MainTex, i.uv2).rg;
				half  s3 = tex2D(_MainTex, i.uv3).r;

				half2 result = s1;
				//dilation - 4 samples used for dilation (includes current texel)
				if (result.x > 0.998)
				{
					result.x = min3(s0.x, s2.x, s3);
				}
				//blur - 3 samples used for blur, centered
				//the centerWeight must be at least 1.0, which is why the blurFactor must be at least 1/16 (0.0625)
				half blurFactor = max( _uvTransform.w*_uvTransform.w, 0.0625 );
				half centerWeight = lerp(0.0f, 16.0f, blurFactor);

				result.y = _uvTransform.z * (s0.y + s1.y*centerWeight + s2.y) / (centerWeight + 2.0);
				return half4(result, 0, 1);
			}
			ENDCG
		}
	}
}
