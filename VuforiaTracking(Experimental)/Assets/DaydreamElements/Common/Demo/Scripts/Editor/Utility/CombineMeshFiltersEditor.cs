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

namespace DaydreamElements.Common {

  using UnityEditor;
  using UnityEngine;
  using System.Collections;

  [CustomEditor(typeof(CombineMeshFilters))]
  [CanEditMultipleObjects]
  public class CombineMeshFiltersEditor : Editor {

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      if (GUILayout.Button("Bake Mesh")) {
        for (int i=0; i<targets.Length; i++) {
          CombineMeshFilters l = targets[i] as CombineMeshFilters;

          Mesh m = l.GetCombineMesh();
          AssetDatabase.CreateAsset(m, "Assets/" + l.gameObject.name +  "_mesh.asset");

          MeshFilter mf = l.GetComponent<MeshFilter>();
          if (mf != null) {
            mf.mesh = m;
          }
        }
      }
    }
  }
}
