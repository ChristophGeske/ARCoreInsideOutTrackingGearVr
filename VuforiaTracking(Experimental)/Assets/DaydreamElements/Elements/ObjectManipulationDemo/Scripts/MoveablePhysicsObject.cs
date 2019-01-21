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

  /// Used for responding to pointer events, and implementing an interactive
  /// physics object.
  [RequireComponent(typeof(Rigidbody))]
  public class MoveablePhysicsObject : BaseInteractiveObject, IPointerDownHandler {

    /// The minimum distance of a selected object.
    [Tooltip("The minimum distance of a selected object.")]
    public float minDistance = 0.5f;

    /// The maximum distance of a selected object.
    [Tooltip("The maximum distance of a selected object.")]
    public float maxDistance = 10;

    /// Setting that determines whether the object will maintain facing to the controller.
    [Tooltip("The object maintains facing to the controller.")]
    public bool maintainFacing = true;

    /// Setting to enable rotation from touch input.
    [Tooltip("Rotate the object on touch.")]
    public bool rotationOnTouch = false;

    /// Setting to clear rigidbody velocities on deselect.
    /// Disabling this can result in undesirable velocity inheritance.
    [Tooltip("Clear rigidbody velocities on drop.")]
    public bool resetPhysicsOnDrop = true;

    /// The scale of touchpad motion from swipe applied in world units.
    [Tooltip("The scale of touchpad motion from swipe applied in world units.")]
    public float distanceIncrementOnSwipe = 2.0f;

    /// The minimum distance of the control transform from the controller.
    [Tooltip("The minimum distance of the control transform from the controller.")]
    public float distanceFromControllerMin = 0.5f;

    /// The maximum distance of the control transform from the controller.
    [Tooltip("The maximum distance of the control transform from the controller.")]
    public float distanceFromControllerMax = 10f;

    /// The smoothing time with respect to object distance.
    [Tooltip("The smoothing time with respect to object distance.")]
    private float distanceSmoothingTime = 0.01f;

    /// The smoothing time with respect to relative orientation.
    [Tooltip("The smoothing time with respect to relative orientation.")]
    private float orientationSmoothingTime = 0.01f;

    /// The speed of rotation from touchpad input in degrees per frame.
    [Tooltip("The speed of rotation from touchpad input in degrees per frame.")]
    public float rotationOnTouchSpeed = 2.0f;

    /// The main camera.
    [Tooltip("The main camera.")]
    public new Camera camera;

    // The rigidbody assigned to this object.
    private Rigidbody rigidbodyCmp;

    // Is the object upside down?
    private bool objectInverted;

    // Default gravity setting for the rigidbody.
    // useGravity will revert to this value on deselect.
    private bool useGravityDefault;
    // Default kinematic setting for the rigidbody.
    // isKinematic will revert to this value on deselect.
    private bool isKinematicDefault;

    // The distance between the control transform and the controller transform.
    private float controlZDistance;
    private float targetControlZDistance;
    private float controlZDistanceSpeed;

    private float targetYRotationFromInput;

    // Changes the responsiveness of the object based on the distance to the
    // control transform.
    private float controlTension;
    // Changes the responsiveness of the object based on rigidbody mass.
    private float weightScale;

    private float touchDownPositionY;
    private float touchPosXRemap;
    private float touchPosYRemap;

    // Control transform that leads the object.
    private Vector3 controlTransformPosition;
    private Quaternion controlTransformRotation;

    private Vector3 normalizedForward;

    private Quaternion objectStartRotation;

    private Quaternion targetOrientationDelta;
    private Quaternion orientationDelta;
    private Quaternion targetRotationDelta;

    private bool swiping;

    const float MAX_ANGULAR_DELTA = 15f;
    const float MIN_MASS = 1f;
    const float MAX_MASS = 10f;

    void Awake() {
      if (rigidbodyCmp == null) {
        rigidbodyCmp = gameObject.GetComponent<Rigidbody>();
      }
      // Store the rigidbody gravity setting.
      useGravityDefault = rigidbodyCmp.useGravity;
      // Store the rigidbody kinematic setting.
      isKinematicDefault = rigidbodyCmp.isKinematic;
    }

    // Input state is checked in Update().
    protected override void OnDrag() {
      UpdateControlTransform();
    }

    // Evaluate rigidbody physics in FixedUpdate().
    void FixedUpdate() {
      // While the object is selected, update the rigidbody.
      if (State == ObjectState.Selected) {
        DragRigidbody();
      }
    }

    protected override void OnSelect(){
      // Perform the transformation relative to control.
      Vector3 vectorToObject = transform.position - ControlPosition;
      float d = vectorToObject.magnitude;

      // Only select the object if it conforms to the min and max distance.
      if(d >= minDistance && d <= maxDistance){
        base.OnSelect();

        // Call the static SetSelected() function on ObjectPointer.
        ObjectManipulationPointer.SetSelected(gameObject.transform, Vector3.zero);

        // The selected object sets initial values for the control transform.
        controlTransformPosition = transform.position;
        controlTransformRotation = transform.rotation;

        // Store the initial rotation for the object.
        objectStartRotation = controlTransformRotation;

        // If the distance vector cannot be normalized, use the look vector.
        if (d > NORMALIZATION_EPSILON) {
          normalizedForward = vectorToObject / d;
        } else {
          d = 0;
          normalizedForward = ControlForward;
        }

        // Reset distance interpolation values to current values.
        targetControlZDistance = controlZDistance = d;
        // Reset orientation interpolation values to 0.
        targetOrientationDelta = orientationDelta = Quaternion.identity;

        // Get the up vector for the object.
        Vector3 objectUp = transform.TransformDirection(Vector3.up);
        // Get the dot product of the object up vector and the world up vector.
        float dotUp = Vector3.Dot(objectUp, Vector3.up);
        // Mark whether the object is upside down or rightside up.
        objectInverted = dotUp < 0;
      }
    }

    protected override void OnDeselect(){
      base.OnDeselect();
      ObjectManipulationPointer.ReleaseSelected(gameObject.transform);
      ResetRigidbody();
    }

    private void UpdateControlTransform() {

      // On a new touch, record the start position.
      if (GvrControllerInput.TouchDown) {
        // Threshold and remap input to be -1 to 1.
        touchDownPositionY = GvrControllerInput.TouchPosCentered.y;
      // While touching, calculate the touchpad drag distance.
      } else if (GvrControllerInput.IsTouching) {
        ZDistanceFromSwipe();
        if (rotationOnTouch && !swiping) {
          RotationFromTouch();
        }
      }

      // Compute orientation delta from selection.
      targetOrientationDelta = ControlRotation * InverseControllerOrientation;

      // If we are smoothing orientation, do it!
      if (orientationSmoothingTime > 0) {
        // Adjust speed of smoothing based on the distance between the target offset, and current offset.
        float speed = Quaternion.Angle(orientationDelta, targetOrientationDelta);
        speed = Mathf.Clamp01(speed / MAX_ANGULAR_DELTA);
        float smoothedDeltaTime = (speed * Time.deltaTime) / orientationSmoothingTime;
        // Apply the delta.
        orientationDelta = Quaternion.Slerp(orientationDelta,
                                            targetOrientationDelta,
                                            smoothedDeltaTime);
      // Otherwise assign it directly.
      } else {
        orientationDelta = targetOrientationDelta;
      }

      // Assign the rotation of the control transform.
      if (maintainFacing) {
        controlTransformRotation = orientationDelta * objectStartRotation * targetRotationDelta;
      } else {
        controlTransformRotation = orientationDelta * targetRotationDelta;
      }

      // Assign the position of the control transform.
      controlTransformPosition = controlZDistance *
                                 (orientationDelta * normalizedForward) +
                                 ControlPosition;

      // Get the distance between the control transform and the controller transform.
      Vector3 targetToControl = ControlPosition - controlTransformPosition;

      // Increase tension when the control transform is closer to the controller transform.
      controlTension = Mathf.Clamp01((distanceFromControllerMax - distanceFromControllerMin) /
                                     (targetToControl.magnitude - distanceFromControllerMin + 0.0001f));

      // Modifies movement responsiveness based on the mass of the rigidbody.
      weightScale = Mathf.Clamp((MAX_MASS / rigidbodyCmp.mass), MIN_MASS, MAX_MASS);
    }

    // Up/down swipe detection on the touchpad for increasing or decreasing the
    // distance of the control transform from the control transform.
    private void ZDistanceFromSwipe() {
      // Compute delta position since last frame.
      float deltaPosition = GvrControllerInput.TouchPosCentered.y - touchDownPositionY;
      touchDownPositionY = GvrControllerInput.TouchPosCentered.y;

      if (deltaPosition > 0.1f) {
        swiping = true;
      } else {
        swiping = false;
      }

      // Set target distance based on touchpad delta.
      targetControlZDistance += deltaPosition * distanceIncrementOnSwipe;
      targetControlZDistance = Mathf.Clamp(targetControlZDistance,
                                           distanceFromControllerMin,
                                           distanceFromControllerMax);

      // If we are smoothing distance, well, smooth distance.
      if(distanceSmoothingTime > 0) {
        controlZDistance = Mathf.SmoothDamp(controlZDistance,
                                            targetControlZDistance,
                                            ref controlZDistanceSpeed,
                                            distanceSmoothingTime);
      // Otherwise just assign it.
      } else {
        controlZDistance = targetControlZDistance;
      }
    }

    // Rotates the control transform based on touchpad input.
    // This rotation will be in object local space.
    private void RotationFromTouch() {
      // Remap touch position to decrease sensitivity in the center of the touchpad.
      touchPosXRemap = GvrControllerInput.TouchPosCentered.x * Mathf.Abs(GvrControllerInput.TouchPosCentered.x);

      // Get target rotation step in each axis from touch position.
      targetYRotationFromInput += rotationOnTouchSpeed * touchPosXRemap;

      // Calculate desired rotation delta.
      // Rotation direction is determined by the orientation of
      // the object at selection time.
      // If the object is rightside up, invert rotation from input.
      if (!objectInverted) {
        targetRotationDelta = Quaternion.AngleAxis(-1.0f * targetYRotationFromInput, Vector3.up);
      } else if (objectInverted) {
        targetRotationDelta = Quaternion.AngleAxis(targetYRotationFromInput, Vector3.up);
      }
    }

    // Update the rigidbody so it follows the control transform.
    private void DragRigidbody() {
      // Turn gravity off for this rigidbody.
      rigidbodyCmp.useGravity = false;
      // Make this rigidbody not kinematic.
      rigidbodyCmp.isKinematic = false;
      // Update rotation for the rigidbody.
      RotateRigidbody();
      // Update the position for the rigidbody.
      MoveRigidbody();
    }

    // Sets the velocity of the rigidbody.
    private void MoveRigidbody() {

      // Get the vector from the control transform to the rigidbody.
      Vector3 forceDirection = controlTransformPosition - rigidbodyCmp.position;
      Vector3 normalizedForce = forceDirection.normalized;
      float distanceFromControlTransform = forceDirection.magnitude;

      // Normalize the rigidbody velocity when it is more than one unit from the
      // target.
      if (distanceFromControlTransform > 1.0f) {
        forceDirection = normalizedForce;
      // Otherwise, scale it by the distance to the target.
      } else {
        forceDirection = forceDirection * distanceFromControlTransform;
      }

      // Set the desired max velocity for the rigidbody.
      Vector3 targetVelocity = forceDirection * weightScale;

      // Have the rigidbody accelerate until it reaches target velocity.
      float timeStep = Mathf.Clamp01(Time.fixedDeltaTime * controlTension * 8.0f);
      rigidbodyCmp.velocity += timeStep * (targetVelocity - rigidbodyCmp.velocity);
    }

    // Sets the angular velocity of the rigidbody.
    private void RotateRigidbody() {
      float angle;
      Vector3 axis;
      // Get the delta between the control transform rotation and the rigidbody.
      Quaternion rigidbodyRotationDelta = controlTransformRotation *
                                          Quaternion.Inverse(rigidbodyCmp.rotation);
      // Convert this rotation delta to values that can be assigned to rigidbody
      // angular velocity.
      rigidbodyRotationDelta.ToAngleAxis(out angle, out axis);
      // Set the angular velocity of the rigidbody so it rotates towards the
      // control transform.
      float timeStep = Mathf.Clamp01(Time.fixedDeltaTime * weightScale * controlTension);
      rigidbodyCmp.angularVelocity = timeStep * angle * axis;
    }
    // Clear all state and mark the object as ready for selection.
    protected override void OnReset() {
      ResetControlTransform();
    }

    // Reset the control transform.
    private void ResetControlTransform() {
      if (controlTransform != null) {
       controlTransformPosition = ControlPosition;
       controlTransformRotation = ControlRotation;
      }
      targetYRotationFromInput = 0f;
      controlTension = 0f;
    }

    // Reset rigidbody properties.
    private void ResetRigidbody() {
      rigidbodyCmp.useGravity = useGravityDefault;
      rigidbodyCmp.isKinematic = isKinematicDefault;
      // Reset the velocity and angular velocity of the rigidbody
      // if resetPhysicsOnDrop is checked.
      if (resetPhysicsOnDrop) {
        rigidbodyCmp.velocity = Vector3.zero;
        rigidbodyCmp.angularVelocity = Vector3.zero;
      }
    }
  }
}
