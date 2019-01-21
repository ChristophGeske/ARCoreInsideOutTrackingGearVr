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

using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

/// Camera component used for rendering exponential shadow maps.
public class ShadowRendering : MonoBehaviour {
  /// Whether or not to render static shadows.
  /// If false, all shadows will be dynamic.
  public bool renderStaticShadows = true;

  /// Layer ID for dynamic shadow casters, if using mixed static
  /// and dynamic shadows.
  public int dynamicShadowCasterLayerID = 4;

  /// Changing this value adjusts shadow softness.
  public float shadowExponent;

  /// Set this to a similar value to the shadow camera's far clip plane.
  public float shadowDistance;

  /// Increasing blur iterations will result in softer, more expensive shadows.
  public int staticBlurIterations = 10;
  public int dynamicBlurIterations = 1;

  // This should be a default Unity quad.
  public Mesh quad;

  // Camera to be used for shadow casting.
  [SerializeField]
  private Camera camera;
  [SerializeField]
  private Shader depthShader;

  [SerializeField]
  private Material blurMaterial;
  [SerializeField]
  private Material combineBlurMaterial;
  [SerializeField]
  private Material blitMat;

  private RenderTexture shadowTexture;

  // Render texture used for static shadow pass.
  private RenderTexture staticShadowTexture;

  // Render textures used in shadow compositing ops.
  private RenderTexture shadowTextureA;
  private RenderTexture shadowTextureB;

  private RenderTexture shadowTextureDisplay;

  private const string SHADOW_CAMERA_MATRIX_NAME = "_ShadowCameraMatrix";
  private const string SHADOW_MATRIX_NAME = "_ShadowMatrix";
  private const string SHADOW_TEXTURE_NAME = "_ShadowTexture";
  private const string SHADOW_DATA_NAME = "_ShadowData";

  private int shadowCameraMatrixId;
  private int shadowMatrixId;
  private int shadowTextureId;
  private int shadowDataId;

  private int renderRes = 1024;
  private int texRes = 512;
  private Vector4 shadowData = new Vector4();
  private CommandBuffer initMips;
  private CommandBuffer mips;

  private bool initialized = false;

  // The background color for the shadow camera.
  private Color almostWhite = new Color(0.99f,0.99f,0.99f,0.99f);

  void OnEnable() {
    if (renderStaticShadows == true) {
      staticShadowTexture = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.ARGB32);
    }

    shadowTexture = new RenderTexture(renderRes, renderRes, 16, RenderTextureFormat.ARGB32);
    shadowTextureA = new RenderTexture(texRes, texRes,0, RenderTextureFormat.ARGB32);
    shadowTextureB = new RenderTexture(texRes, texRes,0, RenderTextureFormat.ARGB32);

    shadowTextureDisplay = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.ARGB32);

    shadowTextureDisplay.useMipMap = true;
    shadowTextureDisplay.autoGenerateMips = false;
    shadowTextureDisplay.Create();

    shadowTexture.Create();
    shadowTextureA.Create();
    shadowTextureB.Create();

    if (renderStaticShadows == true) {
      staticShadowTexture.Create();
    }

    shadowTextureDisplay.DiscardContents();
    shadowTexture.DiscardContents();
    shadowTextureA.DiscardContents();
    shadowTextureB.DiscardContents();

    if (renderStaticShadows == true) {
      staticShadowTexture.DiscardContents();
    }

    camera.clearFlags = CameraClearFlags.SolidColor;
    camera.backgroundColor = almostWhite;
    camera.SetReplacementShader(depthShader,"RenderType");

    shadowCameraMatrixId = Shader.PropertyToID(SHADOW_CAMERA_MATRIX_NAME);
    shadowMatrixId = Shader.PropertyToID(SHADOW_MATRIX_NAME);
    shadowTextureId = Shader.PropertyToID(SHADOW_TEXTURE_NAME);
    shadowDataId = Shader.PropertyToID(SHADOW_DATA_NAME);

    UpdateCameraParams();

    initialized = false;
  }

  void OnDisable() {
    if (renderStaticShadows == true) {
      staticShadowTexture.Release();
    }
    shadowTexture.Release();
    shadowTextureA.Release();
    shadowTextureB.Release();
    shadowTextureDisplay.Release();
  }

  void OnDestroy() {
    if (renderStaticShadows == true) {
      staticShadowTexture.Release();
      Destroy(staticShadowTexture);
    }
    shadowTexture.Release();
    shadowTextureA.Release();
    shadowTextureB.Release();
    shadowTextureDisplay.Release();

    Destroy(shadowTexture);
    Destroy(shadowTextureA);
    Destroy(shadowTextureB);
    Destroy(shadowTextureDisplay);
  }

  void Initialize() {
    if(shadowTexture.IsCreated() &&
      shadowTextureA.IsCreated() &&
      shadowTextureB.IsCreated() &&
      shadowTextureDisplay.IsCreated()) {

      RenderTexture current = RenderTexture.active;

      RenderTexture.active = shadowTexture;
      GL.Clear(true, true, almostWhite, 1);

      RenderTexture.active = shadowTextureA;
      GL.Clear(true, false, almostWhite);

      RenderTexture.active = shadowTextureB;
      GL.Clear(true, false, almostWhite);

      RenderTexture.active = shadowTextureDisplay;
      GL.Clear(true, false, almostWhite);

      if (renderStaticShadows == true) {
        RenderTexture.active = staticShadowTexture;
        GL.Clear(true, false, almostWhite);
      }

      RenderTexture.active = current;

      Shader.SetGlobalTexture(shadowTextureId, shadowTextureDisplay);
      ClearMips();
      GenerateMips();
      Graphics.ExecuteCommandBuffer(initMips);

      camera.targetTexture = shadowTexture;
      camera.enabled = true;
      initialized = true;

      // The static shadow pass is only performed once.
      if (renderStaticShadows == true) {
        RenderStaticLayer();
      }
    }
  }

  void Update() {
    if(!initialized){
      Initialize();
    }
    else{
      UpdateCameraParams();
    }
  }

  void OnPostRender() {
    BlurLastRender();
  }

  void UpdateCameraParams() {
    shadowData.x = shadowDistance;
    shadowData.y = shadowExponent;
    shadowData.z = Mathf.Exp(shadowExponent);
    shadowData.w = Mathf.Exp(-shadowExponent);
    Shader.SetGlobalVector(shadowDataId, shadowData);
    Shader.SetGlobalMatrix(shadowCameraMatrixId, camera.worldToCameraMatrix);
    Shader.SetGlobalMatrix(shadowMatrixId, camera.projectionMatrix * camera.worldToCameraMatrix);
  }

  void RenderStaticLayer() {
    // Exclude the layer from the shadow camera's culling mask that is being
    // used for dynamic shadow casters.
    camera.cullingMask &= ~(1<<dynamicShadowCasterLayerID);

    camera.Render();

    Graphics.Blit(shadowTexture, shadowTextureA, blurMaterial, 0);

    RenderTexture current = shadowTextureA;
    RenderTexture target = shadowTextureB;

    // Blur static shadows.
    for(int i=0; i<staticBlurIterations; i++){
      Graphics.Blit(current, target, blurMaterial, 0);
      RenderTexture tmp = current;
      current = target;
      target = tmp;
    }

    Graphics.Blit(current, staticShadowTexture, blurMaterial, 0);

    combineBlurMaterial.SetTexture("_MainTex2", staticShadowTexture);

    // Set the camera culling mask back to the layer being used for dynamic
    // shadow casters.
    camera.cullingMask = (1<<dynamicShadowCasterLayerID);
  }

  private void BlurLastRender() {
    /// Downsample and blur.

    // Discard render textures.
    shadowTextureA.DiscardContents();
    shadowTextureB.DiscardContents();
    shadowTextureDisplay.DiscardContents();

    if (renderStaticShadows == true) {
      Graphics.Blit(shadowTexture, shadowTextureA, combineBlurMaterial,0);
    } else {
      Graphics.Blit(shadowTexture, shadowTextureA, blurMaterial, 0);
    }

    RenderTexture current = shadowTextureA;
    RenderTexture target = shadowTextureB;

    // Blur dynamic shadows.
    for(int i = 0; i < dynamicBlurIterations; i++){
      Graphics.Blit(current, target, blurMaterial, 0);
      RenderTexture tmp = current;
      current = target;
      target = tmp;
    }

    // Blit the combined static and dynamic shadows to the final render
    // texture for display.
    Graphics.Blit(current, shadowTextureDisplay, blurMaterial, 0);

    blitMat.mainTexture = current;
    Graphics.ExecuteCommandBuffer(mips);
  }

  void GenerateMips() {
    mips = new CommandBuffer();

    Matrix4x4 mat = Matrix4x4.identity;

    // 128
    mips.SetRenderTarget(shadowTextureDisplay, 1);
    mips.DrawMesh(quad, mat, blitMat, 0, 0);

    // 64
    mips.SetRenderTarget(shadowTextureDisplay, 2);
    mips.DrawMesh(quad, mat, blitMat, 0, 0);

    // 32
    mips.SetRenderTarget(shadowTextureDisplay, 3);
    mips.DrawMesh(quad, mat, blitMat, 0, 0);

    // 16
    mips.SetRenderTarget(shadowTextureDisplay, 4);
    mips.DrawMesh(quad, mat, blitMat, 0, 0);
  }

  void ClearMips() {
    initMips = new CommandBuffer();

    Matrix4x4 mat = Matrix4x4.identity;

    initMips.SetRenderTarget(shadowTextureDisplay, 1);
    initMips.ClearRenderTarget(false, true, Color.white);

    initMips.SetRenderTarget(shadowTextureDisplay, 2);
    initMips.ClearRenderTarget(false, true, Color.white);

    initMips.SetRenderTarget(shadowTextureDisplay, 3);
    initMips.ClearRenderTarget(false, true, Color.white);

    initMips.SetRenderTarget(shadowTextureDisplay, 4);
    initMips.ClearRenderTarget(false, true, Color.white);

    initMips.SetRenderTarget(shadowTextureDisplay, 5);
    initMips.ClearRenderTarget(false, true, Color.white);

    initMips.SetRenderTarget(shadowTextureDisplay, 6);
    initMips.ClearRenderTarget(false, true, Color.white);

    initMips.SetRenderTarget(shadowTextureDisplay, 7);
    initMips.ClearRenderTarget(false, true, Color.white);

    initMips.SetRenderTarget(shadowTextureDisplay, 8);
    initMips.ClearRenderTarget(false, true, Color.white);
  }
}