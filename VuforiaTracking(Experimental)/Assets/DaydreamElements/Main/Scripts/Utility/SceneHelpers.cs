﻿// Copyright 2017 Google Inc. All rights reserved.
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
using UnityEngine.SceneManagement;
using System.Collections;

namespace DaydreamElements.Main {

  public static class SceneHelpers {
    /// Find object of a type in the active scene.
    /// Unlike GameObject.FindObjectOfType, this can include
    /// inactive objects.
    public static T FindObjectOfType<T>(bool includeInactive) {
      Scene activeScene = SceneManager.GetActiveScene();
      GameObject[] rootObjects = activeScene.GetRootGameObjects();
      foreach (GameObject gameObject in rootObjects) {
        T result = gameObject.GetComponentInChildren<T>(includeInactive);
        if (result != null) {
          return result;
        }
      }

      return default(T);
    }

    public static void SetLayerRecursively(GameObject obj, int layer) {
      obj.layer = layer;
      for (int i = 0; i < obj.transform.childCount; i++) {
        GameObject child = obj.transform.GetChild(i).gameObject;
        SetLayerRecursively(child, layer);
      }
    }
  }
}
