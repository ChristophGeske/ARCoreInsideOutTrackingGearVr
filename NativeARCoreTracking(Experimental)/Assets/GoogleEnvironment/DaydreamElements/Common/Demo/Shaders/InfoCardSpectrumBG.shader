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

Shader "DaydreamElements/Demo/Info Card Spectrum BG" {
  Properties {
    _Color ("Text Color", Color) = (1,1,1,1)
    _BGColor ("Background Color", Color) = (0,0,0,1)
    // Highlight color.
    _HighlightColor ("Highlight Color", Color) = (0,0.8,1,1)
    // Highlight Intensity.
    _Highlight ("Highlight Intensity", Range(0,1)) = 0
    _MainTex ("Texture", 2D) = "" {}
    _SpectrumOpacity ("Spectrum Opacity", Range(0,1)) = 0.6
  }
  SubShader {
    Tags { "RenderType"="Opaque"}
    LOD 100

    Cull Off

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      #define TWO_PI 6.28318530718

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
        float3 normal : NORMAL;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        half4 color : COLOR;
      };

      half3 _Color;
      half3 _BGColor;
      half3 _HighlightColor;
      half _Highlight;
      sampler2D _MainTex;
      float4 _MainTex_ST;
      float _SpectrumOpacity;

      float3 worldPos;

      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.color = v.color;

        // Get vertex world position.
        worldPos = mul(unity_ObjectToWorld,(v.vertex));

        float3 spectrum;
        float spectrumPos = length(worldPos.xyz) * 0.1;

        // Determine the color of the vertex.
        spectrum.r = 0.5 * cos(spectrumPos * TWO_PI) + 0.5;
        spectrum.g = 0.5 * cos((spectrumPos - 0.667) * TWO_PI) + 0.5;
        spectrum.b = 0.5 * cos((spectrumPos - 0.333) * TWO_PI) + 0.5;

        o.color.rgb = _SpectrumOpacity * spectrum + (1-_SpectrumOpacity) * _BGColor;

        // Add highlight.
        float highlightMask = _Highlight * 0.25;
        o.color.rgb = lerp(o.color.rgb, _HighlightColor.rgb, highlightMask);
        o.color.rgb += _HighlightColor.rgb * 0.5 * highlightMask;

        return o;
      }

      half4 frag (v2f i) : SV_Target {
        half4 col = tex2D(_MainTex, i.uv);
        // Mask vertex color by texture luminance.
        half alpha = Luminance(col.rgb);
        col.rgb = _Color * alpha + i.color * (1-alpha);
        return col;
      }
      ENDCG
    }
  }
}