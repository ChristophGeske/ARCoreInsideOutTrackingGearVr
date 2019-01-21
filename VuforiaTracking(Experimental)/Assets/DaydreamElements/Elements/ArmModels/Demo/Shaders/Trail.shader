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

Shader "DaydreamElements/ArmModels/Trail" {
  Properties {
      [PerRendererData]_Alpha ("Alpha", Range(0, 1)) = 1
  }

  SubShader {
    Tags { 
      "Queue"="Transparent"
      "IgnoreProjector" = "True"
      "RenderType"="Transparent"
    }

    LOD 100

    Cull off
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        half4 color : COLOR;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        half4 color : COLOR;
      };

      float _Alpha;
      
      v2f vert (appdata v) {
        v2f o;

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color;
        o.color.a *= _Alpha;

        return o;
      }
      
      half4 frag (v2f i) : SV_Target {
        half4 col = i.color;
        return col;
      }
      ENDCG
    }
  }
}
