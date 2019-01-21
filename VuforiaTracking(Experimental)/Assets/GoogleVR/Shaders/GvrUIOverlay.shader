// Copyright 2016 Google Inc. All rights reserved.
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

Shader "GoogleVR/UI/Overlay" {
  Properties {
    [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    _Color ("Tint", Color) = (1,1,1,1)

    _StencilComp ("Stencil Comparison", Float) = 8
    _Stencil ("Stencil ID", Float) = 0
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255

    _ColorMask ("Color Mask", Float) = 15

    [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
  }

  SubShader {
    Tags {
    // Overlay+110 fixes depth sorting between UI and controller.
    "Queue"="Overlay+110"
    "IgnoreProjector"="True"
    "RenderType"="Transparent"
    "PreviewType"="Plane"
    "CanUseSpriteAtlas"="True"
    }

    Stencil {
      Ref [_Stencil]
      Comp [_StencilComp]
      Pass [_StencilOp]
      ReadMask [_StencilReadMask]
      WriteMask [_StencilWriteMask]
    }

    Cull Off
    Lighting Off
    ZWrite Off
    ZTest [unity_GUIZTestMode]
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask [_ColorMask]

    Pass {
      Name "Default"
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0

      #include "UnityCG.cginc"
      #include "UnityUI.cginc"

      #pragma multi_compile __ UNITY_UI_ALPHACLIP

      struct appdata_t {
        float4 vertex   : POSITION;
        float4 color    : COLOR;
        float2 texcoord : TEXCOORD0;
      };

      struct v2f {
        float4 vertex   : SV_POSITION;
        fixed4 color    : COLOR;
        half2 texcoord  : TEXCOORD0;
        float4 worldPosition : TEXCOORD1;
      };

      fixed4 _Color;
      fixed4 _TextureSampleAdd;
      float4 _ClipRect;

      v2f vert(appdata_t IN) {
        v2f OUT;
        OUT.worldPosition = IN.vertex;
        OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

        OUT.texcoord = IN.texcoord;

        #ifdef UNITY_HALF_TEXEL_OFFSET
        OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1) * OUT.vertex.w;
        #endif  // UNITY_HALF_TEXEL_OFFSET

        OUT.color = IN.color * _Color;
        return OUT;
      }

      sampler2D _MainTex;

      fixed4 frag(v2f IN) : SV_Target {
        half4 color =
          (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

        color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

        #ifdef UNITY_UI_ALPHACLIP
        clip (color.a - 0.001);
        #endif  // UNITY_UI_ALPHACLIP

        return color;
      }
      ENDCG
    }
  }
}
