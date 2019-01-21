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

namespace DaydreamElements.Teleport {
  /// Allow moving the player in the scene by teleporting.
  /// You can fully customize and extend the teleport behavior
  /// by changing the detector, visualizer, and transition classes.
  ///
  /// Teleportation is activated by using triggers, which allow
  /// you to pick which controller buttons will activate teleportation
  /// and rotation. To setup a trigger simply add it as a component
  /// and then assign that component for one of the TeleportController triggers.
  /// You can build your own triggers by subclass BaseActionTrigger if
  /// you have a custom gesture or controller scheme to integrate with.
  ///
  /// Here's an example of a common teleport trigger configuration:
  ///    startTrigger = TouchpadTouchDownTrigger
  ///    cancelTrigger = TouchpadTouchUpTrigger
  ///    commitTrigger = TouchpadClickTrigger
  ///    rotateLeftTrigger = TouchpadSideTrigger (left configured)
  ///    rotateRightTrigger = TouchpadSideTrigger (right configured)
  public class TeleportController : MonoBehaviour {
    /// Player tracking.
    [Tooltip("Player transform to move when teleporting")]
    public Transform player;

    /// Controller transform.
    [Tooltip("Controller transform used for tracking tilt angle")]
    public Transform controller;

    /// Trigger use to start teleport arc selection
    [Tooltip("Trigger used to start teleport selection")]
    public BaseActionTrigger teleportStartTrigger;

    /// Trigger used to commit selection and perform teleport.
    [Tooltip("Trigger used to commit selection and perform teleport")]
    public BaseActionTrigger teleportCommitTrigger;

    /// Trigger use to cancel teleport arc selection
    [Tooltip("Optional trigger used to cancel teleport selection")]
    public BaseActionTrigger teleportCancelTrigger;

    /// Trigger used to activate left rotation.
    [Tooltip("Trigger used to signal left rotation")]
    public BaseActionTrigger rotateLeftTrigger;

    /// Trigger used to activate right rotation.
    [Tooltip("Trigger used to signal right rotation")]
    public BaseActionTrigger rotateRightTrigger;

    /// Detect if there's a valid teleport selection.
    [Tooltip("Detects if there's a valid teleport selection")]
    public BaseTeleportDetector detector;

    /// Visualize active teleport selection.
    [Tooltip("Visualize a teleport selection")]
    public BaseTeleportVisualizer visualizer;

    /// Transition used for teleporting if selection is valid.
    [Tooltip("Transition to use when teleporting player")]
    public BaseTeleportTransition transition;

    /// Rotate player when they click the side of the d-pad.
    [Tooltip("Rotate the player when they click the side of the touchpad")]
    public bool allowRotation = true;

    /// Speed to rotate at while trigger is active.
    [Tooltip("Rotation speed while trigger is active")]
    public float rotationSpeed = 45.0f;

    /// Rotation amount in degrees.
    public float rotationDegreesIncrement = 20.0f;

    /// Flag for if we're currently showing teleport selection.
    private bool selectionIsActive;

    /// State tracking for completing animated rotations.
    private bool isRotating;
    private Quaternion finalRotation;

    // The transform of the current controller.
    private Transform currentController;

    // The selection result returned by the detector.
    private BaseTeleportDetector.Result selectionResult;

    /// Returns true if the user is currently selecting a location to teleport to.
    public bool IsSelectingTeleportLocation {
      get {
        return selectionIsActive;
      }
    }

    /// Returns true if the user is currently teleporting.
    public bool IsTeleporting {
      get {
        if (transition == null) {
          return false;
        }

        return transition.IsTransitioning;
      }
    }

#if UNITY_EDITOR

    void OnValidate() {
      if (Application.isPlaying && isActiveAndEnabled) {
        Start();
      }
    }

#endif

    void Start() {
      if (detector == null) {
        detector = GetComponent<BaseTeleportDetector>();
      }
      if (visualizer == null) {
        visualizer = GetComponent<BaseTeleportVisualizer>();
      }
      if (transition == null) {
        transition = GetComponent<BaseTeleportTransition>();
      }
    }

    void OnDisable() {
      if (!selectionIsActive) {
        return;
      }

      selectionIsActive = false;
      isRotating = false;

      // Clear selection state.
      if (visualizer != null) {
        visualizer.EndSelection();
      }

      // Clear transition state.
      if (transition != null && transition.IsTransitioning) {
          transition.CancelTransition();
      }
    }

    void Update() {
      if (IsConfigured() == false) {
        return;
      }

      currentController = GetControllerTransform();

      // Complete active teleport transitions.
      if (IsTeleporting) {
        visualizer.OnTeleport();
        // Update the visualization.
        visualizer.UpdateSelection(currentController, selectionResult);
        return;
      }

      // If rotation is allowed, handle player rotations.
      if (allowRotation) {
        HandlePlayerRotations();
      }

      // If a teleport selection session has not started, check the appropriate
      // trigger to see if one should start.
      if (selectionIsActive == false) {
        if (teleportStartTrigger.TriggerActive()) {
          StartTeleportSelection();
        }
      }

      // Get the current selection result from the detector.
      float playerHeight = DetectPlayerHeight();
      selectionResult = detector.DetectSelection(currentController, playerHeight);

      // Update the visualization.
      visualizer.UpdateSelection(currentController, selectionResult);

      // If not actively teleporting, just return.
      if (selectionIsActive == false) {
        return;
      }

      // Check for the optional cancel trigger.
      if (teleportCancelTrigger != null &&
          teleportCancelTrigger.TriggerActive() &&
          !teleportCommitTrigger.TriggerActive()) {
        EndTeleportSelection();
        return;
      }

      // When trigger deactivates we finish the teleport.
      if (selectionIsActive && teleportCommitTrigger.TriggerActive()) {
        if (selectionResult.selectionIsValid) {
          Vector3 nextPlayerPosition = new Vector3(
            selectionResult.selection.x,
            selectionResult.selection.y + playerHeight,
            selectionResult.selection.z);

          // Start a transition to move the player.
          transition.StartTransition(
            player,
            currentController,
            nextPlayerPosition);
        }

        EndTeleportSelection();
      }
    }

    private void StartTeleportSelection() {
      detector.StartSelection(currentController);
      visualizer.StartSelection(currentController);
      selectionIsActive = true;
    }

    private void EndTeleportSelection() {
      detector.EndSelection();
      visualizer.EndSelection();
      selectionIsActive = false;
    }

    private float DetectPlayerHeight() {
      RaycastHit hit;
      if (Physics.Raycast(player.position, Vector3.down, out hit)) {
        return hit.distance;
      }
      else{
        // Log error, and default to something sensible.
        Debug.LogError("Failed to detect player height by raycasting downwards");
        return 1.0f;
      }
    }

    // Returns the transform of the assigned controller.
    // The pointer provided by GVR is used if no controller is assigned.
    private Transform GetControllerTransform() {
      if (controller != null) {
        return controller;
      }

      if (GvrPointerInputModule.Pointer == null) {
        return null;
      }

      return GvrPointerInputModule.Pointer.PointerTransform;
    }

    /// Returns true if player rotation was applied.
    private bool HandlePlayerRotations() {
      // Rotate left.
      if (rotateLeftTrigger != null && rotateLeftTrigger.TriggerActive()) {
        Quaternion left = Quaternion.Euler(0, -rotationDegreesIncrement, 0);
        finalRotation = player.rotation * left;
        isRotating = true;
      }

      // Rotate right.
      if (rotateRightTrigger != null && rotateRightTrigger.TriggerActive()) {
        Quaternion right = Quaternion.Euler(0, rotationDegreesIncrement, 0);
        finalRotation = player.rotation * right;
        isRotating = true;
      }

      // Continue a rotation animation each frame.
      if (isRotating) {
        player.rotation = Quaternion.Lerp(player.rotation,
          finalRotation, rotationSpeed * Time.deltaTime);

        if (player.rotation == finalRotation) {
          isRotating = false;
        }

        return true;
      }

      return false;
    }

    // Make sure we have required modules for teleporting.
    private bool IsConfigured() {
      if (teleportStartTrigger == null) {
        Debug.LogError("Can't teleport without start trigger");
        return false;
      }

      if (teleportCommitTrigger == null) {
        Debug.LogError("Can't teleoprt without commit trigger");
        return false;
      }

      if (detector == null) {
        Debug.LogError("Can't teleport without detector");
        return false;
      }

      if (visualizer == null) {
        Debug.LogError("Can't teleport without visualizer");
        return false;
      }

      if (transition == null) {
        Debug.LogError("Can't teleport without transition");
        return false;
      }

      if (player == null) {
        Debug.LogError("Can't teleport without player");
        return false;
      }

      if (GetControllerTransform() == null) {
        Debug.LogError("Can't teleport without controller");
        return false;
      }

      if (allowRotation) {
        if (rotateLeftTrigger == null) {
          Debug.LogError("Can't rotate left without trigger");
          return false;
        }

        if (rotateRightTrigger == null) {
          Debug.LogError("Can't rotate right without trigger");
          return false;
        }
      }

      return true;
    }
  }
}