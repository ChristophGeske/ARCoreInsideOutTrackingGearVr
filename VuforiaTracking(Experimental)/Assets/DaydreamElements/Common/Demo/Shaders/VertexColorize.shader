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

Shader "DaydreamElements/Demo/VertexColorize" {
  Properties {
    _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
  }
  SubShader {
    Tags { "RenderType" = "Opaque" }
    LOD 100

    Pass {
      Tags { "LightMode" = "ForwardBase" }

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float4 color : COLOR;
      };

      struct v2f {
        half4 color : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      half4 _Color;

      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color * ((1.0 - v.color.a) + _Color * v.color.a);
        return o;
      }

      fixed4 frag (v2f i) : SV_Target {
        return i.color;
      }
      ENDCG
    }
  }
}
