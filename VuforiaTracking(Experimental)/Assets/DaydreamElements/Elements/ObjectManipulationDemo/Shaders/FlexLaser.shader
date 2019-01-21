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

Shader "GoogleVR/Unlit/Flex Laser" {
  Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Alpha ("Alpha From Controller", Range(0,1)) = 1
    _MinLineThickness ("Min Line Thickness", Float) = 0.0001
    _MaxLineThickness ("Max Line Thickness", Float) = 1
    _MaxCameraDistance ("Max Camera Distance", Float) = 100
  }

  SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Overlay+100" }
    LOD 100

    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite Off

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        half4 color : TEXCOORD1;
      };

      half4 _Color;
      float _Alpha;
      float4 _LineJoint[101];
      float3 _LineNormalAxis;
      float _MinLineThickness;
      float _MaxCameraDistance;
      float _MaxLineThickness;

      v2f vert (appdata v) {
        v2f o;

        int idx = (int)(v.vertex.z * 100 + 0.5);
        int nextIdx = idx + 1;

        if (v.vertex.z < 0) {
          idx = 0;
          nextIdx = 1;
        }

        if (idx >= 99) {
          idx = 99;
          nextIdx = 100;
        }

        float4 jointPosition = _LineJoint[idx];
        float4 nextPosition = _LineJoint[nextIdx];

        float3 direction = nextPosition.xyz - jointPosition.xyz;
        float3 forwardDirection = normalize(direction);

        float3 localUp = normalize(cross(forwardDirection, _LineNormalAxis));
        float3 localRight = normalize(cross(localUp, forwardDirection));

        float3 vectorToCamera = jointPosition.xyz - _WorldSpaceCameraPos;
        float distanceToCamera = length(vectorToCamera);

        float weight = saturate( distanceToCamera / _MaxCameraDistance );
        float thickness = lerp(_MinLineThickness, _MaxLineThickness, weight);
        v.vertex.xyz = jointPosition.xyz +
                       thickness * v.vertex.z * forwardDirection +
                       thickness * v.vertex.x * localRight +
                       thickness * v.vertex.y * localUp;

        // Divide by the index to get the alpha value.
        o.color = half4(1,1,1,1 - smoothstep(0.1, .9, idx / 100.0 ) );

        // Apply user-defined color and alpha.
        o.color *= _Color;

        // Update alpha.
        o.color.a *= _Alpha;

        o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
        return o;
      }

      half4 frag (v2f i) : SV_Target {
        return i.color;
      }
      ENDCG
    }
  }
}