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

Shader "DaydreamElements/Demo/Unlit Vertex Color ESM Receiver" {
  Properties {
  }
  SubShader {
    Tags{ "RenderType" = "Opaque" }
    LOD 100
    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        half4 color : COLOR;
        float3 normal : NORMAL;
        float4 uv : TEXCOORD0;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        float4 uv : TEXCOORD0;
        float3 shadowPosition : TEXCOORD3;
        half4 color : COLOR;
      };

      float4x4 _ShadowMatrix;
      float4x4 _ShadowCameraMatrix;
      float4 _ShadowData;
      float _ShadowBias;

      v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color;

        // Shadows
        float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);
        float4 shadowPosition = mul(_ShadowMatrix, worldPosition);

        float2 shadowTexturePosition = .5 + .5*shadowPosition.xy/abs(shadowPosition.w);
        float shadowDepth = -_ShadowData.y;
        shadowDepth *= abs(_ShadowBias + mul(_ShadowCameraMatrix, worldPosition).z);
        shadowDepth = shadowDepth / _ShadowData.x;
        o.shadowPosition = float3(shadowTexturePosition.x, shadowTexturePosition.y, shadowDepth);

        return o;
      }

      sampler2D _ShadowTexture;

      half4 frag(v2f i) : SV_Target {
        // Shadows
        half4 compressedShadow = tex2D(_ShadowTexture, i.shadowPosition.xy);
        float shadowDepth = _ShadowData.z*DecodeFloatRGBA(compressedShadow);
        float depth = exp(i.shadowPosition.z);
        half2 shadowClipping = max(0,10*(abs(2 * i.shadowPosition.xy - 1) -0.9));
        half shadowExtent = max(shadowClipping.x,shadowClipping.y);
        half shadow = min(1,shadowExtent + saturate(smoothstep(.8,1,depth*shadowDepth)));

        // Color
        half4 col = i.color;
        col = col * (shadow + (1-shadow) * col * col);

        return col;
      }
    ENDCG
    }
  }
}