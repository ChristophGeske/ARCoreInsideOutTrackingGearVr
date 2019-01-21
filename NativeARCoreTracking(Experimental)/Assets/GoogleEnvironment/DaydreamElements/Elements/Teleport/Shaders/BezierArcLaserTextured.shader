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

// Shader draws a bezier line that bends smoothly with control point.
Shader "DaydreamElements/Teleport/Bezier Arc Laser Textured" {
  Properties {
    _Color ("Color", Color) = (1, 1, 1, 1)
    _MainTex ("Main Texture", 2D) = "white" {}
    _StartPosition ("Start", Vector) = (0, 0, 0, 0)
    _EndPosition ("End", Vector) = (0, 0, 0, 0)
    _ControlPosition ("Control", Vector) = (0, 0, 0, 0)
    _LineWidth ("Line Width", float) = 0.0075
    _EndLineWidth ("End Line Width", float) = 0.25
    _MaxDistance ("Maximum Distance", float) = 15
    _Completion ("Completion", Range(0,1)) = 1
    _Alpha ("Controller Alpha", Range(0,1)) = 1
  }

  SubShader {
    Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Overlay+101" }
    LOD 100
    Blend SrcAlpha One

    ZTest LEqual
    ZWrite Off
    Cull Off

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      half4 _Color;
      sampler2D _MainTex;
      float4 _MainTex_ST;
      float4 _StartPosition;
      float4 _EndPosition;
      float4 _ControlPosition;
      float _LineWidth;
      float _EndLineWidth;
      float _MaxDistance;
      float _Completion;
      float _Alpha;

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

      // Calculate a quadratic bezier curve point for a given time (0-1).
      float BezierValueForTime(
        float start,
        float end,
        float control,
        float t) {
      return (pow(1 - t, 2) * start)
        + (2 * (1 - t) * t * control)
        + (pow(t, 2) * end);
      }

      // Move the verticies to make a smooth bezier curve.
      v2f vert (appdata v) {
        v2f o;
        UNITY_INITIALIZE_OUTPUT(v2f, o); // d3d11 requires initialization

        // The input mesh uses z values 0-1 to indicate its position along the curve.
        float percent = v.vertex.z;

        float4 segmentCenter = v.vertex;

        // Calcuate new x, y, z values for the center of each segment,
        // given the start/end/control positions and line width.
        segmentCenter.x = BezierValueForTime(_StartPosition.x, _EndPosition.x, _ControlPosition.x, percent);
        segmentCenter.y = BezierValueForTime(_StartPosition.y, _EndPosition.y, _ControlPosition.y, percent);
        segmentCenter.z = BezierValueForTime(_StartPosition.z, _EndPosition.z, _ControlPosition.z, percent);

        // Width is scaled the farther you are from the start position.
        float segmentToStart = distance(segmentCenter, _StartPosition);
        float distanceFade = min(1, segmentToStart / _MaxDistance) * percent;
        float width = lerp(_LineWidth, _EndLineWidth, distanceFade);

        float4 vertexViewPos = mul(UNITY_MATRIX_MV, segmentCenter);

        // The mesh has edge vertices at world -1 to 1, so we can identify
        // left and right hand vertices by the sign of v.vertex.x, and displace them in
        // screen space accordingly.
        vertexViewPos.x += v.vertex.x * width;

        o.vertex = mul(UNITY_MATRIX_P, vertexViewPos);

        o.uv = TRANSFORM_TEX(v.uv, _MainTex);

        o.color = _Color;

        // Fade by completion and position on the arc.
        o.color.a = saturate(1 - (1 / _Completion) * percent);
        // Take 25% of alpha to match the opacity of the GVR SDK laser.
        o.color.a *= 0.25;
        // Apply the alpha from the controller visual.
        o.color.a *= _Alpha;

        return o;
      }

      half4 frag (v2f i) : SV_Target {
        // UV flip for GVR laser texture.
        float2 flipUV = float2(i.uv.y, i.uv.x);
        half4 col = tex2D(_MainTex, i.uv);

        col *= i.color;

        return col;
      }

      ENDCG
    }
  }
}