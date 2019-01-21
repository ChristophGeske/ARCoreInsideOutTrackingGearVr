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

namespace DaydreamElements.ObjectManipulation {

  /// Highlights objects on hover.
  public class HighlightOnHover : MonoBehaviour {

    [Tooltip("The BaseInteractiveObject used by this script.")]
    public BaseInteractiveObject interactiveObject;

    [Tooltip("The MeshRenderer used by this script.")]
    public MeshRenderer meshRenderer;

    [Tooltip("Highlight transition speed.")]
    public float highlightSpeed = 2.0f;

    private int highlightID;

    private float highlight;
    private float targetHighlight;

    private MaterialPropertyBlock materialPropertyBlock;

    void OnValidate() {
      if (interactiveObject == null) {
        interactiveObject = GetComponent<BaseInteractiveObject>();
      }
      if (meshRenderer == null) {
        meshRenderer = GetComponent<MeshRenderer>();
      }
    }

    void Awake() {
      Reset();
    }

    void Start() {
      materialPropertyBlock = new MaterialPropertyBlock();
      highlightID = Shader.PropertyToID("_Highlight");
    }

    void Update() {
      if (!ObjectManipulationPointer.IsObjectSelected() && interactiveObject.Hover) {
        targetHighlight = 1f;
      } else {
        targetHighlight = 0f;
      }

      highlight = Mathf.Lerp(highlight, targetHighlight, Time.deltaTime * highlightSpeed);
      materialPropertyBlock.SetFloat(highlightID, highlight);
      meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void Reset() {
      highlight = 0f;
      targetHighlight = 0f;
    }
  }
}
