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

Shader "Unlit/Poly Object Library"
{
  Properties
  {
    // Tint color.
    _Color ("Ambient Tint", Color) = (1,1,1,1)
    // Highlight color.
    _HighlightColor ("Highlight Color", Color) = (0,0.8,1,1)
    // Highlight Intensity.
    _Highlight ("Highlight Intensity", Range(0,1)) = 0
    // Intensity of a virtual light.
    _LightIntensity ("Virtual Light Intensity", Range(0,2)) = 0.5
    // World space position of a virtual light.
    _LightPos ("Virtual Light Position", Vector) = (0,15,0,0)
    // World space position of a virtual ground plane.
    _GroundPos ("Ground Plane Position (Y)", Float) = 0
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
        half4 color : COLOR;
        float3 normal : NORMAL;
      };

      struct v2f
      {
        half4 color : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      half3 _Color;
      half3 _HighlightColor;
      half _Highlight;
      float _LightIntensity;
      float3 _LightPos;
      float _GroundPos;

      float3 worldPos;

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color;

        // Get vertex world position.
        worldPos = mul(unity_ObjectToWorld,(v.vertex));

        // Simple nDotL lighting.
        float3 normal = UnityObjectToWorldNormal(v.normal);
        float3 lightDir = normalize(worldPos - _LightPos);
        float nDotL = dot(normal, -lightDir);
        float3 viewDir = normalize(WorldSpaceViewDir(v.vertex));
        float nDotV = dot(normal, viewDir);

        float lighting = nDotL * _LightIntensity;

        // Apply simple lighting at different rates per channel.
        // Red is more diffuse, while green and blue are more emissive.
        o.color.r += 0.25 * lighting;
        o.color.g += 0.25 * lighting;
        o.color.b += 0.125 * lighting;

        // Darken vertices that are closer to a defined ground plane
        float groundOccStart = worldPos.y - _GroundPos;
        float groundOccEnd = groundOccStart + 1;
        half groundMask = saturate(clamp(worldPos.y,groundOccStart,groundOccEnd));
        o.color.rgb = o.color.rgb * groundMask + 0.5 * o.color.rgb * (1-groundMask);

        // Multiply by tint color.
        o.color.rgb *= _Color;

        // Add highlight.
        float rim = 0.5 * (1 - nDotV);
        float highlightMask = saturate(_Highlight * rim);
        o.color.rgb = lerp(o.color.rgb, _HighlightColor.rgb, highlightMask);
        o.color.rgb += _HighlightColor.rgb * 0.5 * highlightMask;

        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        return i.color;
      }
      ENDCG
    }
  }
}
