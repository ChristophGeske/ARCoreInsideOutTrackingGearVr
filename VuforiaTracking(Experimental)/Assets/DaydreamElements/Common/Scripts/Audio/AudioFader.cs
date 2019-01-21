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

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  [RequireComponent(typeof(GvrAudioSource))]
  public class AudioFader : MonoBehaviour {
    /// Volume change per second.
    [Range(0.1f, 10.0f)]
    public float fadeSpeed = 2.0f;

    /// Volume level to lerp to.
    [Range(0.0f, 1.0f)]
    public float targetVolume = 1.0f;

    private GvrAudioSource audioSource;

    private const float SIMILARITY_THRESHOLD = 0.01f;

    public bool IsFading {
      get {
        return audioSource.volume != targetVolume;
      }
    }

    void Awake() {
      audioSource = GetComponent<GvrAudioSource>();
    }

  	void Update() {
      float currentVolume = audioSource.volume;

      if (currentVolume == targetVolume) {
        return;
      }

      float dt = Time.deltaTime;
      currentVolume = Mathf.Lerp(currentVolume, targetVolume, fadeSpeed * dt);

      float diff = Mathf.Abs(targetVolume - currentVolume);
      if (diff < SIMILARITY_THRESHOLD) {
        currentVolume = targetVolume;
      }

      audioSource.volume = currentVolume;
  	}
  }
}