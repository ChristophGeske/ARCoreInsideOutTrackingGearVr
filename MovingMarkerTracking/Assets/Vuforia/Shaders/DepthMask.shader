//===============================================================================
//Copyright (c) 2015 PTC Inc. All Rights Reserved.
//
//Confidential and Proprietary - Protected under copyright and other laws.
//Vuforia is a trademark of PTC Inc., registered in the United States and other
//countries.
//===============================================================================
//===============================================================================
//Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
//All Rights Reserved.
//Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
//===============================================================================

Shader "DepthMask" {
   
    SubShader {
        // Render the mask after regular geometry, but before masked geometry and
        // transparent things.
       
        Tags {"Queue" = "Geometry-10" }
       
        // Turn off lighting, because it's expensive and the thing is supposed to be
        // invisible anyway.
       
        Lighting Off

        // Draw into the depth buffer in the usual way.  This is probably the default,
        // but it doesn't hurt to be explicit.

        ZTest LEqual
        ZWrite On

        // Don't draw anything into the RGBA channels. This is an undocumented
        // argument to ColorMask which lets us avoid writing to anything except
        // the depth buffer.

        ColorMask 0

        // Do nothing specific in the pass:

        Pass {}
    }
}
