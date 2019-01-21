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

  /// An implementation of a hinge that rotates about the y-axis.
  public class HingeConstraint : BaseInteractiveObject, IPointerUpHandler {

    /// The minimum distance of a selected object.
    [Tooltip("The minimum distance of a selected object.")]
    public float minDistance = 0.5f;

    /// The maximum distance of a selected object.
    [Tooltip("The maximum distance of a selected object.")]
    public float maxDistance = 10;

    /// The minimum rotation angle.
    [Tooltip("The minimum rotation angle.")]
    public float minYawDegrees = 0;

    /// The maximum rotation angle.
    [Tooltip("The maximum rotation angle.")]
    public float maxYawDegrees = 90;

    /// Amount of smoothing applied to rotation.
    [Tooltip("Amount of smoothing applied to rotation.")]
    public float smoothingTime = 0.1f;

    /// The y-axis rotation offset.
    [Tooltip("The y-axis rotation offset.")]
    public float rotationOffset = 90;

    /// The local space offset of the door handle.
    [Tooltip("The local space offset of the door handle.")]
    public Vector3 handleOffset;

    /// The motion mode used by this object.
    [Tooltip("The motion mode used by this object.")]
    public MotionMode motionMode = MotionMode.Transform;

    /// The rigidbody used by this object.
    [Tooltip("The rigidbody used by this object.")]
    public Rigidbody rigidbodyCmp;

    private float targetYaw;
    private float yaw = 0;
    private float yawSpeed = 0;

    private Quaternion inveseSelectionRotation;
    private Quaternion initialRotation;
    private Quaternion localRotation;

    private const float SCREEN_EPSILON = 0.001f;
    private const float INITIAL_Z_PULL_DISTANCE = 0.7f;
    private const float Z_PULL_WEIGHT = 10;

    void Start() {
      initialRotation = localRotation = transform.localRotation;
    }

    void FixedUpdate() {
      // Move the object in FixedUpdate() if it has a rigidbody attached.
      if (motionMode == MotionMode.Rigidbody && rigidbodyCmp != null){
        Quaternion targetRotation = localRotation;
        if (transform.parent != null) {
          targetRotation = transform.parent.rotation * (localRotation);
        }
        rigidbodyCmp.MoveRotation(targetRotation);
      }
    }

    protected override void OnSelect() {
      // Perform the transformation relative to control.
      Vector3 vectorToObject = transform.position - ControlPosition;
      float d = vectorToObject.magnitude;

      // Only select the object if it conforms to the min and max distance.
      if (d >= minDistance && d <= maxDistance) {
        ObjectManipulationPointer.SetSelected(transform, handleOffset);
        targetYaw = yaw;
        yawSpeed = 0;
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

      Vector3 worldPoint = transform.TransformPoint(handleOffset);

      Vector3 currentScreenPos = Camera.main.WorldToViewportPoint(transform.position);

      Vector3 handleToControl = (worldPoint - ControlPosition).normalized;
      float deltaDir = Mathf.Clamp01(Vector3.Dot(handleToControl, ControlForward));
      float scale = 1 - deltaDir;

      // We want to 'rotate' the pull vector to be more tangent to the surface as
      // the user points further from the door.  Otherwise, it's impossible to close a
      // door you are facing.  We do this by forcing the pull vector to originate further
      // away in z.
      Vector3 pullPoint = ControlPosition + (INITIAL_Z_PULL_DISTANCE+ Z_PULL_WEIGHT * scale) * currentScreenPos.z * ControlForward;
      Vector3 pullVector = (pullPoint - transform.position).normalized;

      Vector3 localPullVector = pullVector;
      if (transform.parent != null) {
        localPullVector = transform.parent.InverseTransformVector(pullVector);
      }

      float angle = (Mathf.Rad2Deg*Mathf.Atan2(localPullVector.x, localPullVector.z)) + rotationOffset;
      float deltaAngle = Mathf.DeltaAngle(yaw, angle);

      targetYaw = Mathf.Clamp(yaw + deltaAngle, minYawDegrees, maxYawDegrees);
      yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawSpeed, smoothingTime);
      localRotation = Quaternion.AngleAxis(yaw, Vector3.up) * initialRotation;

      if (motionMode == MotionMode.Transform) {
        transform.localRotation = localRotation;
      }
    }
  }
}
