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
using UnityEngine.EventSystems;

namespace DaydreamElements.ObjectManipulation {

  /// Used for responding to pointer events, and implementing a movable object.
  public class SimpleInteractiveObject : BaseInteractiveObject {

    /// The minimum distance of a selected object.
    [Tooltip("The minimum distance of a selected object.")]
    public float minDistance = 0.5f;

    /// The maximum distance of a selected object.
    [Tooltip("The maximum distance of a selected object.")]
    public float maxDistance = 10;

    /// The scale of the touchpad motion applied in world units.
    [Tooltip("The scale of the touchpad motion applied in world units.")]
    public float distanceScale = 1;

    /// The scale of the touchpad motion applied in degrees.
    [Tooltip("The scale of the touchpad motion applied in degrees.")]
    public float yawScale = 1;

    private Quaternion initialRotation;
    private Vector3 initialForwardVector;
    private float currentDistance;

    protected override void OnSelect() {
      // Perform the transformation relative to control.
      Vector3 vectorToObject = transform.position - ControlPosition;
      float d = vectorToObject.magnitude;

      // Only select the object if it conforms to the min and max distance.
      if (d >= minDistance && d <= maxDistance) {
        base.OnSelect();
        // Grab the initial object rotation
        initialRotation = transform.rotation;

        currentDistance = d;

        // If the distance vector cannot be normalized, use the look vector.
        if (d > NORMALIZATION_EPSILON) {
          initialForwardVector = vectorToObject / d;
        } else {
          d = 0;
          initialForwardVector = ControlForward;
        }
      }
    }

    protected override void OnDeselect() {
      base.OnDeselect();
    }

    protected override void OnDrag() {
      base.OnDrag();

      Quaternion targetDeltaOrientation = GetDeltaRotation();

      transform.rotation = targetDeltaOrientation *
        initialRotation *
        Quaternion.AngleAxis(yawScale * TotalTouchpadMotionSinceSelection.x, Vector3.up);

      transform.position = currentDistance *
        (targetDeltaOrientation * initialForwardVector) + ControlPosition;
    }

    protected override void OnTouchDown() {
      base.OnTouchDown();
    }

    protected override void OnTouchUpdate(Vector2 deltaPosition){
      base.OnTouchUpdate(deltaPosition);
      currentDistance = Mathf.Clamp(currentDistance + deltaPosition.y * distanceScale,
                                    minDistance, maxDistance);
    }

    protected override void OnReset() {
      base.OnReset();
    }
  }
}
