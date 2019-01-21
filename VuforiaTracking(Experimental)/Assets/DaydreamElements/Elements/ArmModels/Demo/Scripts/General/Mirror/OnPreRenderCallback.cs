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

using System;
using UnityEngine;

namespace DaydreamElements.ArmModels {

  /// Provides a callback that occurs during OnPreRender for a camera
  /// that other scripts can hook into.
  public class OnPreRenderCallback : MonoBehaviour {
    public event Action<Camera> OnPreRenderCamera;

    void OnPreRender() {
      Camera currentCamera = Camera.current;

      if (currentCamera == null) {
        return;
      }

      if (OnPreRenderCamera != null) {
        OnPreRenderCamera(currentCamera);
      }
    }
  }
}