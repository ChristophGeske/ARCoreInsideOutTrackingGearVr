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

/// Used in conjunction with _Mirror.cs_ to render a stereoscopic mirror.
Shader "DaydreamElements/Demo/Mirror" {
  Properties {
    [HideInInspector] _LeftEyeTex ("Base (RGB)", 2D) = "white" {}
    [HideInInspector] _RightEyeTex ("Base (RGB)", 2D) = "white" {}
    [PerRendererData] _Alpha ("Alpha", Range(0, 1)) = 1
    _SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
    _SpecIntesnity ("Specular Intesntiy", float) = 1
    _Shininess ("Shininess", float) = 25
  }

  SubShader {
    Tags {
      "Queue"="Transparent"
      "IgnoreProjector" = "True"
      "RenderType"="Transparent"
    }

    LOD 100

    Cull Back
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"


      struct appdata_t {
       float4 vertex : POSITION;
       float3 normal : NORMAL;
      };

      struct v2f {
        float4 texcoord : TEXCOORD0;
        float4 vertex : SV_POSITION;
        float4 posWorld : TEXCOORD1;
        float4 lightColor : COLOR0;
        float3 normal : NORMAL;
      };

      sampler2D _LeftEyeTex;
      sampler2D _RightEyeTex;
      float4 _SpecColor;
      float _Shininess;
      float _SpecIntesnity;
      float _Alpha;
      fixed4 _LightColor0;

      v2f vert(appdata_t v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.posWorld = mul(unity_ObjectToWorld, v.vertex);
        o.texcoord = ComputeNonStereoScreenPos(o.vertex);
        o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
        return o;
      }

      fixed4 frag(v2f i) : SV_Target {
        fixed4 tex;

        if (unity_StereoEyeIndex == 1) {
          tex = tex2Dproj(_RightEyeTex, UNITY_PROJ_COORD(i.texcoord));
        } else {
          tex = tex2Dproj(_LeftEyeTex, UNITY_PROJ_COORD(i.texcoord));
        }

        float3 normalDirection = normalize(i.normal);

        float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
        float3 lightDirection;
        float attenuation;

        if (0.0 == _WorldSpaceLightPos0.w) {
          attenuation = 1.0;
          lightDirection = normalize(_WorldSpaceLightPos0.xyz);
        } else {
          float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
          float distance = length(vertexToLightSource);
          attenuation = 1.0 / distance;
          lightDirection = normalize(vertexToLightSource);
        }

        float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb;

        float3 diffuseReflection = attenuation * _LightColor0.rgb * max(0.0, dot(normalDirection, lightDirection));

        float3 specularReflection;
        if (dot(normalDirection, lightDirection) < 0.0)  {
          specularReflection = float3(0.0, 0.0, 0.0);
        } else {
          specularReflection = _SpecIntesnity * attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
        }

        return float4(tex.rgb * (ambientLighting + diffuseReflection + specularReflection), _Alpha);
      }
      ENDCG
    }
  }
}