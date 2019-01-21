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

namespace DaydreamElements.ArmModels {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using DaydreamElements.Common;

  public class BallSpawner : MonoBehaviour {

    public GameObject ballPrefab;
    public int maxBalls;

    private GameObjectPool ballPool;
    private List<Ball> spawnedBalls = new List<Ball>();
    private Ball currentBall;

    void Start() {
      ballPool = new GameObjectPool(ballPrefab, maxBalls, maxBalls);
      Spawn();
    }

    public Ball TakeBall() {
      if (!currentBall.Ready) {
        return null;
      }

      Ball ball = currentBall;
      ball.OnTaken();
      currentBall = null;
      return ball;
    }

    public void ReturnBall(Ball ball) {
      if (ball == null || ball == currentBall) {
        return;
      }

      ball.Reset();
      ball.OnThrown -= OnBallThrown;
      ballPool.Return(ball.gameObject);
      spawnedBalls.Remove(ball);
    }

    void OnTriggerEnter(Collider other) {
      ThrowController throwController = other.GetComponent<ThrowController>();
      if (throwController == null || currentBall == null || !currentBall.Ready) {
        return;
      }

      currentBall.SetHighlightEnabled(true);
    }

    void OnTriggerStay(Collider other) {
      OnTriggerEnter(other);
    }

    void OnTriggerExit(Collider other) {
      ThrowController throwController = other.GetComponent<ThrowController>();
      if (throwController == null || currentBall == null || !currentBall.Ready) {
        return;
      }

      currentBall.SetHighlightEnabled(false);
    }

    private void Spawn() {
      if (ballPool.IsPoolEmpty) {
        if (spawnedBalls.Count == 0) {
          Debug.LogError("No balls available to spawn.");
          return;
        } else {
          Ball oldBall = spawnedBalls[0];
          ReturnBall(oldBall);
        }
      }

      GameObject ballObject = ballPool.Borrow();
      ballObject.transform.SetParent(transform, false);
      ballObject.transform.localPosition = Vector3.zero;
      ballObject.transform.localRotation = Quaternion.identity;

      currentBall = ballObject.GetComponent<Ball>();
      currentBall.OnThrown += OnBallThrown;
      spawnedBalls.Add(currentBall);

      currentBall.PlaySpawnAnimation();
    }

    private void OnBallThrown() {
      Spawn();
    }
  }
}