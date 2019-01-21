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

#if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP) && defined(PARTICLE_RENDERING)
	half3  color  : TEXCOORD1;
#elif !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP)
	#if defined(NORMALMAP)
		half3  color0 : TEXCOORD1;
		half3  color1 : TEXCOORD2;
		half3  color2 : TEXCOORD3;
		#if defined(SHADOWS)
			half4 shadowUV0 : TEXCOORD4;
		#endif
		#if defined(LIGHTMAP)
			half2 lightmapUV : TEXCOORD5;
		#endif
		#if defined(_DAYDREAM_FOG) || defined(_FOG_HEIGHT)
			half4 fogValue: TEXCOORD6;
		#endif
	#else
		half3  color: TEXCOORD1;
		#if defined(SHADOWS)
			half4 shadowUV0 : TEXCOORD2;
		#endif
		#if defined(LIGHTMAP)
			half2 lightmapUV : TEXCOORD3;
		#endif
		#if defined(_DAYDREAM_FOG) || defined(_FOG_HEIGHT)
			half4 fogValue: TEXCOORD4;
		#endif
	#endif
#elif defined(REC_SHADOWMAP)
	half4 shadowUV0 : TEXCOORD1;
	half4 shadowUV1 : TEXCOORD2;
	half4 shadowUV2 : TEXCOORD3;
	half4 shadowUV3 : TEXCOORD4;
#elif defined(BUILD_SHADOWMAP)
	half depth : TEXCOORD1;
#endif
};

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

void computeLightSetLocal(int index, in float3 viewPos, in float3 nrm, inout float3 color)
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
	half4 lightAtten = half4(dr_LightAtten[index + 0].z, dr_LightAtten[index + 1].z, dr_LightAtten[index + 2].z, dr_LightAtten[index + 3].z);
	half4 lightRange = half4(dr_LightAtten[index + 0].w, dr_LightAtten[index + 1].w, dr_LightAtten[index + 2].w, dr_LightAtten[index + 3].w);
	//Calculate Attenuation
	half4 angularAtten = computeAngularAttenuation(index, light0Dir, light1Dir, light2Dir, light3Dir);
	half4 atten = computeAttenuation(len2, lightAtten, lightRange) * angularAtten;

	float4 nDotL = float4(lightModel(dot(nrm, light0Dir)), lightModel(dot(nrm, light1Dir)), lightModel(dot(nrm, light2Dir)), lightModel(dot(nrm, light3Dir)));
	color += dr_LightColor[index + 0] * atten.x * nDotL.x;
	color += dr_LightColor[index + 1] * atten.y * nDotL.y;
	color += dr_LightColor[index + 2] * atten.z * nDotL.z;
	color += dr_LightColor[index + 3] * atten.w * nDotL.w;
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

void computeLightingParticle(in float3 viewPos, in float3 nrm, inout float3 color)
{
	computeLightSetParticle(0, viewPos, nrm, color);
#if defined(MAX_LIGHT_COUNT_8)
	computeLightSetParticle(4, viewPos, nrm, color);
#endif
}

void computeLightingLocal(in float3 viewPos, in float3 nrm, inout float3 color)
{
	computeLightSetLocal(0, viewPos, nrm, color);
#if defined(MAX_LIGHT_COUNT_8)
	computeLightSetLocal(4, viewPos, nrm, color);
#endif
}

void computeAmbientLocal(in float3 viewPos, in float3 nrm, inout float3 color)
{
#if defined(DYNAMIC_AMBIENT)
	//get the world UP vector in view space.
	float3 u = float3(UNITY_MATRIX_V[0].y, UNITY_MATRIX_V[1].y, UNITY_MATRIX_V[2].y);

	float wu = saturate(0.5*dot(u, nrm) + 0.5);
	float wd = saturate(0.5*dot(-u, nrm) + 0.5);
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
#if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP) && defined(PARTICLE_RENDERING)
	fixed4 computeDiffuseParticle(v2f i)
	{
		half4 texDiffuse = tex2D(_MainTex, i.uv.xy);						//2 ops + fetch latency
		texDiffuse *= _BaseColor;
		half emissive = _Emissive * texDiffuse.a;

		//Approximate Diffuse [18 ALU ops]
		half3 diffuse = i.color.xyz;
		
		//--------------//
		// Translucency //
		//--------------//
		diffuse.rgb = lerp(diffuse.rgb*texDiffuse.rgb, texDiffuse.rgb, emissive);
		#if defined(_ALPHATEST_ON)
			clip( texDiffuse.a - 0.5 );
		#endif
		#if defined(_ALPHAPREMULTIPLY_ON)
			diffuse.rgb *= texDiffuse.a;
		#endif
		
		return fixed4(diffuse, texDiffuse.a);
	}
#endif

#if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP) && !defined(PARTICLE_RENDERING)
	fixed4 computeDiffuse(v2f i)
	{
		half4 texDiffuse = tex2D(_MainTex, i.uv.xy);						//2 ops + fetch latency
		half  emissive   = _Emissive * texDiffuse.a;
		texDiffuse *= _BaseColor;

		#if defined(DETAILMAP)
			texDiffuse.rgb *= tex2D(_NormalTex, i.uv.zw).rgb;
		#endif

		half aoDiffuse = 1.0;
		#if defined(AMBIENT_OCCLUSION)
			float aoBase = tex2D(_NormalTex, i.uv.zw).a;
			aoDiffuse = _AmbientOcclusionDiffuse*aoBase + 1.0 - _AmbientOcclusionDiffuse;
		#endif

		//Approximate Diffuse [18 ALU ops]
		#if defined(NORMALMAP)
			//Tangent Space Per-Pixel Normal
			half3 tsTextureNrm = UnpackNormal(tex2D(_NormalTex, i.uv.zw));	//3 ops + fetch latency
			#ifndef NORMALMAP_FLIP_Y
				tsTextureNrm.y = -tsTextureNrm.y;
			#endif

			half3 tsNormal = normalize(tsTextureNrm);					//7 ops
			half3 diffuse  = i.color0.xyz * dot(c_basis0, tsNormal);
			      diffuse += i.color1.xyz * dot(c_basis1, tsNormal);
			      diffuse += i.color2.xyz * dot(c_basis2, tsNormal);
		#else
			half3 diffuse = i.color.xyz;
		#endif

		half shadow = 1.0;
		#if defined(SHADOWS)
			#if defined(SHDTYPE_PROJECTED)
				shadow = computeShadow(i.shadowUV0, _Shadowmap0);
			#else
				shadow = tex2Dproj(_Shadowmap0, i.shadowUV0).r;
			#endif
			diffuse *= shadow;
		#endif

		//[10/7 ALU ops; mul x 6, mov x 1]
		#if defined(LIGHTMAP)
			half3 lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)).rgb * _StaticLightingScale;
			diffuse = (diffuse + lightmap*shadow)*texDiffuse.rgb;
		#else
			diffuse = diffuse*texDiffuse.rgb;
		#endif
		diffuse.rgb *= aoDiffuse;

		//--------------//
		// Translucency //
		//--------------//
		diffuse.rgb = lerp(diffuse.rgb, texDiffuse.rgb, emissive);
		#if defined(_ALPHATEST_ON)
			clip( texDiffuse.a - 0.5 );
		#endif
		#if defined(_ALPHAPREMULTIPLY_ON)
			diffuse.rgb *= texDiffuse.a;
		#endif

		return fixed4(diffuse, texDiffuse.a);
	}
#endif

////////////////////////////////////////////////////////
//Distance Fog: 15 - 24 ALU ops
//Height Fog: +14 ALU ops
//Sphere Fog: +36 ALU ops
//Total fog cost (approx) in ALU ops:
//   Base 15 - 24
//      +Height Fog 29 - 38 (if not using SphereFog - this may be 2 ops cheaper since dir.xy isn't needed, 27-36)
//		+Sphere Fog 51 - 60
//		+Height +Sphere 65 - 74
////////////////////////////////////////////////////////
// Conclusions: Height fog is a reasonable extra cost Sphere fog is most likely
//				too expensive  in many cases (especially when using both height & sphere fog)
////////////////////////////////////////////////////////
half4 computeFog(half3 posWorld)
{
	half4 fogValue;
	half3 relPos = posWorld.xyz - _WorldSpaceCameraPos.xyz;	//3
	half  sceneDepth = length(relPos);						//10
		
	#if defined(_FOG_HEIGHT)
		half3 dir = relPos / sceneDepth;						//4 (or 2 if dir.xy is unused later)

		//Volume height
		half dH = _fogHeight.y; //dH = maxY - minY
		//Ray/Volume intersections (y0,y1) scaled to the (-inf, 1] range.
		//Note that the ray is clipped to the top of the volume but allowed to pass through the bottom.
		//[To clip to the volume bottom as well, change min(y, 1) to saturate(y) for both y0 and y1]
		half y0 = min(_WorldSpaceCameraPos.y - _fogHeight.x, dH);	//2
		half y1 = min(posWorld.y - _fogHeight.x, dH);				//2
		//Magnitude of the difference in Y between intersections scaled by the fog "thickness"
		half dh = abs(y0 - y1)*_fogHeight.z;						//3

		//compute the length of the ray passing through the volume bounding by [_fogHeight.x, _fogHeight.y] with a thickness of [_fogHeight.z]
		//the ray is clipped by the height bounds, this handles the camera being inside or outside of the volume
		//Note that the thickness artificially scales the length of the ray in order to simulate thicker height fog.
		//--------------------------------------------------------------------------------------------------------------------------------------
		//If the ray is not clipped by the volume, the camera is inside and the "thickness" is 1.0 - then this is equivalent to depth based fog.
		half L = dh / abs(dir.y);									//3
	#else
		//the camera is considered to be inside an infinite volume of fog so only the distance from the camera to the surface is important.
		half L = sceneDepth;
	#endif

	half Lw;
	if (_fogMode == 0.0)
	{
		Lw = saturate(L*_fogLinear.x + _fogLinear.y);	//2
		fogValue.w = Lw;
	}
	else if (_fogMode == 1.0)
	{
		Lw = L*_fogHeight.w;							//8
		fogValue.w = 1.0 - exp(-Lw);
	}
	else if (_fogMode == 2.0)
	{
		Lw = L*_fogHeight.w;							//9
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
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv.xy = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
	o.uv.zw = v.uv * _NormalTex_ST.xy + _NormalTex_ST.zw;

	#if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP) && defined(PARTICLE_RENDERING)
		//View space position of the vertex
		float3 viewPos = mul(UNITY_MATRIX_MV, v.vertex).xyz;
		//Build the View Space to Tangent Space transform.
		float3 nrm = normalize(mul(UNITY_MATRIX_MV, float4(v.nrm.xyz, 0)).xyz);
		
		#if defined(DISABLE_LIGHTING)
			o.color.xyz = 1.0;
		#else
			o.color.xyz = 0.0;
			computeLightingParticle(viewPos, nrm, o.color.xyz);
			computeAmbientLocal(viewPos,  nrm, o.color.xyz);
		#endif
	#elif !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP)
		//View space position of the vertex
		float3 viewPos = mul(UNITY_MATRIX_MV, v.vertex).xyz;

		#if defined(SHADOWS) || defined(_DAYDREAM_FOG) || defined(_FOG_HEIGHT)
			half3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
		#endif

		//Build the Word Space to Tangent Space transform.
		#if defined(NORMALMAP)
			#if defined(DISABLE_LIGHTING)
				// add vertex baked lighting
				#if defined(VERTEX_LIGHTING)
					o.color0.xyz = v.uv1.xyz * _StaticLightingScale;
					o.color1.xyz = v.uv2.xyz * _StaticLightingScale;
					o.color2.xyz = v.uv3.xyz * _StaticLightingScale;
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
			#endif
		#else
			#if defined(DISABLE_LIGHTING)
				// add vertex baked lighting
				#if defined(VERTEX_LIGHTING)
					o.color.xyz = (v.uv1.xyz + v.uv2.xyz + v.uv3.xyz) * 0.333333 * _StaticLightingScale;
				#elif LIGHTMAP
					o.color.xyz = 0.0;
				#else
					o.color.xyz = 1.0;
				#endif
			#else
				float3 nrm = normalize(mul(UNITY_MATRIX_MV, float4(v.nrm.xyz, 0)).xyz);
				o.color.xyz = 0.0;
				computeLightingLocal(viewPos, nrm, o.color.xyz);
				computeAmbientLocal(viewPos, nrm, o.color.xyz);
				#if defined(VERTEX_LIGHTING)
					o.color.xyz = (v.uv1.xyz + v.uv2.xyz + v.uv3.xyz) * 0.333333 * _StaticLightingScale;
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
	#if !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP) && defined(PARTICLE_RENDERING)
		return computeDiffuseParticle(i);
	#elif !defined(BUILD_SHADOWMAP) && !defined(REC_SHADOWMAP)
		half4 outColor;

		outColor = computeDiffuse(i);

		//Fog
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
	#elif (BUILD_SHADOWMAP)	//Shadow map
		const half depthScale = 1.0 / 32.0;
		//convert to per-pixel z, scaled so that z = 32.0 maps to 1.0
		half wScaled = i.depth * depthScale;
		return half4(wScaled, 1.0, 1.0, 1.0);
	#endif
}
