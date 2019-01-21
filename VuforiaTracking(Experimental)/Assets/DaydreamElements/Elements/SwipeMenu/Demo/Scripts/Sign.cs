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

namespace DaydreamElements.SwipeMenu {

  [RequireComponent(typeof(GvrAudioSource))]
  public class Sign : MonoBehaviour {
    private TextMesh textMesh;
    private float timerEnd;
    private int prevTime;
    private GvrAudioSource audioSource;

    private static bool started = false;
    private static int score = 0;

    public GameObject signText;

    public static void IncScore() {
      if (started) {
        ++score;
      }
    }

    void Start() {
      audioSource = GetComponent<GvrAudioSource>();
      textMesh = signText.GetComponent<TextMesh>();
      Debug.Assert(textMesh != null);
    }

    void Update() {
      if (started) {
        int newTime = (int)(timerEnd - Time.realtimeSinceStartup) + 1;
        if (newTime <= 0) {
          started = false;
          textMesh.text = "Game Over!\nShoot again to restart\nYour score: " + score + "\n";
        } else if (newTime != prevTime) {
          prevTime = newTime;
          textMesh.text = "00:" + newTime.ToString("00") + "\nYour score: " + score + "\n";
          if (newTime == 5) {
            audioSource.Play();
          }
        }
      }
    }

    void OnCollisionEnter(Collision collision) {
      RigidPaperAirplane rigidPencil = collision.gameObject.GetComponent<RigidPaperAirplane>();
      if (rigidPencil == null) {
        return;
      }
      if (!started) {
        started = true;
        timerEnd = Time.realtimeSinceStartup + 60.0f;
        prevTime = 0;
        score = 0;
      }
    }
  }
}
