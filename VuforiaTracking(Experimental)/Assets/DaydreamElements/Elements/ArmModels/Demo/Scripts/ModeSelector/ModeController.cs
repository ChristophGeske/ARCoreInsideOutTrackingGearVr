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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaydreamElements.ArmModels {

  /// Used to switch between modes to test out different arm models.
  public class ModeController : MonoBehaviour {
    public enum Mode {
      None,
      Selector,
      Zapper,
      Sword,
      Throw,
      Customize
    }

    [Serializable]
    public struct ModeData {
      public Mode mode;
      public GameObject obj;
    }

    [Tooltip("Associates objects in the scene with a particular mode.")]
    public ModeData[] modes;

    private Mode currentMode = Mode.None;

    public Mode CurrentMode {
      get {
        return currentMode;
      }
      set {
        if (currentMode == value) {
          return;
        }

        currentMode = value;
        SetModeActive(currentMode);
      }
    }

    void Start() {
      CurrentMode = Mode.Selector;
    }

    void Update() {
      if (GvrControllerInput.AppButtonUp) {
        CurrentMode = Mode.Selector;
      }
    }

    private void SetModeActive(Mode mode) {
      foreach (ModeData modeData in modes) {
        bool active = modeData.mode == mode;
        SetActiveAnimated activateAnimation = modeData.obj.GetComponent<SetActiveAnimated>();
        if (activateAnimation != null) {
          activateAnimation.SetActive(active);
        } else {
          modeData.obj.SetActive(active);
        }
      }
    }
  }
}