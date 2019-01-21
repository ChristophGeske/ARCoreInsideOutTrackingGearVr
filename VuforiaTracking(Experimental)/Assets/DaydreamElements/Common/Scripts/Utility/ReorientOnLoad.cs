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
using System.Collections;

namespace DaydreamElements.Common {

  /// At the end of the first frame after this script is loaded,
  /// apply the inverse of the camera's y rotation.
  /// Used to make sure that the player is oriented in the correct direction
  /// when entering a scene regardless of the direction that they were facing
  /// at the time. This script should be on a shared parent of the MainCamera,
  /// the controller, and any other objects that need to be rotated with the player.
  public class ReorientOnLoad : MonoBehaviour {
    private Quaternion initialRotation;

    void Start() {
      StartCoroutine(ReorientAtEndOfFrame());
    }

    void Update() {
      // If the play recenter's, then we need to undo the rotation
      // so that the recenter results in the user facing the correct direction.
      if (GvrControllerInput.Recentered && initialRotation != Quaternion.identity) {
        transform.localRotation = transform.localRotation * Quaternion.Inverse(initialRotation);
        initialRotation = Quaternion.identity;
      }
    }

    private IEnumerator ReorientAtEndOfFrame() {
      yield return null;
      Reorient();
    }

    private void Reorient() {
      Quaternion cameraRotation = Camera.main.transform.localRotation;
      float yRotation = cameraRotation.eulerAngles.y;
      initialRotation = Quaternion.Euler(0.0f, -yRotation, 0.0f);
      transform.localRotation = transform.localRotation * initialRotation;
    }
  }
}