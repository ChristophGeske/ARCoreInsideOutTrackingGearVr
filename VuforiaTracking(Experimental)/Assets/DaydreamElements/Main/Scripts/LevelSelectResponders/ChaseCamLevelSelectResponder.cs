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
using DaydreamElements.Chase;

namespace DaydreamElements.Main {

  /// Stops the character from moving when the navigation menu is open
  /// in the ChaseCam demo scene, and disables chase cam to stop triggers.
  public class ChaseCamLevelSelectResponder : BaseLevelSelectResponder {
    private BasePositionedCharacter positionedCharacter;
    private GvrTrackedController controller;
    private CharacterPositionPointer pointer;
    private ChaseCam chaseCam;

    public override void OnMenuOpened() {
      positionedCharacter.StopMoving();
      controller.gameObject.SetActive(false);
      chaseCam.enabled = false;
    }

    public override void OnMenuClosed() {
      controller.gameObject.SetActive(true);
      GvrPointerInputModule.Pointer = pointer;
      chaseCam.enabled = true;
    }

    protected override void Awake() {
      base.Awake();
      positionedCharacter = SceneHelpers.FindObjectOfType<BasePositionedCharacter>(true);
      Assert.IsNotNull(positionedCharacter);

      controller = SceneHelpers.FindObjectOfType<GvrTrackedController>(true);
      Assert.IsNotNull(controller);

      pointer = controller.GetComponentInChildren<CharacterPositionPointer>(true);
      Assert.IsNotNull(pointer);

      chaseCam = SceneHelpers.FindObjectOfType<ChaseCam>(true);
      Assert.IsNotNull(chaseCam);
    }
  }
}
