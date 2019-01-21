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

namespace DaydreamElements.ClickMenu {

  public class MenuSounds : MonoBehaviour {
    private GvrAudioSource audioSelect;
    private GvrAudioSource audioHover;
    private GvrAudioSource audioBack;

    public ClickMenuRoot menuRoot;

    void Awake () {
      GvrAudioSource[] audioSources = GetComponents<GvrAudioSource>();
      audioSelect = audioSources[0];
      audioHover = audioSources[1];
      audioBack = audioSources[2];
      menuRoot.OnItemSelected += OnItemSelected;
      menuRoot.OnItemHovered += OnItemHovered;
      menuRoot.OnMenuOpened += OnMenuOpened;
    }

    void OnDestroy() {
      if (menuRoot) {
        menuRoot.OnItemSelected -= OnItemSelected;
        menuRoot.OnItemHovered -= OnItemHovered;
        menuRoot.OnMenuOpened -= OnMenuOpened;
      }
    }

    private void OnItemSelected(ClickMenuItem item) {
      if (item == null || item.id == -1) {
        audioBack.Play();
      } else {
        audioSelect.Play();
      }
    }

    private void OnItemHovered(ClickMenuItem item) {
      audioHover.Play();
    }

    private void OnMenuOpened() {
      audioSelect.Play();
    }
  }
}
