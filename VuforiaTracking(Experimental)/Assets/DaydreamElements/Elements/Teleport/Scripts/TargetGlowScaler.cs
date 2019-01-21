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
  /// Scales up the teleport target glow as it moves away from player.
  public class TargetGlowScaler : MonoBehaviour {
    [Tooltip("Player position, if empty main camera is used")]
    public Transform player;

    [Tooltip("Glow transform to scale up")]
    public Transform glow;

    [Tooltip("Scaling factor")]
    public float scaleFactor = .25f;

    [Tooltip("Minimum height of glow when close")]
    public float minimumGlowHeight = .25f;

    [Tooltip("Max glow height scale value when far away")]
    public float maximumGlowHeight = 6;

    void Start() {
      // Grab the main camera if
      if (player == null) {
        player = Camera.main.transform;
      }
    }

    void OnEnable() {
      glow.localScale = new Vector3(1, 0, 1);
    }

    void OnDisable() {
      glow.localScale = new Vector3(1, 0, 1);
    }

    // Update is called once per frame
    void Update () {
      if (player == null) {
        Debug.LogError("Can't scale glow target without player");
        return;
      }

      if (glow == null) {
        Debug.LogError("Can't scale glow without transform");
        return;
      }

      float distance = (glow.position - player.position).magnitude;
      float yScaleAmount = Mathf.Pow(distance, 2) * scaleFactor;
      float yGlowScale = Mathf.Clamp(yScaleAmount, minimumGlowHeight, maximumGlowHeight);

      glow.localScale = new Vector3(1, yGlowScale, 1);
    }
  }
}
