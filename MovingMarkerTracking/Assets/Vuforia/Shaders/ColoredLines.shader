//===============================================================================
//Copyright (c) 2015-2016 PTC Inc. All Rights Reserved.
//
//Confidential and Proprietary - Protected under copyright and other laws.
//Vuforia is a trademark of PTC Inc., registered in the United States and other
//countries.
//===============================================================================

Shader "Custom/ColoredLines" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    
    SubShader {
        Pass { 
            Lighting Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            Color [_Color]
        }
    } 
}
