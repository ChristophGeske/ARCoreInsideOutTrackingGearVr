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

namespace DaydreamElements.ArmModels {

  [RequireComponent(typeof(MeshRenderer))]
  public class ProjectileSplat : MonoBehaviour {
    public Color startColor;
    public Color endColor;
    public Vector3 startScale = Vector3.zero;
    public Vector3 endScale = Vector3.one;
    public float durationSeconds = 0.3f;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock materialPropertyBlock;
    private float elapsedSeconds;
    private int colorId;

    void Awake() {
      meshRenderer = GetComponent<MeshRenderer>();
      materialPropertyBlock = new MaterialPropertyBlock();
      colorId = Shader.PropertyToID("_Color");
    }

    void LateUpdate() {
      elapsedSeconds += Time.deltaTime;
      float ratio = Mathf.Clamp01(elapsedSeconds / durationSeconds);
      ratio = EasingFunctions.RatioEaseOutCubic(ratio);

      Color currentColor = Color.Lerp(startColor, endColor, ratio);
      Vector3 currentScale = Vector3.Lerp(startScale, endScale, ratio);

      transform.localScale = currentScale;

      meshRenderer.GetPropertyBlock(materialPropertyBlock);
      materialPropertyBlock.SetColor(colorId, currentColor);
      meshRenderer.SetPropertyBlock(materialPropertyBlock);

      if (ratio == 1.0f) {
        Destroy(gameObject);
      }
    }
  }
}