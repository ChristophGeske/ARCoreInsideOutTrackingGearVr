/*===============================================================================
Copyright 2017-2018 PTC Inc.

Licensed under the Apache License, Version 2.0 (the "License"); you may not
use this file except in compliance with the License. You may obtain a copy of
the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
CONDITIONS OF ANY KIND, either express or implied. See the License for the
specific language governing permissions and limitations under the License.
===============================================================================*/

Shader "Custom/VideoBackground" {
    // Used to render the Vuforia Video Background

    Properties
    {
        [NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
        [NoScaleOffset] _UVTex("UV Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags {"Queue" = "geometry-11" "RenderType" = "opaque" }
        Pass {
            ZWrite Off
            Cull Off
            Lighting Off

             CGPROGRAM

            #pragma multi_compile VUFORIA_RGB VUFORIA_YUVNV12 VUFORIA_YUVNV21

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"


            sampler2D _MainTex;
            float4 _MainTex_ST;
#if (VUFORIA_YUVNV12 || VUFORIA_YUVNV21)
            sampler2D _UVTex;
            float4 _UVTex_ST;
#endif

            struct v2f {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
#if (VUFORIA_YUVNV12 || VUFORIA_YUVNV21)
                float2  uv2 : TEXCOORD1;
#endif
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
#if (VUFORIA_YUVNV12 || VUFORIA_YUVNV21)
                o.uv2 = TRANSFORM_TEX(v.texcoord, _UVTex);
#endif				               
                return o;
            }

#if (VUFORIA_YUVNV12 || VUFORIA_YUVNV21)
               half4 frag(v2f i) : COLOR
            {
                half4 c;
                half2 uv = tex2D(_UVTex, i.uv2).rg;
                float y = tex2D(_MainTex, i.uv).r;

#if VUFORIA_YUVNV12				
                half4 v4yuv1 = half4(y, uv, 1.0);

                c.r = dot(half4(1.1640625,  0.000000000,  1.5957031250, -0.87060546875), v4yuv1);
                c.g = dot(half4(1.1640625, -0.390625000, -0.8134765625,  0.52929687500), v4yuv1);
                c.b = dot(half4(1.1640625,  2.017578125,  0.0000000000, -1.08154296875), v4yuv1);
                c.a = 1.0;
#else               
                half4 v4yuv1 = half4(y, uv, 1.0);

                c.r = dot(half4(1.1640625,  1.5957031250,  0.000000000, -0.87060546875), v4yuv1);
                c.g = dot(half4(1.1640625, -0.8134765625, -0.390625000,  0.52929687500), v4yuv1);
                c.b = dot(half4(1.1640625,  0.0000000000,  2.017578125, -1.08154296875), v4yuv1);
                c.a = 1.0;
#endif

#ifdef UNITY_COLORSPACE_GAMMA
                return c;
#else
                return fixed4(GammaToLinearSpace(c.rgb), c.a);
#endif	
            }
#else
            half4 frag(v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);

                c.rgb = c.rgb;
                c.a = 1.0;

#ifdef UNITY_COLORSPACE_GAMMA
                return c;
#else
                return fixed4(GammaToLinearSpace(c.rgb), c.a);
#endif	
            }
#endif
            ENDCG
        }
    }
        Fallback "Legacy Shaders/Diffuse"
}
