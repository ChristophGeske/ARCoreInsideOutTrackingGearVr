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

Shader "Daydream/Standard"
{
	Properties
	{
		//NOTE: Unity Material Property Drawers have strange limitations when passing custom parameters - strings ARE allowed, though quotes cause compilation to fail.
		//Because quotes cause compilation to fail, punctuation, such as ,;:- etc. also causes compilation to fail. So tooltips must be written without any punctuation.
		[TextureProperty_TT_transform(Material color texture rgb controls color and alpha controls opacity or emissive or specular masks)]
		_MainTex("Texture", 2D) = "white" {}

		[TextureProperty_TT_transform(Optional tangent space normal map or detail map)]
		_NormalTex("Normal/Detail", 2D) = "bump" {}

		[ColorProperty_TT(Base material color which is multiplied with the material color texture to get the final prelit color)]
		_BaseColor("BaseColor", Color) = (1.0, 1.0, 1.0, 1.0)

		[RangeProperty_TT(Emissive factor ranging from normally lit to fullbright and not lit  Note that the emissive factor is multiplied with the material color Texture ALPHA)]
		_Emissive("Emissive", Range(0.0,1.0)) = 0.0

		[RangeProperty_TT(How much the lighting wraps around the object where 0 is standard and 1 is fully wrapping around)]
		_Wrap("Wrap", Range(0.0,1.0)) = 0.0

		[RangeProperty_TT(The baked lighting scale where 0 is off and 1 is normal)]
		_StaticLightingScale("Baked Lighting Scale", Range(0.0,4.0)) = 1.0

		[RangeProperty_TT(Material smoothness ranging from rough to smooth  Smooth surfaces have sharp specular highlights and rough surfaces very broad and matte looking highlights)]
		_SpecSmoothness("Smoothness", Range(0.0,1.0)) = 0.5

		[RangeProperty_TT(Intensity of dynamic shadows affecting the surface ranging from invisible to dark)]
		_ShadowIntens("Shadow Intensity", Range(0.0,1.0)) = 0.5

		[RangeProperty_TT(Controls how quickly the shadow is attenuated based on the distance from the shadow caster)]
		_ShadowFalloff("Shadow Falloff", Range(0.03125,1.0)) = 0.25

		[RangeProperty_TT(Controls ambient occlusion diffuse intensity)]
		_AmbientOcclusionDiffuse("AO Diffuse", Range(0.0,1.0)) = 1.00

		[RangeProperty_TT(Controls ambient occlusion specular intensity)]
		_AmbientOcclusionSpec("AO Specular", Range(0.0,1.0)) = 0.00

		// Blending state
		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "VertexLM" }
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma only_renderers d3d11 glcore gles gles3
			
			//variants
			//global keywords
			#pragma multi_compile __ BUILD_SHADOWMAP REC_SHADOWMAP SHDTYPE_PROJECTED SHDTYPE_MASK
			#pragma multi_compile __ _DAYDREAM_FOG _FOG_HEIGHT
			#pragma multi_compile __ _USE_ENLIGHTEN

			//alpha
			#pragma shader_feature __ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

			//Sets - these should be multi_compile but that method does not work consistently on Android
			//Lighting [Max light count 0, 4 (default), 8]
			#pragma shader_feature __ DISABLE_LIGHTING MAX_LIGHT_COUNT_8
			#pragma shader_feature ATTEN_CLAMP
			#pragma shader_feature AMBIENT_OCCLUSION
			//Ambient
			#pragma shader_feature DYNAMIC_AMBIENT
			//Specular approximation
			#pragma shader_feature SPECULAR
			//Specular Power
			#pragma shader_feature __ SPECULAR_FIXED_LOW SPECULAR_FIXED_MED

			//Individual toggles
			//Enable colored specular lighting
			#pragma shader_feature SPECULAR_COLORED
			//Enable specular antialiasing
			#pragma shader_feature SPECULAR_AA
			//Shadows
			#pragma shader_feature SHADOWS
			//Normal / Detail mapping
			#pragma shader_feature __ NORMALMAP DETAILMAP
			#pragma shader_feature NORMALMAP_FLIP_Y
			//Unity Lightmaps or Daydream vertex baked lighitng
			#pragma shader_feature STATIC_LIGHTING
			#pragma shader_feature __ VERTEX_LIGHTING LIGHTMAP

			#include "daydreamStandardInclude.cginc"

			v2f vert(appdata v)
			{
				return daydreamVertex(v);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return daydreamFragment(i);
			}
			ENDCG
		}

		Pass
		{
			Tags{ "LightMode" = "VertexLMRGBM" }
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma only_renderers d3d11 glcore gles gles3

			//variants
			//global keywords
			#pragma multi_compile __ BUILD_SHADOWMAP REC_SHADOWMAP SHDTYPE_PROJECTED SHDTYPE_MASK
			#pragma multi_compile __ _DAYDREAM_FOG _FOG_HEIGHT
			#pragma multi_compile __ _USE_ENLIGHTEN

			//alpha
			#pragma shader_feature __ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

			//Sets - these should be multi_compile but that method does not work consistently on Android
			//Lighting [Max light count 0, 4 (default), 8]
			#pragma shader_feature __ DISABLE_LIGHTING MAX_LIGHT_COUNT_8
			#pragma shader_feature ATTEN_CLAMP
			#pragma shader_feature AMBIENT_OCCLUSION
			//Ambient
			#pragma shader_feature DYNAMIC_AMBIENT
			//Specular approximation
			#pragma shader_feature SPECULAR
			//Specular Power
			#pragma shader_feature __ SPECULAR_FIXED_LOW SPECULAR_FIXED_MED

			//Individual toggles
			//Enable colored specular lighting
			#pragma shader_feature SPECULAR_COLORED
			//Enable specular antialiasing
			#pragma shader_feature SPECULAR_AA
			//Shadows
			#pragma shader_feature SHADOWS
			//Normalmapping
			#pragma shader_feature __ NORMALMAP DETAILMAP
			#pragma shader_feature NORMALMAP_FLIP_Y
			//Unity Lightmaps or Daydream vertex baked lighitng
			#pragma shader_feature STATIC_LIGHTING
			#pragma shader_feature __ VERTEX_LIGHTING LIGHTMAP

			#include "daydreamStandardInclude.cginc"

			v2f vert(appdata v)
			{
				return daydreamVertex(v);
			}
					
			fixed4 frag(v2f i) : SV_Target
			{
				return daydreamFragment(i);
			}
			ENDCG
		}

		Pass
		{
			Tags{ "LightMode" = "Vertex" }
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma only_renderers d3d11 glcore gles gles3

			//variants
			//global keywords
			#pragma multi_compile __ BUILD_SHADOWMAP REC_SHADOWMAP SHDTYPE_PROJECTED SHDTYPE_MASK
			#pragma multi_compile __ _DAYDREAM_FOG _FOG_HEIGHT
			#pragma multi_compile __ _USE_ENLIGHTEN

			//alpha
			#pragma shader_feature __ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			
			//Sets - these should be multi_compile but that method does not work consistently on Android
			//Lighting [Max light count 0, 4 (default), 8]
			#pragma shader_feature __ DISABLE_LIGHTING MAX_LIGHT_COUNT_8
			#pragma shader_feature ATTEN_CLAMP
			#pragma shader_feature AMBIENT_OCCLUSION
			//Ambient
			#pragma shader_feature DYNAMIC_AMBIENT
			//Specular approximation
			#pragma shader_feature SPECULAR
			//Specular Power
			#pragma shader_feature __ SPECULAR_FIXED_LOW SPECULAR_FIXED_MED

			//Individual toggles
			//Enable colored specular lighting
			#pragma shader_feature SPECULAR_COLORED
			//Enable specular antialiasing
			#pragma shader_feature SPECULAR_AA
			//Shadows
			#pragma shader_feature SHADOWS
			//Normalmapping
			#pragma shader_feature __ NORMALMAP DETAILMAP
			#pragma shader_feature NORMALMAP_FLIP_Y
			//No lightmaps with this mode (i.e. if an object is not flagged as "static")
			//Unity Lightmaps or Daydream vertex baked lighitng
			#pragma shader_feature STATIC_LIGHTING
			#pragma shader_feature __ VERTEX_LIGHTING LIGHTMAP

			#include "daydreamStandardInclude.cginc"

			v2f vert(appdata v)
			{
				return daydreamVertex(v);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return daydreamFragment(i);
			}
			ENDCG
		}
	}

	// properties and subshaders here...
	CustomEditor "DaydreamStandardUI"
}
