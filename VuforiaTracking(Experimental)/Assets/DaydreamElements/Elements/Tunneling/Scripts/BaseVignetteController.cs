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

  /// Base class for controlling the state of a TunnelingVignette.
  /// Allows the user to set a target field of view. Subclasses of
  /// BaseVignetteController should implement a visual transition of the
  /// TunnelingVignette's current field of view to the target field of view.
  [RequireComponent(typeof(TunnelingVignette))]
  public abstract class BaseVignetteController : MonoBehaviour {
    protected TunnelingVignette Vignette { get; private set; }

    /// Set the field of view to display the vignette at.
    /// This number is the radius in degrees from the center of the camera.
    public abstract float FieldOfView { get; set; }

    /// True if the vignette is visible and fully transitioned-in.
    public abstract bool IsVignetteReady { get; }

    /// The recommended field of view (degrees) to use for the vignette when
    /// the camera is translating to prevent motion sickness.
    public const float RECOMMENDED_TRANSLATION_FOV = 40.0f;

    /// The recommended field of view (degrees) to use for the vignette when
    /// the camera is rotating to prevent motion sickness.
    public const float RECOMMENDED_ROTATION_FOV = 30.0f;

    /// Turn on the vignette.
    public abstract void ShowVignette();

    /// Turn off the vignette.
    public abstract void HideVignette();

    public void SetFieldOfViewForTranslation() {
      FieldOfView = RECOMMENDED_TRANSLATION_FOV;
    }

    public void SetFieldOfViewForRotation() {
      FieldOfView = RECOMMENDED_ROTATION_FOV;
    }

    protected virtual void Awake() {
      Vignette = GetComponent<TunnelingVignette>();
    }
  }
}
