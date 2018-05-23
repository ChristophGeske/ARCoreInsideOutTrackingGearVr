// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "DaydreamElements/Demo/Glowing Orb"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Wattage ("Wattage", Float) = 1
		_Rim ("Rim", Float) = 1
		_Sign ("Sign", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"  "Queue"="Geometry"}
		LOD 100
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
      // Unity fog
      // #pragma multi_compile_fog
      // Custom fog
      #include "DaydreamElementsLighting.cginc"
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				half4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 fresnel : TEXCOORD0;
				half4 color : COLOR;
			};

			half3 _Color;
			half _Wattage;
			half _Rim;
			half _Sign;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				// Get vertex world position
        float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
        // Calculations for custom fog
        float3 direction = _WorldSpaceCameraPos - worldPos.xyz;
        float distance = length(direction);
        float3 viewDir = direction / distance;
        half4 fogVal = simplefog(worldPos, -viewDir, distance);

				half3 normal = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;

				float3 worldPosition = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1)).xyz;

				half3 viewDirection = normalize(worldPosition - _WorldSpaceCameraPos);

				half nDotV = abs(dot(normalize(viewDirection), normalize(normal)));
				half inv = 1 - nDotV;

				float t = _Time.y;
				_Wattage += (sin(t * 2.3)) * 0.5;

				o.fresnel.x = _Rim * inv * 0.75 + _Wattage * pow(nDotV, 7);
				o.fresnel.y = 0.5 + 0.5 * max(0,_Sign * normal.y);

				o.color.a = saturate(o.fresnel.x + o.fresnel.y) + 0.25 * o.fresnel.x;

        half3 col = _Color;
				// Apply custom fog
        col = fogVal.a * fogVal.rgb + (1-fogVal.a) * col;

        o.color.rgb = col;

				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 col = half4(i.color.a * i.color.rgb,1);
				return col;
			}
			ENDCG
		}
	}
}
