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

Shader "DaydreamElements/Demo/ESM/ScreenSpace"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        half2 uv20 : TEXCOORD0;
        half2 uv21 : TEXCOORD1;
        half2 uv22 : TEXCOORD2;
        half2 uv23 : TEXCOORD3;
      };

      sampler2D _MainTex;
      uniform half4 _MainTex_TexelSize;

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = v.vertex;
        o.vertex.xy*=2;
        o.uv20 = v.uv + _MainTex_TexelSize.xy;
        o.uv21 = v.uv + _MainTex_TexelSize.xy * half2(-0.5h,-0.5h);
        o.uv22 = v.uv + _MainTex_TexelSize.xy * half2(0.5h,-0.5h);
        o.uv23 = v.uv + _MainTex_TexelSize.xy * half2(-0.5h,0.5h);
        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        float color = DecodeFloatRGBA(tex2D (_MainTex, i.uv20));
        color += DecodeFloatRGBA(tex2D (_MainTex, i.uv21));
        color += DecodeFloatRGBA(tex2D (_MainTex, i.uv22));
        color += DecodeFloatRGBA(tex2D (_MainTex, i.uv23));
        return EncodeFloatRGBA(color *.25);
      }
      ENDCG
    }
  }
}
