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

float  _Wrap;
float  _StaticLightingScale;
float  _AmbientOcclusionDiffuse;
float  _AmbientOcclusionSpec;

#if defined(_USE_ENLIGHTEN)
	#define dr_LightColor    unity_LightColor
	#define dr_LightPosition unity_LightPosition
	#define dr_LightAtten	 unity_LightAtten
	#define dr_SpotDirection unity_SpotDirection
#else
	// light color
	half4 dr_LightColor[8];

	// view-space vertex light positions (position,1), or (-direction,0) for directional lights.
	float4 dr_LightPosition[8];

	// x = cos(spotAngle/2)   or -1 for non-spot
	// y = 1/cos(spotAngle/4) or  1 for non-spot
	// z = quadratic attenuation
	// w = range*range
	half4 dr_LightAtten[8];

	// view-space spot light directions, or (0,0,1,0) for non-spot
	float4 dr_SpotDirection[8];
#endif

//TANGENT SPACE BASIS VECTORS:
// (-1/sqrt(6), -1/sqrt(2), 1/sqrt(3))
// (-1/sqrt(6),  1/sqrt(2), 1/sqrt(3))
// (sqrt(2/3),   0        , 1/sqrt(3))
static const float3 c_basis0 = float3(-0.40824829046386301636621401245098f, -0.70710678118654752440084436210485f, 0.57735026918962576450914878050195f);
static const float3 c_basis1 = float3(-0.40824829046386301636621401245098f, 0.70710678118654752440084436210485f, 0.57735026918962576450914878050195f);
static const float3 c_basis2 = float3(0.81649658092772603273242802490196f, 0.0f, 0.57735026918962576450914878050195f);

#if defined(SHADER_API_MOBILE)
#define powf(x, y) pow(x, y)
#else
#define powf(x, y) pow(max(x,0.0), y)
#endif

//n^16 implemented as 4 scalcar multiplies.
half pow16(half n)
{
	//clamp the results to 0 to 1 range to avoid edge pixels blowing out.
	n = clamp(n, 0, 1);

	n = n*n;	//^2
	n = n*n;	//^4
	n = n*n;	//^8
	return n*n;	//^16
}

float lightModel(float nDotL)
{
	return max(0.5 * (nDotL*(2.0 - _Wrap) + _Wrap), 0.0);
}

half4 computeAngularAttenuation(int index, half3 light0Dir, half3 light1Dir, half3 light2Dir, half3 light3Dir)
{
	half4 cosOuter = half4(dr_LightAtten[index + 0].x, dr_LightAtten[index + 1].x, dr_LightAtten[index + 2].x, dr_LightAtten[index + 3].x);
	half4 cosRange = half4(dr_LightAtten[index + 0].y, dr_LightAtten[index + 1].y, dr_LightAtten[index + 2].y, dr_LightAtten[index + 3].y);
	half4 cosAng   = half4(dot(dr_SpotDirection[index + 0].xyz, light0Dir), dot(dr_SpotDirection[index + 1].xyz, light1Dir), dot(dr_SpotDirection[index + 2].xyz, light2Dir), dot(dr_SpotDirection[index + 3].xyz, light3Dir));
	cosAng = max(cosAng, 0);

	return saturate((cosAng - cosOuter) * cosRange);
}

half4 computeAttenuation(half4 len2, half4 attenConst, half4 lightRange2)
{
#if defined(ATTEN_CLAMP)  //this is much more aggressive than Unity (causing lighting to be darker) but is required to completely avoid popping.
	float cutoff = 0.75;
	float scale = 1.0 / (1.0 - cutoff);
	//Calculate Attenuation
	half4 atten = 1.0 / (1.0 + len2*attenConst);
	//Make sure there is no pop...
	half4 unitLen2 = len2 / lightRange2;
	half4 fade = 1.0 - saturate((unitLen2 - cutoff) * scale);
	atten *= fade;

	return max(atten, 0.0);
#else	//this is closer to Unity's behavior, though it does still do some cutoff; but it won't work with bright lights.
	const half cutoff = 0.01;	//attenuation value that is remapped to zero.
								//Calculate Attenuation
	half4 atten = 1.0 / (1.0 + len2*attenConst);
	//Smooth cutoff so the light doesn't extend forever.
	atten = (atten - cutoff) / (1.0 - cutoff);
	return max(atten, 0.0);
#endif
}

//calculate the contribution of 4 lights
void computeLightSet(int index, in float3 viewPos, in float3x3 viewToTangent, inout float3 color0, inout float3 color1, inout float3 color2)
{
	//Calculate the scaled light offsets.
	float3 light0Dir = dr_LightPosition[index + 0].xyz - viewPos*dr_LightPosition[index + 0].w;
	float3 light1Dir = dr_LightPosition[index + 1].xyz - viewPos*dr_LightPosition[index + 1].w;
	float3 light2Dir = dr_LightPosition[index + 2].xyz - viewPos*dr_LightPosition[index + 2].w;
	float3 light3Dir = dr_LightPosition[index + 3].xyz - viewPos*dr_LightPosition[index + 3].w;
	//Squared length
	half4 len2 = half4(dot(light0Dir, light0Dir), dot(light1Dir, light1Dir), dot(light2Dir, light2Dir), dot(light3Dir, light3Dir));	//13 ops
																																	//Normalize light directions.
	light0Dir *= rsqrt(len2.x);
	light1Dir *= rsqrt(len2.y);
	light2Dir *= rsqrt(len2.z);
	light3Dir *= rsqrt(len2.w);
	//Calculate Angular Attenuation
	half4 angularAtten = computeAngularAttenuation(index, light0Dir, light1Dir, light2Dir, light3Dir);
	//Transform light offsets into tangent space
	light0Dir = mul(viewToTangent, light0Dir.xyz);
	light1Dir = mul(viewToTangent, light1Dir.xyz);
	light2Dir = mul(viewToTangent, light2Dir.xyz);
	light3Dir = mul(viewToTangent, light3Dir.xyz);
	//Attenuation Constants
	half4 lightAtten = half4(dr_LightAtten[index + 0].z, dr_LightAtten[index + 1].z, dr_LightAtten[index + 2].z, dr_LightAtten[index + 3].z);
	half4 lightRange = half4(dr_LightAtten[index + 0].w, dr_LightAtten[index + 1].w, dr_LightAtten[index + 2].w, dr_LightAtten[index + 3].w);
	//Calculate Attenuation
	half4 atten = computeAttenuation(len2, lightAtten, lightRange) * angularAtten;
	//Add each secondary light contribution to each basis vector
	//Color 0
	color0 += lightModel(dot(light0Dir, c_basis0)) * dr_LightColor[index + 0] * atten.x;
	color0 += lightModel(dot(light1Dir, c_basis0)) * dr_LightColor[index + 1] * atten.y;
	color0 += lightModel(dot(light2Dir, c_basis0)) * dr_LightColor[index + 2] * atten.z;
	color0 += lightModel(dot(light3Dir, c_basis0)) * dr_LightColor[index + 3] * atten.w;
	//Color 1
	color1 += lightModel(dot(light0Dir, c_basis1)) * dr_LightColor[index + 0] * atten.x;
	color1 += lightModel(dot(light1Dir, c_basis1)) * dr_LightColor[index + 1] * atten.y;
	color1 += lightModel(dot(light2Dir, c_basis1)) * dr_LightColor[index + 2] * atten.z;
	color1 += lightModel(dot(light3Dir, c_basis1)) * dr_LightColor[index + 3] * atten.w;
	//Color 2
	color2 += lightModel(dot(light0Dir, c_basis2)) * dr_LightColor[index + 0] * atten.x;
	color2 += lightModel(dot(light1Dir, c_basis2)) * dr_LightColor[index + 1] * atten.y;
	color2 += lightModel(dot(light2Dir, c_basis2)) * dr_LightColor[index + 2] * atten.z;
	color2 += lightModel(dot(light3Dir, c_basis2)) * dr_LightColor[index + 3] * atten.w;
}

//calculate the contribution of 4 lights
void computeLightSetParticle(int index, in float3 viewPos, in float3 nrm, inout float3 color)
{
	//Calculate the scaled light offsets.
	float3 light0Dir = dr_LightPosition[index + 0].xyz - viewPos*dr_LightPosition[index + 0].w;
	float3 light1Dir = dr_LightPosition[index + 1].xyz - viewPos*dr_LightPosition[index + 1].w;
	float3 light2Dir = dr_LightPosition[index + 2].xyz - viewPos*dr_LightPosition[index + 2].w;
	float3 light3Dir = dr_LightPosition[index + 3].xyz - viewPos*dr_LightPosition[index + 3].w;
	//Squared length
	half4 len2 = half4(dot(light0Dir, light0Dir), dot(light1Dir, light1Dir), dot(light2Dir, light2Dir), dot(light3Dir, light3Dir));	//13 ops
	//Normalize
	half4 scale = 1.0 / sqrt(len2);
	light0Dir *= scale.x;
	light1Dir *= scale.y;
	light2Dir *= scale.z;
	light3Dir *= scale.w;
	//Attenuation Constants
	half4 angularAtten = computeAngularAttenuation(index, light0Dir, light1Dir, light2Dir, light3Dir);
	half4 lightAtten = half4(dr_LightAtten[index + 0].z, dr_LightAtten[index + 1].z, dr_LightAtten[index + 2].z, dr_LightAtten[index + 3].z);
	half4 lightRange = half4(dr_LightAtten[index + 0].w, dr_LightAtten[index + 1].w, dr_LightAtten[index + 2].w, dr_LightAtten[index + 3].w);
	//Calculate Attenuation
	half4 atten = computeAttenuation(len2, lightAtten, lightRange) * angularAtten;
	//Final sum	
	color += dr_LightColor[index + 0] * atten.x;
	color += dr_LightColor[index + 1] * atten.y;
	color += dr_LightColor[index + 2] * atten.z;
	color += dr_LightColor[index + 3] * atten.w;
}
