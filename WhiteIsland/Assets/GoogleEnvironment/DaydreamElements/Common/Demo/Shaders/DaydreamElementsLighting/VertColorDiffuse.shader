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

Shader "DaydreamElements/Demo/Vertex Color Diffuse"
{ 
  Properties {
  // Shadow bias property for ESM shadowing
    _ShadowBias ("Shadow Bias", Float) = 0
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
      // Custom fog
      #include "DaydreamElementsLighting.cginc"
      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        half4 color : COLOR;
      };
      
      v2f vert (appdata v)
      {
        v2f o;

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        o.color = v.color;
       
        // Calculations for custom fog
        half4 worldPos = (mul(unity_ObjectToWorld, v.vertex));
        float3 direction = _WorldSpaceCameraPos - worldPos.xyz;
        float distance = length(direction);
        float3 viewDir = direction / distance;
        half4 fogVal = simplefog(worldPos, -viewDir, distance);
        
        half3 col = o.color.rgb;

        // Apply custom fog
        col = fogVal.a * fogVal.rgb + (1-fogVal.a) * col;

        o.color = half4(col,1);

        return o;
      }
      
      half4 frag (v2f i) : SV_Target
      {
        half4 col = i.color;
        return col;
      }   
      ENDCG
    }
  }
}
