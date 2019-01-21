//========================================================================
// Copyright (c) 2017 PTC Inc. All Rights Reserved.
//
// Vuforia is a trademark of PTC Inc., registered in the United States and other
// countries.
//=========================================================================

Shader "Custom/CameraDiffuse"
{   
    Properties 
    {
        _MaterialColor ("Color", Color) = (1,1,1,1)
    }

    CGINCLUDE

    uniform float4 _MaterialColor;

    ENDCG

    SubShader
    {
        Pass
        {
            // indicate that our pass is the "base" pass in forward
            // rendering pipeline. It gets ambient and main directional
            // light data set up; light direction in _WorldSpaceLightPos0
            // and color in _LightColor0
            Tags {"LightMode"="ForwardBase"}
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0

            struct v2f
            {
                fixed4 diff : COLOR0; // diffuse lighting color
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);


                // get vertex normal in world space
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // compute world space view direction
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting "(support double-sided material)" 
                half nl = abs(dot(worldNormal, worldViewDir));
                
                // factor in the material color
                o.diff = lerp(_MaterialColor, nl * _MaterialColor, 0.2);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return i.diff;
            }
            ENDCG
        }
    }
}