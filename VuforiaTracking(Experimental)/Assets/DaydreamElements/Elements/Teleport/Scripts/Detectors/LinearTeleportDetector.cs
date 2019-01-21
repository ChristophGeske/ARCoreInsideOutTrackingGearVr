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
  /// Teleport detecter that does a simple direct raycast for layers.
  public class LinearTeleportDetector : BaseTeleportDetector {

    // Start teleport selection.
    public override void StartSelection(Transform controller) {
    }

    /// End teleport selection.
    public override void EndSelection() {
    }

    /// Return true if there's a raycast hit in valid teleport location.
    public override Result DetectSelection(Transform controllerTransform, float playerHeight) {
      Result result = new Result();
      result.maxDistance = maxDistance;

      Vector3 start = controllerTransform.position;
      Vector3 direction = controllerTransform.forward;

      RaycastHit hit;
      if (Physics.Raycast(start, direction, out hit, maxDistance, raycastMask) == false) {
        // Set max selection point.
        result.selection = start + (controllerTransform.forward * maxDistance);
        return result;
      }

      result.selection = hit.point;
      result.selectionNormal = hit.normal;
      result.selectionObject = hit.collider.gameObject;

      // Validate that we hit a layer that's valid for teleporting.
      if ((validTeleportLayers.value & (1 << hit.collider.gameObject.layer)) == 0) {
        return result;
      }

      // Validate the angle relative to global up, so users don't teleport into walls etc.
      float angle = Vector3.Angle(Vector3.up, hit.normal);
      if (angle > maxSurfaceAngle) {
        return result;
      }

      result.selectionIsValid = true;
      return result;
    }
  }
}
