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

// Shader for a sphere that can be used as the visual origin of the
// GVR laser line.
Shader "DaydreamElements/ChaseCam/Laser Spark" {
  Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Alpha ("Alpha", Range(0,1)) = 1
  }

  SubShader {
    Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Overlay+101" }
    LOD 100
    Blend SrcAlpha One

    ZTest LEqual
    ZWrite Off
    Cull Back

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
        float3 normal : NORMAL;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
      };

      half4 _Color;
      float _Alpha;

      // Move the verticies to make a smooth bezier curve.
      v2f vert (appdata v) {
        v2f o;
        UNITY_INITIALIZE_OUTPUT(v2f, o); // d3d11 requires initialization

        half3 normal = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
        float3 worldPosition = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1)).xyz;
        half3 viewDirection = normalize(worldPosition - _WorldSpaceCameraPos);
        half nDotV = abs(dot(normalize(viewDirection), normalize(normal)));

        o.vertex = UnityObjectToClipPos(v.vertex);

        // Store the dot product in uv coords.
        o.uv = float2(nDotV, v.uv.y);

        o.color = _Color;

        o.color = nDotV;

        // Apply the alpha from the controller visual.
        o.color.a *= _Alpha;

        return o;
      }

      half4 frag (v2f i) : SV_Target {
        half4 col = i.color;

        // Create the laser texture.
        col *= smoothstep(0.8, 1, i.uv.x) + 0.3 * i.uv.x;

        return col;
      }

      ENDCG
    }
  }
}