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

Shader "DaydreamElements/Demo/Particle Solid Color Additive" 
{
  Properties 
  {
    _MainTex ("Main Texture (A)", 2D) = "" {}
  }
  SubShader 
  {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    
    Pass 
    {
      Blend One OneMinusSrcAlpha
      Cull Back
      ZWrite Off
      ZTest LEqual 
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata 
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
      };

      struct v2f 
      {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
      };
      
      sampler2D _MainTex;
      float4 _MainTex_ST;

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.color = v.color;
        return o;
      }

      half4 frag (v2f i) : SV_TARGET
      {   
        half alpha = tex2D(_MainTex, i.uv).a;
        alpha *= i.color.a;
        half4 col = half4(i.color.rgb*alpha, alpha);
        return col;   
      }
      ENDCG
    }
  }
} 
