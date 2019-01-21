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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace DaydreamElements.ArmModels {

  /// Used to render a stereoscopic mirror.
  /// Based on _PlanarReflection.cs_ included with Unity Standard Assets.
  [RequireComponent(typeof(MeshRenderer))]
  public class Mirror : MonoBehaviour {
    public LayerMask cullingMask;
    public float clipPlaneOffset = 0.02f;

    private MeshRenderer meshRenderer;

    private Camera mirrorCamera;
    private RenderTexture mirrorTexture;
    private Material mirrorMaterial;

    private int textureWidth;
    private int textureHeight;

    private int leftTextureId;
    private int rightTextureId;

    private Camera cachedMainCamera;
    private OnPreRenderCallback renderCallback;

    private const string MIRROR_CAMERA_NAME = "MirrorCamera";
    private const string SHADER_PROP_LEFT_EYE = "_LeftEyeTex";
    private const string SHADER_PROP_RIGHT_EYE = "_RightEyeTex";

    void Awake() {
      Setup();
    }

    void LateUpdate() {
      Camera mainCamera = Camera.main;

      if (mainCamera == null) {
        if (renderCallback != null) {
          Destroy(renderCallback);
        }
        return;
      }

      if (cachedMainCamera != mainCamera) {
        if (renderCallback != null) {
          Destroy(renderCallback);
        }

        cachedMainCamera = mainCamera;

        renderCallback =
        cachedMainCamera.gameObject.AddComponent<OnPreRenderCallback>();
        renderCallback.OnPreRenderCamera += OnPreRenderCamera;
      }
    }

    void OnBecameVisible() {
      enabled = true;
    }

    void OnBecameInvisible() {
      enabled = false;
    }

    private void OnPreRenderCamera(Camera currentCamera) {
      if (!enabled) {
        return;
      }

      UpdateCameraSettings(currentCamera);

      Camera.MonoOrStereoscopicEye activeEye = currentCamera.stereoActiveEye;

      if (activeEye == Camera.MonoOrStereoscopicEye.Left ||
        activeEye == Camera.MonoOrStereoscopicEye.Mono) {
        RenderReflectionForEye(leftTextureId, currentCamera);
      } else if (activeEye == Camera.MonoOrStereoscopicEye.Right) {
        RenderReflectionForEye(rightTextureId, currentCamera);
      }
    }

    private void RenderReflectionForEye(int textureId, Camera cam) {
      Vector3 pos = transform.position;
      Vector3 normal = transform.up;
      float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
      Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

      Matrix4x4 reflection = CalculateReflectionMatrix(reflectionPlane);
      Vector3 newPos = reflection.MultiplyPoint(cam.transform.position);

      Matrix4x4 mirrorMatrix = cam.worldToCameraMatrix * reflection;
      mirrorCamera.worldToCameraMatrix = mirrorMatrix;

      Vector4 clipPlane = CameraSpacePlane(mirrorCamera, pos, normal, 1.0f);

      Matrix4x4 projection = cam.CalculateObliqueMatrix(clipPlane);
      mirrorCamera.projectionMatrix = projection;

      mirrorCamera.transform.position = newPos;
      Vector3 euler = cam.transform.eulerAngles;
      mirrorCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
      mirrorCamera.targetTexture = mirrorTexture;
      GL.invertCulling = true;
      mirrorCamera.Render();
      GL.invertCulling = false;
      mirrorCamera.targetTexture = null;

      mirrorMaterial.SetTexture(textureId, mirrorTexture);
    }

    private void UpdateCameraSettings(Camera cam) {
      mirrorCamera.cullingMask = cullingMask;
      mirrorCamera.backgroundColor = cam.backgroundColor;
      mirrorCamera.aspect = cam.aspect;
      mirrorCamera.clearFlags = cam.clearFlags;
      mirrorCamera.orthographic = cam.orthographic;
      mirrorCamera.farClipPlane = cam.farClipPlane;
      mirrorCamera.nearClipPlane = cam.nearClipPlane;
      mirrorCamera.transform.position = cam.transform.position;
      mirrorCamera.transform.rotation = cam.transform.rotation;
    }

    private void Setup() {
      if (UnityEngine.XR.XRDevice.isPresent) {
        textureWidth = UnityEngine.XR.XRSettings.eyeTextureWidth;
        textureHeight = UnityEngine.XR.XRSettings.eyeTextureHeight;
      } else {
        textureWidth = Screen.width;
        textureHeight = Screen.height;
      }

      leftTextureId = Shader.PropertyToID(SHADER_PROP_LEFT_EYE);
      rightTextureId = Shader.PropertyToID(SHADER_PROP_RIGHT_EYE);

      CreateMirrorCamera();
      CreateMirrorTexture();

      meshRenderer = GetComponent<MeshRenderer>();
      mirrorMaterial = meshRenderer.material;
    }

    private void CreateMirrorCamera() {
      if (mirrorCamera != null) {
        return;
      }

      GameObject cameraObj = new GameObject(MIRROR_CAMERA_NAME);
      cameraObj.transform.SetParent(transform, false);
      mirrorCamera = cameraObj.AddComponent<Camera>();
      mirrorCamera.enabled = false;
      mirrorCamera.stereoTargetEye = StereoTargetEyeMask.None;
    }

    private void CreateMirrorTexture() {
      mirrorTexture = new RenderTexture(textureWidth, textureHeight, 16);
      mirrorTexture.isPowerOfTwo = true;
      mirrorTexture.antiAliasing = QualitySettings.antiAliasing;
      mirrorTexture.Create();
    }

    private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane) {
      Matrix4x4 reflectionMat = Matrix4x4.zero;

      reflectionMat.m00 = (1.0f - 2.0f * plane[0] * plane[0]);
      reflectionMat.m01 = (-2.0f * plane[0] * plane[1]);
      reflectionMat.m02 = (-2.0f * plane[0] * plane[2]);
      reflectionMat.m03 = (-2.0f * plane[3] * plane[0]);

      reflectionMat.m10 = (-2.0f * plane[1] * plane[0]);
      reflectionMat.m11 = (1.0f - 2.0f * plane[1] * plane[1]);
      reflectionMat.m12 = (-2.0f * plane[1] * plane[2]);
      reflectionMat.m13 = (-2.0f * plane[3] * plane[1]);

      reflectionMat.m20 = (-2.0f * plane[2] * plane[0]);
      reflectionMat.m21 = (-2.0f * plane[2] * plane[1]);
      reflectionMat.m22 = (1.0f - 2.0f * plane[2] * plane[2]);
      reflectionMat.m23 = (-2.0f * plane[3] * plane[2]);

      reflectionMat.m30 = 0.0f;
      reflectionMat.m31 = 0.0f;
      reflectionMat.m32 = 0.0f;
      reflectionMat.m33 = 1.0f;

      return reflectionMat;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal,
                                   float sideSign) {
      Vector3 offsetPos = pos + normal * clipPlaneOffset;
      Matrix4x4 m = cam.worldToCameraMatrix;
      Vector3 cpos = m.MultiplyPoint(offsetPos);
      Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

      return new Vector4(cnormal.x, cnormal.y, cnormal.z,
        -Vector3.Dot(cpos, cnormal));
    }
  }
}