﻿// Copyright 2016 Google Inc. All rights reserved.
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

Shader "GoogleVR/Demos/Unlit/GVRDemo AmbientOcclusion"
{
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }
    LOD 100

    ZWrite On
    ZTest LEqual

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "../../../Distortion/GvrDistortion.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        //float4 worldPos : TEXCOORD1;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;

      v2f vert (appdata v) {
        v2f o;

        #if SHADER_API_MOBILE
        o.vertex = undistortVertex(v.vertex);
        #else
        o.vertex = UnityObjectToClipPos(v.vertex);
        #endif  // SHADER_API_MOBILE

        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
      }

      float4 frag (v2f i) : SV_Target {
        // Sample the texture.
        float col = tex2D(_MainTex, i.uv).a;
        return col;
      }
      ENDCG
    }
  }
}
