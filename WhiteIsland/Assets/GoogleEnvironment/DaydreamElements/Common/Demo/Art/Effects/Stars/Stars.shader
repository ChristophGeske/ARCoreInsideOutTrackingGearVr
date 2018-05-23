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

Shader "DaydreamElements/Demo/Stars"
{
  Properties
  {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Main Texture", 2D) = "" {}
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

    Blend One One
    AlphaTest Off
    Cull Off
    Lighting Off
    ZWrite Off
    ZTest LEqual
    Fog { Mode Off }

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata
      {
        half4 vertex : POSITION;
        half2 uv : TEXCOORD0;
      };

      struct v2f
      {
        half4 vertex : SV_POSITION;
        half2 uv : TEXCOORD0;
        half4 color : TEXCOORD1;
      };

      v2f vert (appdata v) {
        v2f o;

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;

        half3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
        half3 worldVector = normalize(worldPosition - _WorldSpaceCameraPos.xyz);

        o.color = half4(1,1,1, saturate( 2 * asin(worldVector.y)/(0.5*3.1415926) ));

        return o;
      };

      half4 _Color;
      sampler2D _MainTex;

      half4 frag (v2f i) : SV_TARGET
      {
        return i.color.a * tex2D(_MainTex, i.uv).a * _Color;
      }

      ENDCG
    }
  }
}
