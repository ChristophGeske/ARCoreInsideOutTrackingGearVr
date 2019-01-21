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
using System.Collections;
using DaydreamElements.Teleport;

namespace DaydreamElements.Main {

  public class DaydreamRendererLevelSelectResponder : BaseLevelSelectResponder {
    private GvrTrackedController controller;
    private TeleportController teleport;

    public override void OnMenuOpened() {
      SetTeleportModeEnabled(false);
    }

    public override void OnMenuClosed() {
      SetTeleportModeEnabled(true);
    }

    void Start() {
      controller = SceneHelpers.FindObjectOfType<GvrTrackedController>(true);
      Assert.IsNotNull(controller);
      teleport = SceneHelpers.FindObjectOfType<TeleportController>(true);
      Assert.IsNotNull(teleport);
    }

    void Update() {
      if (teleport == null) {
        return;
      }

      if (LevelSelectController.Instance == null) {
        return;
      }

      bool isTeleporting = teleport.IsSelectingTeleportLocation || teleport.IsTeleporting;
      LevelSelectController.Instance.enabled = !isTeleporting;
    }

    private void SetTeleportModeEnabled(bool enabled) {
      controller.gameObject.SetActive(enabled);

      if (teleport != null) {
        teleport.gameObject.SetActive(enabled);
      }
    }
  }
}
