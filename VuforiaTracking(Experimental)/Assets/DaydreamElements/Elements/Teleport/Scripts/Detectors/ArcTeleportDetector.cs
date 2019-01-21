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

namespace DaydreamElements.Teleport {
  /// Teleport detector designed for arc-like raycasting from the controller.
  public class ArcTeleportDetector : BaseTeleportDetector {
    /// Maximum controller pitch.
    [Tooltip("Maximum controller pitch.")]
    public float maxControllerPitch = 60f;

    /// The maximum vertical distance that can be traveled while teleporting.
    [Tooltip("Maximum vertical displacement from teleporting.")]
    public float maxVerticalGain = 10f;

    /// The angle to offset the pitch of the detection ray in degrees.
    /// -15 will match the behavior of the GVR pointer.
    [Tooltip("Angle to offset the detection ray.")]
    public float forwardAngleOffset = -15f;

    /// Positional offset for the start of the detection ray.
    [Tooltip("Offset for the origin of the detection ray.")]
    public Vector3 rayOriginOffset = new Vector3(0, -0.0099f, 0.1079f);

    /// The radis used to calculate the amount of open space needed around the player
    /// for the selection to be considered valid.
    [Tooltip("Radius used for the amount of open space around the selection.")]
    public float openSpaceRadius = 0.4f;

    /// The Offset from the floor used for checking the collision of open space around the selection.
    [Tooltip("Offset from the floor for checking collision of open space around the selection.")]
    public float openFloorOffset = 0.25f;

    /// Used when checking collisions to ensure there is enough open space around the selection.
    [Tooltip("Used when checking collisions for the open space around the selection.")]
    public LayerMask openSpaceLayerMask = Physics.AllLayers;

    /// When true, the detector will attempt to find a valid
    /// teleport destination.
    [Tooltip("Whether the detector is active.")]
    public bool active = false;

    private Collider[] capsuleTestResults = new Collider[1];

#if UNITY_EDITOR

    // Debug helper to show raycasting lines in the editor.
    [Tooltip("Show debug lines for raycasting angles in scene editor.")]
    public bool debugRaycasting;

#endif

    private const float MAX_PITCH_WITH_OFFSET = 80f;
    private const float MAX_PITCH_BLEND = 10f;

    public override void StartSelection(Transform controller) {
      active = true;
    }
    public override void EndSelection() {
      active = false;
    }

    // Detect if there's a valid selection by raycasting from a distance above the controller.
    public override Result DetectSelection(Transform controller, float playerHeight) {

      Result result = new Result();
      result.maxDistance = maxDistance;

      Ray ray;
      GetRaycast(controller, out ray);

      RaycastHit hit;

#if UNITY_EDITOR

      // Draw a debug line showing where the raycast happens from and it's current angle downwards.
      if (debugRaycasting) {
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.white);
      }

#endif

      // Selection result defaults to the origin point of the raycast if there is not a hit.
      Vector3 defaultSelectionResult = ray.origin;

      if (!active) {
        result.selection = defaultSelectionResult;
        result.selectionIsValid = false;
        return result;
      }

      // First, raycast forward from the controller up to max raycast distance.
      if (Physics.Raycast(ray.origin, ray.direction,
                          out hit, maxDistance, raycastMask)) {
        // If there is a forward hit, move on to the next check.
        result.selection = hit.point;
        result.selectionNormal = hit.normal;
        result.selectionObject = hit.collider.gameObject;
      } else {
        // If there is no forward hit, raycast down from the end of the ray.
        ray.origin = ray.GetPoint(maxDistance);
        ray.direction = Vector3.down;

        if (Physics.Raycast(ray.origin, ray.direction,
                            out hit, maxDistance, raycastMask)) {
          result.selection = hit.point;
          result.selectionNormal = hit.normal;
          result.selectionObject = hit.collider.gameObject;
        // If there is still no hit, no valid selection is returned.
        } else {
          result.selection = defaultSelectionResult;
          result.selectionIsValid = false;
          return result;
        }
      }

      // Validate the angle relative to global up, so users don't teleport into walls, etc.
      float angle = Vector3.Angle(Vector3.up, hit.normal);
      if (angle > maxSurfaceAngle) {
        // Do a vertical raycast from the hit point in case there is valid terrain nearby.
        ray.origin = hit.point + new Vector3(0, maxVerticalGain, 0);
        ray.direction = Vector3.down;

        if (Physics.Raycast(ray.origin, ray.direction,
                            out hit, maxDistance, raycastMask)) {
          result.selection = hit.point;
          result.selectionNormal = hit.normal;
          result.selectionObject = hit.collider.gameObject;
        // If no surface is found, no valid selection is returned.
        } else {
          result.selection = defaultSelectionResult;
          result.selectionIsValid = false;
          return result;
        }
      }

      // Validate that the player has enough open space at the selection.
      Vector3 point0 = result.selection + Vector3.up * (openSpaceRadius + openFloorOffset);
      Vector3 point1 = point0 + Vector3.up * playerHeight;
      if (Physics.OverlapCapsuleNonAlloc(point0,
                                         point1,
                                         openSpaceRadius,
                                         capsuleTestResults,
                                         openSpaceLayerMask) > 0) {
        result.selection = defaultSelectionResult;
        result.selectionIsValid = false;
        return result;
      }

      // Validate that we hit a layer that's valid for teleporting.
      if ((validTeleportLayers.value & (1 << hit.collider.gameObject.layer)) == 0) {
        result.selection = defaultSelectionResult;
        result.selectionIsValid = false;
        return result;
      }

      result.selectionIsValid = true;

      return result;
    }

    public void GetRaycast(Transform controller, out Ray ray) {
      // Get the origin point of the ray.
      Vector3 raycastOrigin = controller.position +
                              controller.up * rayOriginOffset.y +
                              controller.forward * rayOriginOffset.z;

      // Get the direction of the ray by applying the desired pitch
      // offset to the forward direction of the controller.
      Vector3 forward = controller.forward;
      // Get the pitch of the controller.
      float pitch = Mathf.Rad2Deg * Mathf.Asin(forward.y);
      float absPitch = Mathf.Abs(pitch);
      // Safeguards to prevent undesired behavior around rotation poles.
      if (absPitch < MAX_PITCH_WITH_OFFSET) {
        float pitchBlend = 1 - Mathf.Clamp01((absPitch - (MAX_PITCH_WITH_OFFSET - MAX_PITCH_BLEND)) / MAX_PITCH_BLEND);
        // Apply the visual offset to the pitch of the arc. Blend the offset to
        // zero as controller pitch approaches -90 or 90 degrees.
        float angleOffset = pitchBlend * forwardAngleOffset;
        float yaw = Mathf.Rad2Deg * Mathf.Atan2(forward.x, forward.z);
        float pitchRad = Mathf.Deg2Rad * (angleOffset + pitch);
        Vector3 pitchVec = new Vector3(0, Mathf.Sin(pitchRad), Mathf.Cos(pitchRad));
        // Calculate the axes of the forward vector in the appropriate order.
        forward = Quaternion.AngleAxis(yaw, Vector3.up) * pitchVec;
      }

      ray = new Ray(raycastOrigin, forward);
    }
  }
}