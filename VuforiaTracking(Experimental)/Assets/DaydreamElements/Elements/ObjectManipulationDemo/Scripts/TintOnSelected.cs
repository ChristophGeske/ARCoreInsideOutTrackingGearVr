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

  /// A simple color response example. Interactive objects
  /// are tinted with a solid color while selected.
  public class TintOnSelected : MonoBehaviour {
    /// The renderer used by this object.
    [Tooltip("The renderer used by this object.")]
    public Renderer rendererCmp;

    /// The interactive object to receive a tinting effect.
    [Tooltip("The interactive object to receive a tinting effect.")]
    public BaseInteractiveObject target;

    /// Color tint to be applied to the object when it is not selected.
    [Tooltip("Color tint to be applied to the object when it is not selected.")]
    public Color defaultColor = Color.white;

    /// Color tint to be applied to the object when it is selected.
    [Tooltip("Color tint to be applied to the object when it is selected.")]
    public Color selectedColor = Color.white;

    private MaterialPropertyBlock block;
    private int colorID;
    private BaseInteractiveObject.ObjectState lastState = BaseInteractiveObject.ObjectState.None;

    void Awake() {
      block = new MaterialPropertyBlock();
      colorID = Shader.PropertyToID("_Color");
      block.SetColor(colorID, defaultColor);
    }

    void LateUpdate() {
      if (target == null || rendererCmp == null) {
        return;
      }

      BaseInteractiveObject.ObjectState state = target.State;

      if (state != lastState) {
        lastState = state;
        if (state == BaseInteractiveObject.ObjectState.Selected) {
          block.SetColor(colorID, selectedColor);
          rendererCmp.SetPropertyBlock(block);
        } else {
          block.SetColor(colorID, defaultColor);
          rendererCmp.SetPropertyBlock(block);
        }
      }
    }
  }
}
