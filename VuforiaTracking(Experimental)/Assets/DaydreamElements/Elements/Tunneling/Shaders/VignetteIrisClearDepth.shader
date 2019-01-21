// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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

Shader "DaydreamElements/Tunneling/VignetteIrisClearDepth" {
 Properties {
 }
 SubShader {
   Tags { "RenderType"="Opaque" "Queue"="Geometry-100" }
   LOD 100

   Pass {
     ColorMask 0
     ZTest Always
     ZWrite On
     Cull Front
     CGPROGRAM
     #pragma vertex vert
     #pragma fragment frag

     struct appdata {
       float4 vertex : POSITION;
     };

     struct v2f {
       float4 vertex : SV_POSITION;
     };


     // The forward vector of the 'head' camera,
     // we aren't using camera position by design
     float4 _MainCameraForward;

     float4 _VignetteMinMax;

     v2f vert(appdata v) {
       v2f o;

       float scaledLocation = 1.57079632679 *
          ((v.vertex.y * (1 - _VignetteMinMax.z) + _VignetteMinMax.z));
       float xzDist = _VignetteMinMax.w * sin(scaledLocation);
       float yDist = _VignetteMinMax.w * -cos(scaledLocation) + 1;
       float4 vertexPosition = float4(xzDist * v.vertex.x, yDist, xzDist * v.vertex.z, 1);

       o.vertex = UnityObjectToClipPos (vertexPosition);

       return o;
     }

     fixed4 frag(v2f i) : SV_Target {
       return 0;
     }
     ENDCG
   }
 }
}
