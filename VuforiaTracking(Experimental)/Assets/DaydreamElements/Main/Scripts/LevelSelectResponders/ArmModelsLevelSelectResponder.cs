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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using DaydreamElements.ArmModels;

namespace DaydreamElements.Main {

  /// Used to disable armModels and mode controller when the navigation menu is open.
  public class ArmModelsLevelSelectResponder : BaseLevelSelectResponder {
    private ModeController modeController;
    private GameObject armModels;
    private GvrBasePointer initialPointer;
    private bool enableModeControllerOnUp;
    private List<GvrPointerPhysicsRaycaster> physicsRaycasters = new List<GvrPointerPhysicsRaycaster>();
    private bool[] wasPhysicsRaycasterEnabledArray;
    private GvrPointerGraphicRaycaster graphicRaycaster;
    private bool wasGraphicRaycasterEnabled;

    public override void OnMenuOpened() {
      SetArmModelsEnabled(false);
    }

    public override void OnMenuClosed() {
      SetArmModelsEnabled(true);
    }

    void Start() {
      armModels = GameObject.Find("ModesArmModel");
      Assert.IsNotNull(armModels);
      modeController = SceneHelpers.FindObjectOfType<ModeController>(true);
      Assert.IsNotNull(modeController);
      initialPointer = GvrPointerInputModule.Pointer;
      for (int i = 0; i < Camera.allCameras.Length; i++) {
        GvrPointerPhysicsRaycaster raycaster = Camera.allCameras[i].GetComponent<GvrPointerPhysicsRaycaster>();
        if (raycaster != null) {
          physicsRaycasters.Add(raycaster);
        }
      }

      wasPhysicsRaycasterEnabledArray = new bool[physicsRaycasters.Count];

      GameObject customizeMenu = GameObject.Find("CustomizeArmModelUI");
      Assert.IsNotNull(customizeMenu);
      graphicRaycaster = customizeMenu.GetComponent<GvrPointerGraphicRaycaster>();
      Assert.IsNotNull(graphicRaycaster);
    }

    void Update() {
      // The menu is closed on appButtonDown, but we don't want to re-enable
      // the ModeController until appButtonUp to make sure the arm model mode isn't exited when
      // the user intended to simply close the navigation menu.
      if (enableModeControllerOnUp && !GvrControllerInput.AppButton) {
        enableModeControllerOnUp = false;
        modeController.enabled = true;
      }
    }

    void OnDisable() {
      for (int i = 0; i < physicsRaycasters.Count; i++) {
        if (physicsRaycasters[i] != null) {
          physicsRaycasters[i].enabled = wasPhysicsRaycasterEnabledArray[i];
        }
      }
    }

    private void SetArmModelsEnabled(bool enabled) {
      if (enabled) {
        GvrPointerInputModule.Pointer = initialPointer;
        enableModeControllerOnUp = true;
        for (int i = 0; i < physicsRaycasters.Count; i++) {
          physicsRaycasters[i].enabled = wasPhysicsRaycasterEnabledArray[i];
        }
        graphicRaycaster.enabled = wasGraphicRaycasterEnabled;
      }

      GvrBasePointer currentPointer = GvrPointerInputModule.Pointer;

      armModels.SetActive(enabled);

      // Disabling armModels will set the Pointer to the one used to select which arm model to try.
      // Force it back to the navigation menu pointer.
      if (!enabled) {
        GvrPointerInputModule.Pointer = currentPointer;
        modeController.enabled = false;
        for (int i = 0; i < physicsRaycasters.Count; i++) {
          wasPhysicsRaycasterEnabledArray[i] = physicsRaycasters[i].enabled;
          physicsRaycasters[i].enabled = false;
        }
        wasGraphicRaycasterEnabled = graphicRaycaster.enabled;
        graphicRaycaster.enabled = false;
      }
    }
  }
}
