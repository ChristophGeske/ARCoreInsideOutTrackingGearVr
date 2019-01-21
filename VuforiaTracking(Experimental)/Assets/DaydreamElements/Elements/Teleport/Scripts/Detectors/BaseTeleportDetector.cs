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
  /// Detect if the controller is pointed at a valid teleport location.
  public abstract class BaseTeleportDetector : MonoBehaviour {

    /// Information about a detector selection.
    public struct Result {
      /// Flag that detector selection is valid for teleporting to.
      public bool selectionIsValid;

      /// Position that is selected.
      public Vector3 selection;

      /// If selection is valid, surface normal at the selection.
      public Vector3 selectionNormal;

      /// Object currently selected.
      public GameObject selectionObject;

      /// Maximum distance for teleporting.
      public float maxDistance;
    }

      /// Layers that user can teleport to.
    [Tooltip("Layers that user can teleport to.")]
    public LayerMask validTeleportLayers = Physics.AllLayers;

    /// Layers that raycasting can hit.
    [Tooltip("Layers that raycasting can hit.")]
    public LayerMask raycastMask = Physics.AllLayers;

    /// Max distance to allow teleporting.
    [Tooltip("Max distance to allow teleporting.")]
    public float maxDistance = 20;

    /// Maximum angle that surface can be for teleporting onto.
    [Tooltip("Maximum angle for teleportation destination surface.")]
    public float maxSurfaceAngle = 45;

    // Start teleport selection.
    public abstract void StartSelection(Transform controller);

    /// End teleport selection.
    public abstract void EndSelection();

    // Return true if there's a raycast hit in valid teleport location.
    public abstract Result DetectSelection(Transform controllerTransform, float playerHeight);
  }
}
