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

Shader "DaydreamElements/Demo/Info Card Text" {
  Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Main Texture (A)", 2D) = "" {}
  }
  SubShader {
    Tags { "RenderType"="Transparent" "Queue"="Transparent" }
    LOD 100
 
    Lighting Off 
    ZTest LEqual
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        half4 color : COLOR;
      };

      half4 _Color;
      sampler2D _MainTex;
      float4 _MainTex_ST;

      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.color = _Color;
        return o;
      }

      half4 frag (v2f i) : SV_Target {
        half4 col = i.color;
        col.a *= tex2D(_MainTex, i.uv).a;
        return col;
      }
      ENDCG
    }
  }
}