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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using DaydreamElements.Common;

namespace DaydreamElements.Main {

  /// View for a page of the LevelSelectMenu. Responsible for creating
  /// and managing the buttons on the page based on the data passed in.
  [RequireComponent(typeof(TiledPage))]
  public class LevelSelectPage : MonoBehaviour {
    [SerializeField]
    private GridLayoutGroup buttonLayoutGroup;

    [SerializeField]
    private GameObject buttonPrefab;

    [SerializeField]
    private int maxButtonsPerPage = 6;

    private List<LevelSelectButton> buttons = new List<LevelSelectButton>();
    private Coroutine refreshTiledPageCoroutine;
    private TiledPage tiledPage;

    public int MaxButtonsPerPage {
      get {
        return maxButtonsPerPage;
      }
    }

    public LevelSelectButton AddButton(LevelSelectButtonData buttonData) {
      GameObject buttonObject = GameObject.Instantiate(buttonPrefab);
      LevelSelectButton button = buttonObject.GetComponentInChildren<LevelSelectButton>();
      Assert.IsNotNull(button);

      buttonObject.transform.SetParent(buttonLayoutGroup.transform);
      button.Setup(buttonData);
      buttons.Add(button);

      MarkTilesDirty();

      return button;
    }

    void Awake() {
      tiledPage = GetComponent<TiledPage>();
    }

    private void MarkTilesDirty() {
      if (refreshTiledPageCoroutine != null) {
        return;
      }

      refreshTiledPageCoroutine = StartCoroutine(RefreshTiledPageDelayed());
    }

    private IEnumerator RefreshTiledPageDelayed() {
      yield return null;
      Transform[] tileTransforms = buttons.Select(button => button.transform).ToArray();
      tiledPage.Tiles = tileTransforms;
      refreshTiledPageCoroutine = null;
    }
  }
}
