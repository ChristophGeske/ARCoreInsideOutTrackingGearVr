///////////////////////////////////////////////////////////////////////////////
//Copyright 2017 Google Inc.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
///////////////////////////////////////////////////////////////////////////////

#include "UnityCG.cginc"
#include "daydreamShared.cginc"

struct appdata
{
 float4 vertex : POSITION;
 float3 nrm    : NORMAL;
 float4 tan    : TANGENT;
 float2 uv     : TEXCOORD0;
 #if defined(LIGHTMAP)
  float2 uv1 : TEXCOORD1;
 #elif defined(VERTEX_LIGHTING)
  float3 uv1 : COLOR;
  float3 uv2 : TEXCOORD2;
  float3 uv3 : TEXCOORD3;
 #endif
};

struct v2f
{
 float4 vertex : SV_POSITION;
 float4 uv     : TEXCOORD0;

#if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP)
 #if defined(NORMALMAP)
  half3  viewDir : TEXCOORD1;
  #if defined(SPECULAR_AA)
   half4  color0 : TEXCOORD2_centroid;
   half4  color1 : TEXCOORD3_centroid;
   half3  color2 : TEXCOORD4_centroid;
  #else
   half4  color0 : TEXCOORD2;
   half4  color1 : TEXCOORD3;
   half3  color2 : TEXCOORD4;
  #endif
  #if defined(SHADOWS)
   half4 shadowUV0 : TEXCOORD5;
  #endif
  #if defined(LIGHTMAP)
   half2 lightmapUV : TEXCOORD6;
  #endif
  #if defined(_DAYDREAM_FOG) || defined(_FOG_HEIGHT)
   half4 fogValue: TEXCOORD7;
  #endif
 #else
  half3  color : TEXCOORD1;
  #if defined(SPECULAR)
   half3  spec  : TEXCOORD2;
  #endif
  #if defined(SHADOWS)
   half4 shadowUV0 : TEXCOORD3;
  #endif
  #if defined(LIGHTMAP)
   half2 lightmapUV : TEXCOORD4;
  #endif
  #if defined(_DAYDREAM_FOG) || defined(_FOG_HEIGHT)
   half4 fogValue: TEXCOORD5;
  #endif
 #endif
#elif defined(REC_SHADOWMAP)
 half4 shadowUV0 : TEXCOORD1;
 half4 shadowUV1 : TEXCOORD2;
 half4 shadowUV2 : TEXCOORD3;
 half4 shadowUV3 : TEXCOORD4;
 #if defined(LIGHTMAP)
  float2 lightmapUV : TEXCOORD5;
 #endif
#elif defined(BUILD_SHADOWMAP)
 half depth : TEXCOORD1;
 #if defined(LIGHTMAP)
  float2 lightmapUV : TEXCOORD2;
 #endif
#endif
};

#define SPEC_POWER_LOW   16.0
#define SPEC_POWER_MED   64.0

sampler2D _MainTex;
sampler2D _NormalTex;

sampler2D _Shadowmap0;
sampler2D _Shadowmap1;
sampler2D _Shadowmap2;
sampler2D _Shadowmap3;

float4 _MainTex_ST;
float4 _NormalTex_ST;
float4 _BaseColor;
float  _Emissive;
float  _SpecSmoothness;
float  _ShadowIntens;
float  _ShadowFalloff;

float3 _globalAmbientUp;
float3 _globalAmbientDn;

//Shadows
float4x4 _shadowProj0;
float4x4 _shadowProj1;
float4x4 _shadowProj2;
float4x4 _shadowProj3;

//Fog
float  _fogMode;
//near, far, scale, colorScale
float4 _fogLinear;
//minHeight, maxHeight, thickness, density
float4 _fogHeight;
//colors
float4 _fogNearColor;
float4 _fogFarColor;

void computeSpecularValues(out float specPower, out float specIntens)
{
 float s = max(_SpecSmoothness, 0.1);
 if (s > 0.5)
 {
  //remap the higher range in order to account for resolution limits.
  s = (s - 0.5)*0.4 + 0.5;
 }
 float roughness = max(1.0 - s, 0.001);
 float r2 = roughness*roughness;
 float r4 = r2*r2;
 specPower = min(2.0 / r4 - 2.0, 4096);
 //specular normalization.
 specIntens = min(0.2f * (specPower + 2.0) * 0.15915494309189533576888376337251f, 4);
}

//approximately 10-13 cycles per pixel @60fps, scheduled over 192 ALU units (~10.8 in VR due to extra processing such as warping and scanline racing)
//192 ALU units, 194.4 GFlops (maximum @ half frequency),
//both 32 bit and 16 bit float registers that DO NOT ALIAS; conversions between 32 and 16 cost an extra instruction.
void computeSpecularParam(in half inPower, in half inItens, in half normalLen, out half out_specPower, out half out_specIntens)
{
 //approximate specular anti-aliasing, use the interpolated/mipped per-pixel normal length to approximate the specular LOD.
#if defined(SPECULAR_FIXED_LOW)
 out_specPower = SPEC_POWER_LOW;
#elif defined(SPECULAR_FIXED_MED)
 out_specPower = SPEC_POWER_MED;
#else
 out_specPower = inPower;
#endif
 out_specIntens = inItens;
}

void computeAmbient(in float3 nrm, in float3x3 viewToTangent, inout float3 color0, inout float3 color1, inout float3 color2)
{
#if defined(DYNAMIC_AMBIENT)
 //get the world UP vector in view space.
 float3 u = float3(UNITY_MATRIX_V[0].y, UNITY_MATRIX_V[1].y, UNITY_MATRIX_V[2].y);
 //transform the up direction into tangent space.
 u = normalize( mul(viewToTangent, u) );

 //"sky" ambient weights
 float  wu0 = saturate(dot( u, c_basis0));
 float  wu1 = saturate(dot( u, c_basis1));
 float  wu2 = saturate(dot( u, c_basis2));
 //"ground" ambient weights
 float  wd0 = saturate(dot(-u, c_basis0));
 float  wd1 = saturate(dot(-u, c_basis1));
 float  wd2 = saturate(dot(-u, c_basis2));
 //square the weights so that the transitions are linear (i.e. 45 degrees should map to 0.5, not 0.7071)
 wu0 *= wu0; wd0 *= wd0;
 wu1 *= wu1; wd1 *= wd1;
 wu2 *= wu2; wd2 *= wd2;

 //add the weighted ambient colors for each basis direction.
 color0 += wu0 *_globalAmbientUp + wd0*_globalAmbientDn;
 color1 += wu1 *_globalAmbientUp + wd1*_globalAmbientDn;
 color2 += wu2 *_globalAmbientUp + wd2*_globalAmbientDn;
#endif
}

void computeLightSetLocal(int index, in float3 viewPos, in float3 nrm, in float3 R, in float specPower, in float specIntens, inout float3 color, inout float3 spec)
{
 //Calculate the scaled light offsets.
 float3 light0Dir = dr_LightPosition[index + 0].xyz - viewPos*dr_LightPosition[index + 0].w;
 float3 light1Dir = dr_LightPosition[index + 1].xyz - viewPos*dr_LightPosition[index + 1].w;
 float3 light2Dir = dr_LightPosition[index + 2].xyz - viewPos*dr_LightPosition[index + 2].w;
 float3 light3Dir = dr_LightPosition[index + 3].xyz - viewPos*dr_LightPosition[index + 3].w;
 //Squared length
 half4 len2 = half4(dot(light0Dir, light0Dir), dot(light1Dir, light1Dir), dot(light2Dir, light2Dir), dot(light3Dir, light3Dir)); //13 ops
 //Normalize
 half4 scale = 1.0 / sqrt(len2);
 light0Dir *= scale.x;
 light1Dir *= scale.y;
 light2Dir *= scale.z;
 light3Dir *= scale.w;
 //Attenuation Constants
 half4 lightAtten = half4(dr_LightAtten[index + 0].z, dr_LightAtten[index + 1].z, dr_LightAtten[index + 2].z, dr_LightAtten[index + 3].z);
 half4 lightRange = half4(dr_LightAtten[index + 0].w, dr_LightAtten[index + 1].w, dr_LightAtten[index + 2].w, dr_LightAtten[index + 3].w);
 //Calculate Attenuation
 half4 angularAtten = computeAngularAttenuation(index, light0Dir, light1Dir, light2Dir, light3Dir);
 half4 atten = computeAttenuation(len2, lightAtten, lightRange) * angularAtten;

 float4 nDotL = float4( lightModel(dot(nrm, light0Dir)), lightModel(dot(nrm, light1Dir)), lightModel(dot(nrm, light2Dir)), lightModel(dot(nrm, light3Dir)) );
 color += dr_LightColor[index + 0] * atten.x * nDotL.x;
 color += dr_LightColor[index + 1] * atten.y * nDotL.y;
 color += dr_LightColor[index + 2] * atten.z * nDotL.z;
 color += dr_LightColor[index + 3] * atten.w * nDotL.w;

 #if defined(SPECULAR)
  //calculate the specular contribution
  float4 specMask = min(nDotL*8.0, 1.0) * specIntens * atten;

  //Approximate Specular

  #if defined(SPECULAR_COLORED) //[12 ALU ops]
   #if defined(SPECULAR_FIXED_LOW)
    spec += pow16(dot(light0Dir, R)) * specMask.x * dr_LightColor[index + 0];
    spec += pow16(dot(light1Dir, R)) * specMask.y * dr_LightColor[index + 1];
    spec += pow16(dot(light2Dir, R)) * specMask.z * dr_LightColor[index + 2];
    spec += pow16(dot(light3Dir, R)) * specMask.w * dr_LightColor[index + 3];
   #else
    spec += powf(dot(light0Dir, R), specPower) * specMask.x * dr_LightColor[index + 0];
    spec += powf(dot(light1Dir, R), specPower) * specMask.y * dr_LightColor[index + 1];
    spec += powf(dot(light2Dir, R), specPower) * specMask.z * dr_LightColor[index + 2];
    spec += powf(dot(light3Dir, R), specPower) * specMask.w * dr_LightColor[index + 3];
   #endif
  #else
   #if defined(SPECULAR_FIXED_LOW)
    spec += pow16(dot(light0Dir, R)) * specMask.x;
    spec += pow16(dot(light1Dir, R)) * specMask.y;
    spec += pow16(dot(light2Dir, R)) * specMask.z;
    spec += pow16(dot(light3Dir, R)) * specMask.w;
   #else
        // Compiler issue? When using multiple materials - max() prevents blackout
     spec += max(0.0001, powf(dot(light0Dir, R), specPower)) * specMask.x;
     spec += max(0.0001, powf(dot(light1Dir, R), specPower)) * specMask.y;
     spec += max(0.0001, powf(dot(light2Dir, R), specPower)) * specMask.z;
     spec += max(0.0001, powf(dot(light3Dir, R), specPower)) * specMask.w;
   #endif
  #endif
 #endif
}

void computeLighting(in float3 viewPos, in float3x3 viewToTangent, out float3 color0, out float3 color1, out float3 color2)
{
 color0 = 0.0;
 color1 = 0.0;
 color2 = 0.0;

 computeLightSet(0, viewPos, viewToTangent, color0, color1, color2);
#if defined(MAX_LIGHT_COUNT_8)
 computeLightSet(4, viewPos, viewToTangent, color0, color1, color2);
#endif
}

void computeLightingLocal(in float3 viewPos, in float3 nrm, in float3 viewDir, inout float3 color, inout float3 spec)
{
 //[R = reflect(view, tsNormal); ~7 ops: 2 mul, 5 madd
 half3 R = viewDir - 2.0*dot(nrm, viewDir) * nrm;
 half specPower, specIntens;
 computeSpecularValues(specPower, specIntens);

 half outSpecPower, outSpecIntens;
 computeSpecularParam(specPower, specIntens, 1.0, outSpecPower, outSpecIntens);

 computeLightSetLocal(0, viewPos, nrm, R, outSpecPower, outSpecIntens, color, spec);
#if defined(MAX_LIGHT_COUNT_8)
 computeLightSetLocal(4, viewPos, nrm, R, outSpecPower, outSpecIntens, color, spec);
#endif
}

void computeAmbientLocal(in float3 viewPos, in float3 nrm, inout float3 color)
{
#if defined(DYNAMIC_AMBIENT)
 //get the world UP vector in view space.
 float3 u = float3(UNITY_MATRIX_V[0].y, UNITY_MATRIX_V[1].y, UNITY_MATRIX_V[2].y);
 float wu = saturate(0.5*dot(u, nrm)+0.5);
 float wd = saturate(0.5*dot(-u, nrm)+0.5);
 color += wu*_globalAmbientUp + wd*_globalAmbientDn;
#endif
}

//a simple character shadow, ~12 ops
//supports overall intensity and fade by (approx) distance from caster to fragment.
//finally uses depth testing to avoid back projection and a mask - pre-filtering is assumed.
half computeShadow(half4 shadowUV, sampler2D shadowSampler)
{
 const half bias = 0.001;
 const half depthScale = 32.0;

 half2 texValue = tex2Dproj(shadowSampler, shadowUV).rg;
 half  depth = texValue.r*depthScale + bias;

 half depthDelta = depth - shadowUV.w;
 half fade = saturate(1.0 + depthDelta*_ShadowFalloff) * _ShadowIntens;

 half depthDeltaScaled = saturate(16.0*depthDelta);
 half shadow = 1.0 - texValue.g + depthDeltaScaled*texValue.g;

 return (shadow*fade + 1.0 - fade);
}

//////////////////////////////////////////////////////////////////////////
// Normalmapped diffuse lighting
// Approximate Cost: 37 ALU ops
// Final lighting is modulated with the BaseColor and Albedo texture color
//////////////////////////////////////////////////////////////////////////
#if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP)
 fixed4 computeDiffuseAndSpecular(v2f i)
 {
  fixed4 texDiffuse = tex2D(_MainTex, i.uv.xy);     //2 ops + fetch latency
  texDiffuse.rgb   *= _BaseColor.rgb;
  half emissive     = _Emissive * texDiffuse.a;

  #if defined(DETAILMAP)
   texDiffuse.rgb *= tex2D(_NormalTex, i.uv.zw).rgb;
  #endif

  half aoDiffuse = 1.0, aoSpec = 1.0;
  #if defined(AMBIENT_OCCLUSION)
   float aoBase = tex2D(_NormalTex, i.uv.zw).a;
   aoDiffuse = _AmbientOcclusionDiffuse*aoBase + 1.0 - _AmbientOcclusionDiffuse;
   aoSpec    = _AmbientOcclusionSpec*aoBase    + 1.0 - _AmbientOcclusionSpec;
  #endif

  #if defined(NORMALMAP)
   half3 view = i.viewDir;                     //assume the tesselation is high enough that the view direction is close enough to normalized, this saves ~7 ALU ops but will look bad with large polygons.

   #if defined(AMBIENT_OCCLUSION)
    half3 tsTextureNrm = 2*tex2D(_NormalTex, i.uv.zw).rgb-1;  //in the case we've packed ao, assume normal encoding
   #else
    half3 tsTextureNrm = UnpackNormal(tex2D(_NormalTex, i.uv.zw));  //3 ops + fetch latency
   #endif

   #ifndef NORMALMAP_FLIP_Y
    tsTextureNrm.y = -tsTextureNrm.y;
   #endif


   half nlen = rsqrt(dot(tsTextureNrm, tsTextureNrm)); //~7 ops, scalar rsqrt(4?) + dot3(3 ops)

   half3 tsNormal = tsTextureNrm*nlen;

   //Approximate Diffuse [24 ALU ops]
   half3 bcolor0 = i.color0.xyz * dot(c_basis0, tsNormal);
   half3 bcolor1 = i.color1.xyz * dot(c_basis1, tsNormal);
   half3 bcolor2 = i.color2.xyz * dot(c_basis2, tsNormal);
   half3 diffuse = bcolor0 + bcolor1 + bcolor2;
   #if defined(SPECULAR)


    //specular parameters
    half specPower, specIntens;
    computeSpecularParam(i.color0.w, i.color1.w, nlen, specPower, specIntens);
    specIntens = specIntens*texDiffuse.w;             //specular mask [1 ALU op]
                    //normalize the per-pixel normal, ~3 ops


    //[R = reflect(view, tsNormal); ~7 ops: 2 mul, 5 madd
    half3 R = view - 2.0*dot(tsNormal, view) * tsNormal;



    //Approximate Specular
    #if defined(SPECULAR_FIXED_LOW)
     half sw0 = pow16(dot(c_basis0, R));
     half sw1 = pow16(dot(c_basis1, R));
     half sw2 = pow16(dot(c_basis2, R));
    #else
     half sw0 = powf(dot(c_basis0, R), specPower);
     half sw1 = powf(dot(c_basis1, R), specPower);
     half sw2 = powf(dot(c_basis2, R), specPower);
    #endif

    #if defined(SPECULAR_COLORED) //[12 ALU ops]
     half3 spec = (bcolor0*sw0 + bcolor1*sw1 + bcolor2*sw2) * specIntens;
    #else //[4 ALU ops]
     half  spec = (bcolor0.x*sw0 + bcolor1.x*sw1 + bcolor2.x*sw2) * specIntens;
    #endif
   #else
    half  spec = 0;
   #endif
  #else
   half3 diffuse = i.color.xyz;
   #if defined(SPECULAR)
    half3 spec    = i.spec.xyz;
   #else
    half spec = 0;
   #endif
  #endif

  half3 outputColor;
 #if defined(SHADOWS)
  #if defined(SHDTYPE_PROJECTED)
   half shadow = computeShadow(i.shadowUV0, _Shadowmap0);
  #else
   half shadow = tex2Dproj(_Shadowmap0, i.shadowUV0).r;
  #endif

  #if defined(LIGHTMAP)
   diffuse = diffuse + DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)).rgb * _StaticLightingScale;
  #endif
  diffuse *= aoDiffuse;
  spec    *= aoSpec;
  outputColor = (diffuse*texDiffuse.rgb + spec)*shadow;
 #else
  #if defined(LIGHTMAP)
   diffuse = diffuse + DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)).rgb * _StaticLightingScale;
  #endif
  diffuse *= aoDiffuse;
  spec    *= aoSpec;
  outputColor = diffuse*texDiffuse.rgb + spec;
 #endif

  //--------------//
  // Translucency //
  //--------------//
  outputColor.rgb = lerp(outputColor.rgb, texDiffuse.rgb, emissive);
  #if defined(_ALPHATEST_ON)
   clip(texDiffuse.a - 0.5);
  #endif
  texDiffuse.a *= _BaseColor.a;
  #if defined(_ALPHAPREMULTIPLY_ON)
   outputColor.rgb *= texDiffuse.a;
  #endif

  return fixed4(outputColor.rgb, texDiffuse.a);
 }
#endif

////////////////////////////////////////////////////////
//Distance Fog: 15 - 24 ALU ops
//Height Fog: +14 ALU ops
//Sphere Fog: +36 ALU ops
//Total fog cost (approx) in ALU ops:
//   Base 15 - 24
//      +Height Fog 29 - 38 (if not using SphereFog - this may be 2 ops cheaper since dir.xy isn't needed, 27-36)
//    +Sphere Fog 51 - 60
//    +Height +Sphere 65 - 74
////////////////////////////////////////////////////////
// Conclusions: Height fog is a reasonable extra cost Sphere fog is most likely
//        too expensive  in many cases (especially when using both height & sphere fog)
////////////////////////////////////////////////////////
half4 computeFog(half3 posWorld)
{
 half4 fogValue;
 half3 relPos = posWorld.xyz - _WorldSpaceCameraPos.xyz; //3
 half  sceneDepth = length(relPos);            //10

 #if defined(_FOG_HEIGHT)
  half3 dir = relPos / sceneDepth;            //4 (or 2 if dir.xy is unused later)

  //Volume height
  half dH = _fogHeight.y; //dH = maxY - minY
  //Ray/Volume intersections (y0,y1) scaled to the (-inf, 1] range.
  //Note that the ray is clipped to the top of the volume but allowed to pass through the bottom.
  //[To clip to the volume bottom as well, change min(y, 1) to saturate(y) for both y0 and y1]
  half y0 = min(_WorldSpaceCameraPos.y - _fogHeight.x, dH); //2
  half y1 = min(posWorld.y - _fogHeight.x, dH);       //2
  //Magnitude of the difference in Y between intersections scaled by the fog "thickness"
  half dh = abs(y0 - y1)*_fogHeight.z;            //3

  //compute the length of the ray passing through the volume bounding by [_fogHeight.x, _fogHeight.y] with a thickness of [_fogHeight.z]
  //the ray is clipped by the height bounds, this handles the camera being inside or outside of the volume
  //Note that the thickness artificially scales the length of the ray in order to simulate thicker height fog.
  //--------------------------------------------------------------------------------------------------------------------------------------
  //If the ray is not clipped by the volume, the camera is inside and the "thickness" is 1.0 - then this is equivalent to depth based fog.
  half L = dh / abs(dir.y);                 //3
 #else
  //the camera is considered to be inside an infinite volume of fog so only the distance from the camera to the surface is important.
  half L = sceneDepth;
 #endif

 half Lw;
 if (_fogMode == 0.0)
 {
  Lw = saturate(L*_fogLinear.x + _fogLinear.y); //2
  fogValue.w = Lw;
 }
 else if (_fogMode == 1.0)
 {
  Lw = L*_fogHeight.w;              //8
  fogValue.w = 1.0 - exp(-Lw);
 }
 else if (_fogMode == 2.0)
 {
  Lw = L*_fogHeight.w;              //9
  half e = exp(-Lw);
  fogValue.w = 1.0 - e*e;
 }

 //fog depth color
 half colorFactor = smoothstep(0.0, 1.0, Lw * _fogLinear.w);
 fogValue.rgb = lerp(_fogNearColor.rgb, _fogFarColor.rgb, colorFactor);

 //overall fog opacity
 fogValue.w *= _fogLinear.z;

 return fogValue;
}

v2f daydreamVertex(appdata v)
{
 v2f o = (v2f)0;
 o.vertex = UnityObjectToClipPos(v.vertex);
 o.uv.xy = v.uv * _MainTex_ST.xy   + _MainTex_ST.zw;
 o.uv.zw = v.uv * _NormalTex_ST.xy + _NormalTex_ST.zw;

 #if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP)
  #if defined(DISABLE_LIGHTING)
   // add vertex baked lighting
   #if defined(NORMALMAP)
    #if defined(VERTEX_LIGHTING)
     o.color0.xyz = v.uv1.xyz * _StaticLightingScale;
     o.color1.xyz = v.uv2.xyz * _StaticLightingScale;
     o.color2.xyz = v.uv3.xyz * _StaticLightingScale;

     //We still want specular from static lighting, so there is still work to be done for that.
     float3 viewPos = mul(UNITY_MATRIX_MV, v.vertex).xyz;
     float3   nrm = normalize(mul(UNITY_MATRIX_MV, float4(v.nrm.xyz, 0)).xyz);
     float3   tan = normalize(mul(UNITY_MATRIX_MV, float4(v.tan.xyz, 0)).xyz);
     float3   binormal = cross(nrm, tan) * v.tan.w;
     float3x3 viewToTangent = float3x3(tan, binormal, nrm);
     #if defined(SPECULAR)
      computeSpecularValues(o.color0.w, o.color1.w);
     #endif
     o.viewDir = normalize(mul(viewToTangent, viewPos));
    #elif LIGHTMAP
     o.color0.xyz = 0.0;
     o.color1.xyz = 0.0;
     o.color2.xyz = 0.0;
    #else
     o.color0.xyz = 1.0;
     o.color1.xyz = 1.0;
     o.color2.xyz = 1.0;
    #endif
   #else
    #if defined(VERTEX_LIGHTING)
     o.color.xyz = (v.uv1.xyz + v.uv2.xyz + v.uv3.xyz) * 0.333333 * _StaticLightingScale;
    #elif LIGHTMAP
     o.color.xyz = 0.0;
    #else
     o.color.xyz = 1.0;
    #endif
    #if defined(SPECULAR)
     o.spec = 0.0;
    #endif
   #endif
  #else
   //View space position of the vertex
   float3 viewPos = mul(UNITY_MATRIX_MV, v.vertex).xyz;

   #if defined(SHADOWS) || defined(_DAYDREAM_FOG) || defined(_FOG_HEIGHT)
    half3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
   #endif

   //Build the Word Space to Tangent Space transform.
   #if defined(NORMALMAP)
    float3   nrm = normalize(mul(UNITY_MATRIX_MV, float4(v.nrm.xyz, 0)).xyz);
    float3   tan = normalize(mul(UNITY_MATRIX_MV, float4(v.tan.xyz, 0)).xyz);
    float3   binormal = cross(nrm, tan) * v.tan.w;
    float3x3 viewToTangent = float3x3(tan, binormal, nrm);

    computeLighting(viewPos, viewToTangent, o.color0.xyz, o.color1.xyz, o.color2.xyz);
    computeAmbient(nrm, viewToTangent, o.color0.xyz, o.color1.xyz, o.color2.xyz);

    // add vertex baked lighting
    #if defined(VERTEX_LIGHTING)
     o.color0.xyz += v.uv1.xyz * _StaticLightingScale;
     o.color1.xyz += v.uv2.xyz * _StaticLightingScale;
     o.color2.xyz += v.uv3.xyz * _StaticLightingScale;
    #endif

    #if defined(SPECULAR)
     computeSpecularValues(o.color0.w, o.color1.w);
    #endif

    o.viewDir = normalize(mul(viewToTangent, viewPos));
   #else
    float3 nrm = normalize(mul(UNITY_MATRIX_MV, float4(v.nrm.xyz, 0)).xyz);
    o.color.xyz = 0.0;

    half3 viewDir = normalize(viewPos);
    #if defined(SPECULAR)
     o.spec = 0.0;
     computeLightingLocal(viewPos, nrm, viewDir, o.color.xyz, o.spec);
    #else
     float3 tmp =0;
     computeLightingLocal(viewPos, nrm, viewDir, o.color.xyz, tmp);
    #endif
    computeAmbientLocal(viewPos, nrm, o.color.xyz);

    // add vertex baked lighting
    #if defined(VERTEX_LIGHTING)
     o.color.xyz += (v.uv1.xyz + v.uv2.xyz + v.uv3.xyz) * _StaticLightingScale;
    #endif
   #endif
  #endif

  #if defined(SHADOWS)
   o.shadowUV0 = mul(_shadowProj0, float4(posWorld, 1.0));
  #endif

  #if defined(LIGHTMAP)
   o.lightmapUV = v.uv1*unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  #if defined(_DAYDREAM_FOG) || defined(_FOG_HEIGHT)
   o.fogValue = computeFog(posWorld);
  #endif
 #elif REC_SHADOWMAP
  half3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
  o.shadowUV0 = mul(_shadowProj0, float4(posWorld, 1.0));
  o.shadowUV1 = mul(_shadowProj1, float4(posWorld, 1.0));
  o.shadowUV2 = mul(_shadowProj2, float4(posWorld, 1.0));
  o.shadowUV3 = mul(_shadowProj3, float4(posWorld, 1.0));
 #elif BUILD_SHADOWMAP
  o.depth = o.vertex.w;
 #endif

 return o;
}

fixed4 daydreamFragment(v2f i)
{
 #if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP)
  half4 outColor = computeDiffuseAndSpecular(i);

  #if defined(_DAYDREAM_FOG) || defined(_FOG_HEIGHT)
   outColor.rgb = lerp(outColor.rgb, i.fogValue.rgb, saturate(i.fogValue.w));
   #if _ALPHABLEND_ON
    outColor.a = 1.0 - (1.0 - outColor.a) * max(1.0 - i.fogValue.w, 0.0);
   #endif
  #endif

  return outColor;
 #elif (REC_SHADOWMAP)
  half shadow;
  shadow = computeShadow(i.shadowUV0, _Shadowmap0);
  shadow = min(computeShadow(i.shadowUV1, _Shadowmap1), shadow);
  shadow = min(computeShadow(i.shadowUV2, _Shadowmap2), shadow);
  shadow = min(computeShadow(i.shadowUV3, _Shadowmap3), shadow);

  return half4(shadow, 0, 0, 1);
 #elif (BUILD_SHADOWMAP) //Shadow map
  const half depthScale = 1.0 / 32.0;
  //convert to per-pixel z, scaled so that z = 32.0 maps to 1.0
  half wScaled = i.depth * depthScale;
  return half4(wScaled, 1.0, 1.0, 1.0);
 #endif
}
