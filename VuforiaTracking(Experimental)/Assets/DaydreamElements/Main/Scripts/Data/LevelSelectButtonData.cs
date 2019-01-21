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

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace DaydreamElements.Main {

  /// Serialized Data Model that defines the properties for a button  that is used
  /// to load a specified level.
  [CreateAssetMenu(fileName = "LevelSelectButtonData",
    menuName = "DaydreamElements/LevelSelect/Button",
    order = 1000)]
  public class LevelSelectButtonData : ScriptableObject {

    [SerializeField]
    private Object sceneAsset;

    [SerializeField, HideInInspector]
    private string sceneAssetName;

    [SerializeField]
    private string displayName;

    [SerializeField]
    private Sprite backgroundSprite;

    [SerializeField]
    private GameObject levelSelectResponderPrefab;

    public string SceneName {
      get {
        return sceneAssetName;
      }
    }

    public string DisplayName {
      get {
        return displayName;
      }
    }

    public Sprite BackgroundSprite {
      get {
        return backgroundSprite;
      }
    }

    public GameObject LevelSelectResponderPrefab {
      get {
        return levelSelectResponderPrefab;
      }
    }

#if UNITY_EDITOR
    public Object SceneAsset {
      get {
        return sceneAsset;
      }
    }

    void OnValidate() {
      if (sceneAsset != null) {
        sceneAssetName = AssetDatabase.GetAssetPath(sceneAsset);
        sceneAssetName = Path.GetFileNameWithoutExtension(sceneAssetName);
      } else {
        sceneAssetName = null;
      }
    }
#endif
  }
}
