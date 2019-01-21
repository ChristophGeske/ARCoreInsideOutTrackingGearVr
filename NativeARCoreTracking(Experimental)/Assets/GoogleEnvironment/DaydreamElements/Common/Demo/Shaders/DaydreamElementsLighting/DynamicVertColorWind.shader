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

Shader "DaydreamElements/Demo/Dynamic Vertex Color and Wind" {
  Properties 
  {
     _Color ("Grayscale Tint", Color) = (1,1,1,1)
  }
  SubShader {
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
        float4 vertex : SV_POSITION;
        //UNITY_FOG_COORDS(1)
        float2 uv : TEXCOORD0;
        half4 color : COLOR;
      };

      //Properties set by GlobalDynamicWindColor
      half _GlobalEffectScale = 1;

      half _GlobalColorEffectScale = 1;
      half _GlobalWindEffectScale = 1;

      half _RadialEffectorInfluence;
      
      //half _WorldYWindFalloff;
      //half _WorldGroundYPos;
      
      half _SaturationMin;

      half _WindSpeed;
      half _WindMagnitude;
      half _WindTurbulence;

      half _WindDirectionX;
      half _WindDirectionZ;
      half _GustDensity = 0.5;
      half _GustColorScale = 0.05;

      half4 _EffectorA;
      half4 _EffectorB;
      half4 _EffectorC;

      //Properties set by DynamicWindColorEffector
      half _PerObjectEffectScale = 1;
      half _PerObjectRandom;

      half3 _Color;

      //Set by shader
      half saturation;

      half effectorAMask = 1;
      half effectorBMask = 1;
      half effectorCMask = 1;

      float3 windOffset;
      //float3 windOffsetWorld;

      v2f vert(appdata v) {
        v2f o;

        o.uv = v.uv;

        //Object effect scale limited by global effect scale
        _PerObjectEffectScale *= _GlobalEffectScale;

        // Get vertex world position
        float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
        // Calculations for custom fog
        float3 direction = _WorldSpaceCameraPos - worldPos.xyz;
        float distance = length(direction);
        float3 viewDir = direction / distance;
        half4 fogVal = simplefog(worldPos, -viewDir, distance);

        ////Animate vertex color  

        //Unpack vertex color channels 
        half3 col = v.color.rgb;
        half grayscale = v.color.a;

        //Calculate influence from radial effectors
        half dstToEffectorA = length(_EffectorA.xyz - worldPos.xyz);
        half effectorAScale = _EffectorA.w;
        effectorAMask = 1 - (dstToEffectorA / _EffectorA.w);
        effectorAMask *= effectorAScale;
        effectorAMask = saturate(effectorAMask);

        half dstToEffectorB = length(_EffectorB.xyz - worldPos.xyz);
        half effectorBScale = _EffectorB.w;
        effectorBMask = 1 - (dstToEffectorB / _EffectorB.w);
        effectorBMask *= effectorBScale;
        effectorBMask = saturate(effectorBMask);

        half dstToEffectorC = length(_EffectorC.xyz - worldPos.xyz);
        half effectorCScale = _EffectorC.w;
        effectorCMask = 1 - (dstToEffectorC / _EffectorC.w);
        effectorCMask *= effectorCScale;
        effectorCMask = saturate(effectorCMask);

        //Update saturation value
        saturation = _SaturationMin;
        saturation += _PerObjectEffectScale;
        saturation += (effectorAMask + effectorBMask + effectorCMask) * _RadialEffectorInfluence;
        saturation = clamp(saturation,0,0.25)*4;
        saturation *= _GlobalColorEffectScale;

        ////Animate vertex position
        float t = _Time.y;

        //Unpack UVs
        float windOffsetMask = v.uv.x;
        float windMask = v.uv.y;

        //Add random seed and turbulence to wind effect 
        half randomMask = windOffsetMask * _WindTurbulence;

        //create world space gusts
        float gustMaskX = sin(t * 0.5 * _WindSpeed + worldPos.x * _GustDensity);
        float gustMaskZ = sin(t * 0.5 * _WindSpeed  + worldPos.z * _GustDensity); 

        _WindDirectionX += gustMaskX;
        _WindDirectionX *= windMask;
        _WindDirectionZ += gustMaskZ;
        _WindDirectionZ *= windMask;

        //Animate hue shift 
        col.r += gustMaskX * gustMaskZ * _GustColorScale;
        col.g += gustMaskX * gustMaskZ * _GustColorScale;

        //Tint grayscale
        half3 tint = _Color;

        // Get final albedo 
        col = col * saturation + (grayscale + tint) * (1-saturation);
        // Apply custom fog
        col = fogVal.a * fogVal.rgb + (1-fogVal.a) * col;

        //Final color
        o.color = half4(col,1); 

        //Local wind offset
        t += _PerObjectRandom;
        float effectX = sin(t * _WindSpeed + randomMask) + _WindDirectionX; 
        float effectZ = cos(t * _WindSpeed + randomMask) + _WindDirectionZ;
        windOffset = float3(effectX, 0, effectZ);
        windOffset *= windMask;

        //World space wind effect 
        //float worldYMask = max(0,(worldPos.y - _WorldGroundYPos)*_WorldYWindFalloff);
        //effectX = (sin(t * _WindSpeed + _WindTurbulence) + _WindDirectionX); 
        //effectZ = (cos(t * _WindSpeed + _WindTurbulence) + _WindDirectionZ);
        //windOffsetWorld = float3(effectX, 0, effectZ);
        //windOffsetWorld *= worldYMask; 
        //windOffset += windOffsetWorld;
        
        //Scale wind magnitude
        _WindMagnitude *= 0.01;
        _WindMagnitude *= _PerObjectEffectScale;
        _WindMagnitude *= _GlobalWindEffectScale;

        //Update vertex position
        float4 windOffsetTransform = float4(v.vertex.xyz + _WindMagnitude * windOffset, 1);
        o.vertex = UnityObjectToClipPos(windOffsetTransform);

        //UNITY_TRANSFER_FOG(o,o.vertex);

        return o;      
      }

      half4 frag(v2f i) : SV_TARGET { 
        half4 col = i.color;
        //UNITY_APPLY_FOG(i.fogCoord, col);
        return col;      
      }      
      ENDCG    
    }
  }
}
