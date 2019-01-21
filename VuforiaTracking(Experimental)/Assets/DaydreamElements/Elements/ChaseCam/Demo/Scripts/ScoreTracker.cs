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

namespace DaydreamElements.Chase {
  /// Track the players score, and update the display.
  public class ScoreTracker : MonoBehaviour {
    /// Score text label.
    [Tooltip("Text label to display score in")]
    public Text scoreText;

    /// Track players current score.
    private uint score;
    public uint Score {
      get {
        return score;
      }
      private set {
        score = value;
      }
    }

    void Start() {
      UpdateScoreDisplay();
    }

    public void UpdateScoreDisplay() {
      if (scoreText == null) {
        Debug.LogError("Can't update score display without text label");
        return;
      }

      string msg;
      if (Score == 0) {
        msg = "Collect the Pinecones";
      } else if (Score == 1) {
        msg = "1 Pinecone Collected";
      } else {
        msg = Score + " Pinecones Collected";
      }

      scoreText.text = msg;
    }

    public void OnCoinCollected() {
      Score += 1;
      UpdateScoreDisplay();
    }
  }
}
