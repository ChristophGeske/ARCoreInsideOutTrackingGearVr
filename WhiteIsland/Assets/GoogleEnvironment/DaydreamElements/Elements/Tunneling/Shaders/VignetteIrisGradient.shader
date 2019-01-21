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

Shader "DaydreamElements/Tunneling/VignetteIrisGradient" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
  }
  SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Overlay" }
    LOD 100

    Pass {
      Blend OneMinusSrcAlpha SrcAlpha
      ZTest Always
      ZWrite Off
      Cull Front
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
        float2 uv : TEXCOORD0;
        float alpha : TEXCOORD1;
      };

      // The forward vector of the 'head' camera.
      // We aren't using camera position by design.
      float4 _MainCameraForward;

      float4 _VignetteMinMax;
      float _VignetteAlpha;

      v2f vert (appdata v) {
        v2f o;

        float scaledLocation = 1.57079632679 * ((v.vertex.y * (1 - _VignetteMinMax.x) + _VignetteMinMax.x));
        float xzDist = _VignetteMinMax.w * sin(scaledLocation);
        float yDist = _VignetteMinMax.w * -cos(scaledLocation) + 1;
        float4 vertexPosition = float4(xzDist * v.vertex.x, yDist, xzDist * v.vertex.z, 1);

        o.vertex = UnityObjectToClipPos (vertexPosition);

        // The blending size in radians, scaled by the size of the blend region.
        float val = saturate(_VignetteMinMax.y * v.vertex.y * (1 - _VignetteMinMax.x));
        val = val * val * (3.0 - (2.0 * val));  ///cubic (smoothstep)
        val = 1.0 - (val * _VignetteAlpha);

        // Calculate view direction.
        float3 worldPosition =  mul(unity_ObjectToWorld, vertexPosition).xyz;
        float3 viewDirection = normalize(worldPosition - _WorldSpaceCameraPos.xyz);

        o.uv.x = 0.5;
        o.uv.y = 0.5*(viewDirection.y +1);

        o.alpha = val;

        return o;
      }

      // Gradient texture.
      sampler2D _MainTex;

      fixed4 frag (v2f i) : SV_Target {
        half3 col = tex2D(_MainTex, i.uv);
        return float4(col, i.alpha);
      }
      ENDCG
    }
  }
}
