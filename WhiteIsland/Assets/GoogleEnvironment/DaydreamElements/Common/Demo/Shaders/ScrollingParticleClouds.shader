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

Shader "DaydreamElements/Demo/Scrolling Particle Clouds" 
{
  Properties 
  {
    _Color ("Light Color", Color) = (0.9,0.9,0.8,1)
    _MainTex ("Main Texture (RGBA)", 2D) = "" {}

    _ScrollingAlpha1 ("Scrolling Alpha 1 (A)", 2D) = "" {}
    _ScrollingAlpha2 ("Scrolling Alpha 2 (A)", 2D) = "" {}

    _Texture1ScrollSpeed ("Tex 1 Scroll Speeds", Vector) = (0,0,0,0)
    _Texture2ScrollSpeed ("Tex 2 Scroll Speeds", Vector) = (0,0,0,0)
  }
  SubShader 
  {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    
    Pass {
      Blend One OneMinusSrcAlpha
      Cull Off 
      ZWrite Off
      ZTest LEqual 
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      #pragma multi_compile_particles

      struct appdata {
        float4 vertex : POSITION;
        float4 uv0 : TEXCOORD0;
        float2 uv1 : TEXCOORD1;
        half4 color : COLOR0;
      };

      struct v2f {
        float4 vertex : POSITION;
        half4 color : COLOR1;
        float4 uv0 : TEXCOORD0;
        float2 uv1 : TEXCOORD1;
      };

      half4 _Color;
      sampler2D _MainTex;
      float4 _MainTex_ST;

      sampler2D _ScrollingAlpha1;
      sampler2D _ScrollingAlpha2;
      float4 _ScrollingAlpha1_ST;
      float4 _ScrollingAlpha2_ST;

      float4 _Texture1ScrollSpeed;
      float4 _Texture2ScrollSpeed;

      v2f vert (appdata v) 
      {
        v2f o;

        float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.vertex = mul(UNITY_MATRIX_VP, worldPos);

        //Uses particle color to drive per-particle texture offset
        float4 _PPOffset = float4(v.color.x, v.color.x, v.color.y, v.color.y) * 10;

        float4 t1Offset = _Time.y * _Texture1ScrollSpeed;
        float4 t2Offset = _Time.y * _Texture2ScrollSpeed;

        o.uv0.xy = TRANSFORM_TEX(v.uv0.xy, _MainTex);
        o.uv0.zw = TRANSFORM_TEX(v.uv0.xy, _ScrollingAlpha1) + t1Offset.xy * _ScrollingAlpha1_ST.xy + _PPOffset.xy;
        o.uv1.xy = TRANSFORM_TEX(v.uv0.xy, _ScrollingAlpha2) + t2Offset.xy * _ScrollingAlpha2_ST.xy + _PPOffset.xy;

        o.color = v.color;

        return o;
      }

      half4 frag (v2f i) : SV_TARGET
      {   
        half4 texcol = tex2D(_MainTex, i.uv0.xy);
        half mask = texcol.a;        
        half scroll1 = tex2D(_ScrollingAlpha1, i.uv0.zw).a;
        half scroll2 = tex2D(_ScrollingAlpha2, i.uv1.xy).a;

        half alpha = min(0.25,mask * (scroll1 * scroll2)) * 4 * i.color.a;
        texcol = (texcol*(1-i.uv0.y) + _Color*i.uv0.y) * alpha;

        half4 col = half4(texcol.rgb, alpha);

        return col;
      }
      ENDCG
    }
  }
} 
