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

Shader "DaydreamElements/Demo/ESM/Depth 32" {
  Properties {
  }
  Category {
    SubShader {
      Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }

      Blend Off
      AlphaTest Off
      Cull Off
      Lighting Off
      ZWrite On
      ZTest LEqual
      Fog { Mode Off }

      Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        struct appdata {
            float4 position : POSITION;
        };

        struct v2f {
           float4 position : SV_POSITION;
           float cameraPosition : TEXCOORD0;
        };

        float4x4 _ShadowCameraMatrix;
        float4 _ShadowData;
        float _ShadowBias;

        v2f vert (appdata vertex) {
            v2f output;
            float4 worldPosition = mul (unity_ObjectToWorld, vertex.position);
            output.position = UnityObjectToClipPos (vertex.position);
            float4 shadowPosition = mul (_ShadowCameraMatrix, worldPosition);
            // Shadow bias can be added here.
            output.cameraPosition = _ShadowData.y*abs(( shadowPosition.z+_ShadowBias)/_ShadowData.x );
            return output;
        };

        // Encoding/decoding [0..1) floats into 8 bit/channel RGBA. Note that 1.0 will not be encoded properly.
        inline float4 EncodeFloatRGBA( float v ) {
          float4 kEncodeMul = float4(1.0, 255.0, 65025.0, 16581375.0);
          float kEncodeBit = 1.0/255.0;
          float4 enc = kEncodeMul * v;
          enc = frac (enc);
          enc -= enc.yzww * kEncodeBit;
          return enc;
        }

        half4 frag (v2f fragment) : SV_TARGET {
          return EncodeFloatRGBA(_ShadowData.w*exp(fragment.cameraPosition) );
        }
        ENDCG
      }
    }
  }
}