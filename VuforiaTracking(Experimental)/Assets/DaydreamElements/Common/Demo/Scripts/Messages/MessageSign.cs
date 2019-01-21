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

namespace DaydreamElements.Common {

  public class MessageSign : MonoBehaviour {
    public GameObject farMessageObject;
    public GameObject closeMessageObject;

    public float distanceThreshold = 5.0f;

    public float closeObjectTransitionSpeed = 8.0f;

    public float farObjectTransitionSpeed = 6.0f;

    public float fadeTransitionSpeed = 4.0f;

    private bool isClose;
    private float currentCloseAlpha;
    private MaterialPropertyBlock materialPropertyBlock;
    private int colorId;
    private Renderer[] closeRenderers;

    private const float DEPART_SIGN_THRESHOLD_SCALE = 1.05f;

    void Start() {
      DetectIsClose();

      closeRenderers = closeMessageObject.GetComponentsInChildren<Renderer>();
      materialPropertyBlock = new MaterialPropertyBlock();
      colorId = Shader.PropertyToID("_Color");

      UpdateObjects(true);
    }

    void Update() {
      DetectIsClose();
      UpdateObjects(false);
    }

    private void UpdateObjects(bool immediate) {
      float closeTargetScale = isClose ? 1.0f : 0.5f;
      float farTargetScale = isClose ? 0.0f : 1.0f;

      Vector3 targetCloseScale = new Vector3(closeTargetScale, closeTargetScale, closeTargetScale);
      Vector3 targetFarScale = new Vector3(farTargetScale, farTargetScale, farTargetScale);

      if (immediate) {
        farMessageObject.transform.localScale = targetFarScale;
        closeMessageObject.transform.localScale = targetCloseScale;
      } else {
        farMessageObject.transform.localScale = Vector3.Lerp(farMessageObject.transform.localScale, targetFarScale, farObjectTransitionSpeed * Time.deltaTime);
        closeMessageObject.transform.localScale = Vector3.Lerp(closeMessageObject.transform.localScale, targetCloseScale, closeObjectTransitionSpeed * Time.deltaTime);
      }

      float closeTargetAlpha = isClose ? 1.0f : 0.0f;
      if (immediate) {
        currentCloseAlpha = closeTargetAlpha;
      } else {
        currentCloseAlpha = Mathf.Lerp(currentCloseAlpha, closeTargetAlpha, fadeTransitionSpeed * Time.deltaTime);
      }

      for (int i = 0; i < closeRenderers.Length; i++) {
        Renderer closeRenderer = closeRenderers[i];
        Color color = closeRenderer.sharedMaterial.color;
        color.a = currentCloseAlpha * color.a;
        closeRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor(colorId, color);
        closeRenderer.SetPropertyBlock(materialPropertyBlock);
      }
    }

    private void DetectIsClose() {
      float distanceFromCamera = DistanceFromCamera();

      if (!isClose && distanceFromCamera < distanceThreshold) {
        isClose = true;
        closeMessageObject.transform.LookAt(Camera.main.transform, Vector3.up);
      } else if (isClose && distanceFromCamera > (distanceThreshold * DEPART_SIGN_THRESHOLD_SCALE)) {
        isClose = false;
      }
    }

    private float DistanceFromCamera() {
      Vector3 cameraPos = Camera.main.transform.position;
      Vector3 diff = transform.position - cameraPos;

      return diff.magnitude;
    }
  }
}