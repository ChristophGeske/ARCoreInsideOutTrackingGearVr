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

using UnityEngine.Assertions;
using DaydreamElements.ClickMenu;

namespace DaydreamElements.Main {

  /// Used to disable Painter and Click Menu when the navigation menu is open
  public class ClickMenuLevelSelectResponder : BaseLevelSelectResponder {
    private ClickMenuRoot clickMenu;
    private GvrTrackedController controller;
    private GvrLaserPointer pointer;

    public override void OnMenuOpened() {
      SetClickMenuEnabled(false);
    }

    public override void OnMenuClosed() {
      SetClickMenuEnabled(true);
    }

    void Start() {
      clickMenu = SceneHelpers.FindObjectOfType<ClickMenuRoot>(true);
      Assert.IsNotNull(clickMenu);
      controller = SceneHelpers.FindObjectOfType<GvrTrackedController>(true);
      Assert.IsNotNull(controller);
      pointer = controller.GetComponentInChildren<GvrLaserPointer>(true);
      Assert.IsNotNull(pointer);
    }

    private void SetClickMenuEnabled(bool enabled) {
      clickMenu.gameObject.SetActive(enabled);
      controller.gameObject.SetActive(enabled);
      if (enabled) {
        GvrPointerInputModule.Pointer = pointer;
      }
    }
  }
}
