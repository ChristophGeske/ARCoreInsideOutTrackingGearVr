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
using UnityEngine.EventSystems;

namespace DaydreamElements.Chase {

  /// This class shows a laser pointer with an optional target
  /// prefab positioned at the end of the laser where it hit something. When
  /// The trigger is activated, and we're pointed at a valid position, this
  /// class will ask the positioned character to move to that location. You
  /// can customize what is considered a valid location by subclassing and
  /// overriding IsPointedAtValidMovePosition(), or by ignoring the position
  /// inside your positioned character subclass.
  [RequireComponent(typeof(LineRenderer))]
  public class CharacterPositionPointer : GvrBasePointer, IGvrArmModelReceiver {
    /// Color of the line pointer.
    [Tooltip("Color of the laser pointer")]
    public Color laserColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);

    /// Width of the line used for the laser pointer.
    public float lineWidth = .1f;

    /// Trigger used to confirm a selection and move to point.
    [Tooltip("Trigger for selecting new position to move to")]
    public BaseActionTrigger moveTrigger;

    /// Character we'll integrate to pass new move requests.
    [Tooltip("Positioned character this pointer is controlling")]
    public BasePositionedCharacter character;

    /// Target prefab is positioned at end of the laser pointer.
    [Tooltip("Prefab to place at the end of the laser pointer")]
    public GameObject targetPrefab;

    [Tooltip("Maximum surface angle for valid selections")]
    [Range(0, 180)]
    public float maxHitAngleDegrees = 30.0f;

    public float maxPointerDistance = 20.0f;

    private LineRenderer line;

    /// Position of current pointer raycast hit.
    private Vector3 hitPosition;
    private GameObject hitGameObject;

    /// Target object to position at hit point.
    private GameObject target;

    /// Raycast hit result if pointed at object.
    private RaycastResult hitRaycastResult;

    public GvrBaseArmModel ArmModel { get; set; }

    protected override void Start() {
      base.Start();

      if (targetPrefab == null) {
        Debug.LogError("Character position pointer must have target prefab!");
        return;
      }

      line = GetComponent<LineRenderer>();
      target = Instantiate(targetPrefab, transform);
    }

    void Update() {
      UpdateTargetPosition();

      // We hide the pointer target if there's no valid selection.
      bool isValid = IsPointedAtObject && IsPointedAtValidAngle();
      target.SetActive(isValid);

      if (!isValid) {
        return;
      }

      if (character == null) {
        Debug.LogError("Can't move null positioned character");
        return;
      }

      if (moveTrigger == null) {
        Debug.LogError("Can't position character without a move trigger to check");
        return;
      }

      if (moveTrigger.TriggerActive()) {
        // Ask the character to move to pointed at position.
        character.SetTargetPosition(hitPosition);
        return;
      }
    }

    void LateUpdate() {
      UpdateLaserPointer();
    }

    /// Ignore steep surfaces, like walls.
    protected virtual bool IsPointedAtValidAngle() {
      if (!IsPointedAtObject) {
        return false;
      }

      float angle = Vector3.Angle(Vector3.up, hitRaycastResult.worldNormal);
      return angle <= maxHitAngleDegrees;
    }

    /// True if we're currently pointed at something.
    public bool IsPointedAtObject {
      get {
        return hitGameObject != null;
      }
    }

    public override float MaxPointerDistance {
      get {
        return maxPointerDistance;
      }
    }

    /// Called when the pointer is facing a valid GameObject. This can be a 3D
    /// or UI element.
    public override void OnPointerEnter(RaycastResult raycastResult, bool isInteractive) {
      hitRaycastResult = raycastResult;
      hitGameObject = raycastResult.gameObject;
      hitPosition = raycastResult.worldPosition;
      UpdateLaserPointer();
    }

    /// Called every frame the user is still pointing at a valid GameObject. This
    /// can be a 3D or UI element.
    public override void OnPointerHover(RaycastResult raycastResultResult, bool isInteractive) {
      hitRaycastResult = raycastResultResult;
      hitGameObject = raycastResultResult.gameObject;
      hitPosition = raycastResultResult.worldPosition;
      UpdateLaserPointer();
    }

    /// Called when the pointer no longer faces an object previously
    /// intersected with a ray projected from the camera.
    /// This is also called just before **OnInputModuleDisabled** and may have have any of
    /// the values set as **null**.
    public override void OnPointerExit(GameObject targetObject) {
      hitGameObject = null;
      hitPosition = Vector3.zero;
      UpdateLaserPointer();
    }

    /// Called when a click is initiated.
    public override void OnPointerClickDown() {
    }

    /// Called when click is finished.
    public override void OnPointerClickUp() {
    }

    public override void GetPointerRadius(out float enterRadius, out float exitRadius) {
      enterRadius = 0;
      exitRadius = 0;
    }

    private Vector3 CalculateLaserEndPoint() {
      if (hitGameObject != null) {
        return hitPosition;
      } else {
        return base.PointerTransform.position
        + (base.PointerTransform.forward * maxPointerDistance);
      }
    }

    private void UpdateLaserPointer() {
      Vector3 lineEndPoint = CalculateLaserEndPoint();
      line.SetPosition(0, base.PointerTransform.position);
      line.SetPosition(1, lineEndPoint);

      float preferredAlpha = ArmModel != null ? ArmModel.PreferredAlpha : .5f;
      line.startColor = Color.Lerp(Color.clear, laserColor, preferredAlpha);
      line.endColor = Color.clear;
    }

    private void UpdateTargetPosition() {
      if (target == null) {
        return;
      }

      target.SetActive(hitGameObject != null);
      target.transform.position = hitPosition;
      target.transform.rotation = Quaternion.identity;
    }
  }
}
