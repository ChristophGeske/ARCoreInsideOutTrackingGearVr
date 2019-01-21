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

namespace DaydreamElements.SwipeMenu {

  /// Visual ring around the radial menu to further indicate when the player
  /// has successfully selected a color by briefly glowing.
  [RequireComponent(typeof(SpriteRenderer))]
  public class Ring : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    private Color color = Color.white;

    public GameObject swipeMenu;

    void Start() {
      swipeMenu.GetComponent<SwipeMenu>().OnSwipeSelect += OnSwipeSelect;
      spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
      spriteRenderer.color = color;
      color = color * 0.9f + Color.white * 0.1f;
    }

    private void OnSwipeSelect(int index) {
      color = ColorUtil.ToColor((ColorUtil.Type)index);
    }
  }
}
