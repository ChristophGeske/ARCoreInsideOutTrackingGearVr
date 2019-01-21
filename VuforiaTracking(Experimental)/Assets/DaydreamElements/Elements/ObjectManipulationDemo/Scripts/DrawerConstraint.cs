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

  /// An object that is constrained to move along it's local z-axis
  public class DrawerConstraint : BaseInteractiveObject, IPointerUpHandler {

    /// The minimum distance of a selected object.
    [Tooltip("The minimum distance of a selected object.")]
    public float minDistance = 0.5f;

    /// The maximum distance of a selected object.
    [Tooltip("The maximum distance of a selected object.")]
    public float maxDistance = 10;

    /// The maximum distance that a drawer can be pulled.
    [Tooltip("The maximum distance that a drawer can be pulled.")]
    public float maxPullDistance = 1;

    /// The amount of smoothing to apply to drawer motion.
    [Tooltip("How much smoothing is applied to drawer motion.")]
    public float smoothingTime = 0.1f;

    /// Motion mode to be used by the drawer object.
    [Tooltip("Motion mode for the drawer object.")]
    public MotionMode motionMode = MotionMode.Transform;

    /// The rigidbody assigned to the drawer object.
    [Tooltip("The rigidbody used by the drawer.")]
    public Rigidbody rigidbodyCmp;

    private Vector3 initialLocalPosition;
    private Vector3 localPosition;

    private float zSpeed = 0f;

    private const float SCREEN_EPSILON = .001f;

    void Start() {
      localPosition = initialLocalPosition = transform.localPosition;
    }

    void FixedUpdate() {
      // Move the object in FixedUpdate() if it has a rigidbody attached.
      if (motionMode == MotionMode.Rigidbody && rigidbodyCmp != null) {
        Vector3 targetPosition = localPosition;
        if (transform.parent != null) {
          targetPosition = transform.parent.TransformPoint(localPosition);
        }
        rigidbodyCmp.MovePosition(targetPosition);
      }
    }

    protected override void OnSelect() {
      // Perform the transformation relative to control.
      Vector3 vectorToObject = transform.position - ControlPosition;
      float d = vectorToObject.magnitude;

      // Only select the object if it conforms to the min and max distance.
      if (d >= minDistance && d <= maxDistance) {

        ObjectManipulationPointer.SetSelected(transform, Vector3.zero);

        zSpeed = 0;
        base.OnSelect();
      }
    }

    public void OnPointerUp(PointerEventData data) {
      if (State == ObjectState.Selected) {
        Deselect();
      }
    }

    protected override void OnDeselect() {
      ObjectManipulationPointer.ReleaseSelected(gameObject.transform);
      base.OnDeselect();
    }

    protected override void OnDrag() {
      base.OnDrag();

      Vector3 currentScreenPos = Camera.main.WorldToViewportPoint(transform.position);
      Vector3 pointerScreenPos = Camera.main.WorldToViewportPoint(Camera.main.transform.position +
                                                                  currentScreenPos.z * ControlForward);
      Vector3 forwardScreenPos = Camera.main.WorldToViewportPoint(transform.position +
                                                                  transform.forward);

      Vector2 screenVector = (Vector2)(forwardScreenPos - currentScreenPos);

      float sqrMag = screenVector.sqrMagnitude;

      if (sqrMag < SCREEN_EPSILON) {
        screenVector = new Vector2(1,0);
      } else {
        screenVector = screenVector.normalized;
      }

      Vector2 deltaVector = (Vector2)(pointerScreenPos - currentScreenPos);

      float dotProduct = Vector2.Dot(deltaVector, screenVector);

      float targetZ = Mathf.Clamp(localPosition.z + dotProduct,
                                  initialLocalPosition.z,
                                  initialLocalPosition.z + maxPullDistance);

      localPosition.z = Mathf.SmoothDamp(localPosition.z, targetZ, ref zSpeed, smoothingTime);

      if (motionMode == MotionMode.Transform) {
        transform.localPosition = localPosition;
      }
    }
  }
}
