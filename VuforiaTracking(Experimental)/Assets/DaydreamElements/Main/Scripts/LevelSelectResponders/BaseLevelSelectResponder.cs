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
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace DaydreamElements.Main {

  /// Used to respond to evens from the LevelSelectController.
  /// This is used so that demos can correctly handle the state of the
  /// level select controller based on the interactions in the demo.
  public abstract class BaseLevelSelectResponder : MonoBehaviour {
    public abstract void OnMenuOpened();

    public abstract void OnMenuClosed();

    protected virtual void Awake() {
      Assert.IsNotNull(LevelSelectController.Instance);
      LevelSelectController.Instance.OnStateChanged += OnStateChanged;

      Camera levelSelectCamera = LevelSelectController.Instance.LevelSelectCamera;
      int layer = LevelSelectController.LevelSelectLayer;
      foreach (Camera cam in Camera.allCameras) {
        if (cam != levelSelectCamera) {
          cam.cullingMask &= ~(1 << layer);
        }
      }
    }

    protected virtual void OnDestroy() {
      if (LevelSelectController.Instance != null) {
        LevelSelectController.Instance.OnStateChanged -= OnStateChanged;
      }
    }

    private void OnStateChanged(LevelSelectController.MenuState currentState,
                                LevelSelectController.MenuState previousState) {
      switch (currentState) {
        case LevelSelectController.MenuState.Opening:
          OnMenuOpened();
          break;
        case LevelSelectController.MenuState.Closed:
          OnMenuClosed();
          break;
        default:
          break;
      }
    }
  }
}
