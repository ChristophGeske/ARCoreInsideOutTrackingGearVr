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

Shader "GoogleVR/Unlit/Flex Laser Reticle" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _ReticleScreenSpaceSize ("Reticle Screen Space Size", Float) = 0.01
  }

  SubShader {
    Tags {
      "Queue"="Transparent"
      "IgnoreProjector"="True"
      "RenderType"="TransparentCutout"
    }
    LOD 100

    Cull Off
    Lighting Off

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata_t {
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD0;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        half2 texcoord : TEXCOORD0;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float _ReticleScreenSpaceSize;

      v2f vert (appdata_t v) {
        v2f o;
        // Screen goes from -1 to 1, so we multiply by 2.
        float4 viewSpacePos = mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1));

        // Forward bias by 0.1 world space units.
        viewSpacePos.z += .05;

        o.vertex = mul(UNITY_MATRIX_P, viewSpacePos);
        o.vertex += float4(o.vertex.w * _ReticleScreenSpaceSize *
                           _ScreenParams.x * 2 * v.vertex.xy /
                           _ScreenParams.xy, 0, 0);

        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

        return o;
      }

      half4 frag (v2f i) : SV_Target {
        half4 col = tex2D(_MainTex, i.texcoord);
        clip(col.a - 0.5);
        return col;
      }
      ENDCG
    }
  }
}
