﻿// Copyright 2017 Google Inc. All rights reserved.
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
  public class FadeControllerVisual : MonoBehaviour, IGvrArmModelReceiver {
    private MaterialPropertyBlock materialPropertyBlock;
    private MeshRenderer meshRenderer;
    private int alphaId;

    public GvrBaseArmModel ArmModel { get; set; }

    void Awake() {
      materialPropertyBlock = new MaterialPropertyBlock();
      meshRenderer = GetComponent<MeshRenderer>();
      alphaId = Shader.PropertyToID("_Alpha");
    }

    void LateUpdate() {
      meshRenderer.GetPropertyBlock(materialPropertyBlock);
      float alpha = 1.0f;
      if (ArmModel != null) {
        alpha = ArmModel.PreferredAlpha;
      }
      materialPropertyBlock.SetFloat(alphaId, alpha);
      meshRenderer.SetPropertyBlock(materialPropertyBlock);
      meshRenderer.enabled = alpha != 0.0f;
    }
  }
}