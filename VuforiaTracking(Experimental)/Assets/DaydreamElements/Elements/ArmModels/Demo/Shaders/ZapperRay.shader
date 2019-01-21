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

Shader "DaydreamElements/ArmModels/ZapperRay" {
  Properties {
    _PulseAmount ("Pulse Amount", float) = 1.0
    _PhaseMultiplier ("Phase Multiplier", float) = 2.0
    _OffsetMultiplier ("Offset Multiplier", float) = 10.0
  }

  SubShader {
    Tags {
      "Queue"="Transparent"
      "IgnoreProjector"="True"
      "RenderType"="Transparent"
      "PreviewType"="Plane"
    }

    Blend SrcAlpha OneMinusSrcAlpha
    Cull Back
    Lighting Off
    ZWrite Off

    LOD 100

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        half3 normal : NORMAL;
        half4 color : COLOR;
      };

      struct v2f {
        half4 color : COLOR;
        float4 vertex : SV_POSITION;
      };

      float _PulseAmount;
      float _PhaseMultiplier;
      float _OffsetMultiplier;

      v2f vert (appdata v) {
        v2f o;

        float phase = _Time * _PhaseMultiplier;
        float offset = v.vertex.z * _OffsetMultiplier;
        float pulseWave = sin(phase + offset);// * 0.5 + 0.5;
        float pulse = pulseWave * (1.0 - 0.5) + 0.5;
        pulse = pulse * _PulseAmount;
        v.vertex.xyz = v.vertex.xyz + (v.normal * pulse);

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color;
        return o;
      }

      fixed4 frag (v2f i) : SV_Target {
        fixed4 col = i.color;
        return col;
      }
      ENDCG
    }
  }
}