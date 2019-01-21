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

namespace DaydreamElements.Tunneling {

  /// Transitions a TunnelingVignette between it's current field of view
  /// and it's target field of view by fading the vignette in
  /// when the vignette becomes visible.
  public class FadeVignetteController : BaseVignetteController {
    [Tooltip("Speed that the vignette fades in and out.")]
    public float fadeSpeed = 8.0f;

    [Tooltip("Speed that the vignette opens and closes.")]
    public float irisSpeed = 6.0f;

    [Tooltip("The minimum alpha that the vignette must be at to be considered ready.")]
    [Range(0.0f, 1.0f)]
    public float alphaReadyThreshold = 0.7f;

    private float fieldOfView = BaseVignetteController.RECOMMENDED_TRANSLATION_FOV;
    private bool showVignette = false;

    public override float FieldOfView {
      get {
        return fieldOfView;
      }
      set {
        fieldOfView = value;

        // Set the vignette FOV immediately if the vignette isn't ready.
        // Otherwise, lerp to the field of view in update.
        if (!IsVignetteReady) {
          Vignette.CurrentFOV = FieldOfView;
        }
      }
    }

    public override bool IsVignetteReady {
      get {
        return Vignette.Alpha >= alphaReadyThreshold;
      }
    }

    public override void ShowVignette() {
      showVignette = true;
      Vignette.CurrentFOV = FieldOfView;
    }

    public override void HideVignette() {
      showVignette = false;
    }

    void Update() {
      UpdateAlpha();
      UpdateFOV();
    }

    private void UpdateAlpha() {
      float targetAlpha = showVignette ? 1.0f : 0.0f;
      if (Vignette.Alpha == targetAlpha) {
        if (!showVignette) {
          Vignette.CurrentFOV = TunnelingVignette.MAX_FOV;
        }
        return;
      }

      Vignette.Alpha = Mathf.Lerp(Vignette.Alpha, targetAlpha, Time.deltaTime * fadeSpeed);

      if (Mathf.Abs(Vignette.Alpha - targetAlpha) < 0.01f) {
        Vignette.Alpha = targetAlpha;
        Vignette.CurrentFOV = FieldOfView;
      }
    }

    private void UpdateFOV() {
      if (!showVignette) {
        return;
      }

      if (Mathf.Abs(FieldOfView - Vignette.CurrentFOV) > 0.01f) {
        Vignette.CurrentFOV = Mathf.Lerp(Vignette.CurrentFOV, FieldOfView, Time.deltaTime * irisSpeed);
      } else {
        Vignette.CurrentFOV = FieldOfView;
      }
    }
  }
}
