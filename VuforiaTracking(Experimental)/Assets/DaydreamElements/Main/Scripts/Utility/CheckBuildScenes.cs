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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DaydreamElements.Common;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DaydreamElements.Main {
  /// Used to check if the Daydream Elements Demo scenes have been added to the build settings in the editor.
  /// If they haven't been, the Level Select Menu won't work.
  [ExecuteInEditMode]
  public class CheckBuildScenes : MonoBehaviour {
    [SerializeField]
    private LevelSelectHierarchyData menuHierarchy;

#if UNITY_EDITOR
    private const string DIALOG_TITLE = "Scenes Missing In Build Settings.";
    private const string DIALOG_BODY_1 = "The following Daydream Elements scenes are not " +
                                         "included in your build settings:\n\n";
    private const string DIALOG_BODY_2 = "\nIf you do not add the scenes, then you will " +
                                         "not be able to enter the scene through the Level Select Menu while playing.";

    void Awake() {
      if (Application.isPlaying) {
        return;
      }

      CheckScenes();
    }

    private void CheckScenes() {
      if (menuHierarchy == null) {
        return;
      }

      HashSet<string> currentScenes = new HashSet<string>();
      for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
        EditorBuildSettingsScene editorScene = EditorBuildSettings.scenes[i];
        currentScenes.Add(editorScene.path);
      }

      HashSet<string> missingScenes = new HashSet<string>();
      FindMissingScenes(currentScenes, menuHierarchy.ButtonTree.Root, missingScenes);

      if (missingScenes.Count > 0) {
        ShowMissingScenesDialog(missingScenes);
      }
    }

    private static void FindMissingScenes(HashSet<string> currentScenes, AssetTree.Node currentNode, HashSet<string> missingScenes) {
      LevelSelectButtonData buttonData = currentNode.value as LevelSelectButtonData;
      if (buttonData != null && buttonData.SceneAsset != null) {
        string sceneName = AssetDatabase.GetAssetPath(buttonData.SceneAsset);
        if (!currentScenes.Contains(sceneName)) {
          missingScenes.Add(sceneName);
        }
      }

      for (int i = 0; i < currentNode.children.Count; i++) {
        AssetTree.Node childNode = currentNode.children[i];
        FindMissingScenes(currentScenes, childNode, missingScenes);
      }
    }

    private void ShowMissingScenesDialog(HashSet<string> missingScenes) {
      string body = DIALOG_BODY_1;
      foreach (string missingScene in missingScenes) {
        body += Path.GetFileNameWithoutExtension(missingScene) + "\n";
      }
      body += DIALOG_BODY_2;

      if (EditorUtility.DisplayDialog(DIALOG_TITLE, body, "Add Missing Scenes", "Continue")) {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        scenes = scenes.Concat(missingScenes.Select(x => new EditorBuildSettingsScene(x, true))).ToArray();
        EditorBuildSettings.scenes = scenes;
      }
    }
#endif
  }
}