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

Shader "DaydreamElements/ArmModels/Unlit Vertex Color Outline" {
  Properties {
      [PerRendererData]_Alpha ("Alpha", Range(0, 1)) = 1
      _Color("Color", Color) = (1,1,1,1)
      [Toggle(OUTLINE_ENABLED)] _OutlineEnabled ("Outline Enabled", Float) = 0
      _Outline("Outline Thickness", Range(0.0, 0.3)) = 0.002
      _OutlineColor("Outline Color", Color) = (0,0,0,1)

  }

  SubShader {
    Tags {
      "Queue"="Transparent"
      "IgnoreProjector" = "True"
      "RenderType"="Transparent"
    }

    LOD 100

    Pass {
      Name "Outline"

      Cull Off
      Offset 3, 3
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha


      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile ___ OUTLINE_ENABLED


     #include "UnityCG.cginc"

      struct appdata {
        half4 vertex : POSITION;
        half3 normal : NORMAL;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
      };

      half _Outline;
      half4 _OutlineColor;
      float4x4 _MirrorMVP;

      v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);

#ifdef OUTLINE_ENABLED
        float3 viewNormal = COMPUTE_VIEW_NORMAL;
        o.vertex.xyz += _Outline * viewNormal;
        o.color = _OutlineColor;
#else
        o.color = float4(0, 0, 0, 0);
#endif
        return o;
      }

      fixed4 frag(v2f i) : COLOR {
        fixed4 o;
        o = i.color;
        return o;
      }
      ENDCG
    }

    Pass {
      Name "Mask"

      Offset 4, 4
      ColorMask 0
    }

    Pass {
      Name "Vert Color"

      Cull Back
      Offset 4, 4
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        half4 color : COLOR;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        half4 color : COLOR;
      };

      float _Alpha;
      float4 _Color;

      v2f vert (appdata v) {
        v2f o;

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color * _Color;
        o.color.a = _Alpha * _Color.a;

        return o;
      }

      half4 frag (v2f i) : SV_Target {
        half4 col = i.color;
        return col;
      }
      ENDCG
    }
  }
}
