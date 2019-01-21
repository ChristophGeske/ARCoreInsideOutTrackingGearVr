// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Copyright 2016 Google Inc. All rights reserved.
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

Shader "DaydreamElements/Tunneling/VignetteCage" {
  Properties {
  }
  SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Overlay+10" }
    LOD 100

    Pass {
      ZTest Always
      ZWrite Off
      Cull Back

      Blend SrcAlpha OneMinusSrcAlpha
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      struct appdata {
        float4 vertex : POSITION;
        half4 color: COLOR0;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        half alpha : COLOR0;
        float viewDotCamera : TEXCOORD0;
      };

      float3 _MainCameraForward;

      v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos (v.vertex);
        float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
        float3 viewDireciton = normalize(worldPosition - _WorldSpaceCameraPos);
        o.viewDotCamera = dot(viewDireciton, _MainCameraForward);
        o.alpha = v.color.r;
        return o;
      }

      /// The cage color
      float4 _VignetteCageColor;

      /// The vignette min and max values to control the blending
      float4 _VignettePixel;

      /// Alpha
      float _VignetteAlpha;

      fixed4 frag(v2f i) : SV_Target {
        float alpha = 1 -
        smoothstep(_VignettePixel.x, _VignettePixel.y, i.viewDotCamera);
        return float4(_VignetteCageColor.rgb,
        _VignetteCageColor.a * i.alpha * alpha * alpha * _VignetteAlpha);
      }
      ENDCG
    }
  }
}