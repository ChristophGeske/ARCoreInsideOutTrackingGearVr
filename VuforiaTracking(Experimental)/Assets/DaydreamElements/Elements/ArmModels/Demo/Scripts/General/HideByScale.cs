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

namespace DaydreamElements.ArmModels {

  /// Some objects are expensive to enable/disable (i.e. uGUI objects).
  /// To workaround this, this script hides/shows objects by scaling them
  /// while still leaving them enabled.
  public class HideByScale : MonoBehaviour {
    public bool startEnabled;
    public Transform[] objectsToHide;
    public MonoBehaviour[] monobehavioursToDisable;

    void Awake() {
      SetEnabled(startEnabled);
    }

    void OnEnable() {
      SetEnabled(true);
    }

    void OnDisable() {
      SetEnabled(false);
    }

    private void SetEnabled(bool enabled) {
      for (int i = 0; i < objectsToHide.Length; i++) {
        if (enabled) {
          objectsToHide[i].localScale = Vector3.one;
        } else {
          objectsToHide[i].localScale = Vector3.zero;
        }
      }

      for (int i = 0; i < monobehavioursToDisable.Length; i++) {
        monobehavioursToDisable[i].enabled = enabled;
      }
    }
  }
}