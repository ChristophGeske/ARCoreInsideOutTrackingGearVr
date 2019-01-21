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

Shader "Unlit/Vertex Color Simple Diffuse" {
  Properties {
    // Position of a virtual light
    _LightPos ("Virtual Light Position", Vector) = (0,15,0,0)
    // Main texture.
    _MainTex ("Texture", 2D) = "" {}
  }
  SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
        float3 normal : NORMAL;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        half4 color : COLOR;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;

      float3 _LightPos;

      float3 worldPos;

      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.color = v.color;

        // Get vertex world position.
        worldPos = mul(unity_ObjectToWorld,(v.vertex));

        // Simple nDotL lighting.
        float3 normal = UnityObjectToWorldNormal(v.normal);
        float3 lightDir = normalize(worldPos - _LightPos);
        float nDotL = dot(normal, -lightDir);

        // Apply simple lighting at different rates per channel.
        // Red is more diffuse, while green and blue are more emissive.
        o.color.r += 0.25 * nDotL;
        o.color.g += 0.25 * nDotL;
        o.color.b += 0.125 * nDotL;

        return o;
      }

      half4 frag (v2f i) : SV_Target {
        half3 col = tex2D(_MainTex, i.uv).rgb;

        // Darken texture by red channel (mutes red tones).
        col -= 0.25 * col * i.color.r;

        // Multiply texture and vertex color.
        col *= i.color.rgb;

        return half4(col,1);
      }
      ENDCG
    }
  }
}
