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

Shader "DaydreamElements/Demo/Dynamic Vertex Color Simple AO" {
  Properties 
  {
    _Color ("Grayscale Tint", Color) = (1,1,1,1)
    _MainTex ("Main Texture (A)", 2D) = "" {}
  }
  SubShader 
  {
    Tags { "Queue"="Geometry" "RenderType"="Geometry"}

    Pass {
      Cull Off
      ZWrite On
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      // Unity fog
      // #pragma multi_compile_fog
      // Custom fog
      #include "DaydreamElementsLighting.cginc"
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        // UNITY_FOG_COORDS(1)
        half4 color : COLOR;
      };

      // Properties set by GlobalDynamicWindColor
      half _GlobalEffectScale;
      half _GlobalColorEffectScale;
      
      half _WindSpeed;
      half _GustDensity = 0.5;
      half _GustColorScale = 0.05;
      
      half _SaturationMin;

      // Properties set by DynamicWindColorEffector
      half _PerObjectEffectScale;
      half _PerObjectRandom;

      half3 _Color;

      // Set by shader
      half saturation;

      sampler2D _MainTex;
      half4 _MainTex_ST;

      v2f vert(appdata v) {
        v2f o;

        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.vertex = UnityObjectToClipPos(v.vertex);

        // Object effect scale limited by global effect scale
        _PerObjectEffectScale *= _GlobalEffectScale;
        
        // Get vertex world position
        float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
        // Calculations for custom fog
        float3 direction = _WorldSpaceCameraPos - worldPos.xyz;
        float distance = length(direction);
        float3 viewDir = direction / distance;
        half4 fogVal = simplefog(worldPos, -viewDir, distance);

        /// Animate vertex color  

        // Unpack vertex color channels 
        half3 col = v.color.rgb;
        half grayscale = v.color.a;

        // Update saturation value
        saturation = _SaturationMin;
        saturation += _PerObjectEffectScale;
        saturation = clamp(saturation,0,0.25)*4;
        saturation *= _GlobalColorEffectScale;

        float t = _Time.y;

        //create world space gusts
        float gustMaskX = sin(t * 0.5 * _WindSpeed + worldPos.x * _GustDensity);
        float gustMaskZ = sin(t * 0.5 * _WindSpeed + worldPos.z * _GustDensity); 

        //Animate hue shift 
        col.r += gustMaskX * gustMaskZ * _GustColorScale;
        col.g += gustMaskX * gustMaskZ * _GustColorScale;

        // Tint grayscale
        half3 tint = _Color;

        // Get final albedo 
        col = col * saturation + (grayscale + tint) * (1-saturation);
        // Apply custom fog
        col = fogVal.a * fogVal.rgb + (1-fogVal.a) * col;

        o.color = half4(col,fogVal.a); 

        // UNITY_TRANSFER_FOG(o,o.vertex);

        return o;      
      }

      half4 frag(v2f i) : SV_TARGET { 
        half alpha = tex2D(_MainTex, i.uv).a;
        half4 col = half4(i.color.rgb * i.color.a + alpha * i.color.rgb * (1-i.color.a),1);
        // UNITY_APPLY_FOG(i.fogCoord, col);
        return col;      
      }      
      ENDCG    
    }
  }
}
