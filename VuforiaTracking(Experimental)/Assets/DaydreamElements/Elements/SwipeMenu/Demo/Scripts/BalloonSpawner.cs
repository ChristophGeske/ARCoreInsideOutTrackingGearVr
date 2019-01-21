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

  public class BalloonSpawner : MonoBehaviour {
    private const float MIN_SPAWN_RADIUS = 8.0f;
    private const float MAX_SPAWN_RADIUS = 16.0f;
    private const float MIN_SPAWN_HEIGHT = 0.0f;
    private const float MAX_SPAWN_HEIGHT = 10.0f;
    private const int TARGET_NUM_BALLOONS = 25;
    private const float MIN_DIST_BETWEEN_BALLOONS = 3.0f;

    private int numBalloons = 0;
    private GameObject[] balloons = new GameObject[TARGET_NUM_BALLOONS];

    public GameObject balloonPrefab;

    void Update() {
      // Initialize all balloons
      for (int i = 0; i < TARGET_NUM_BALLOONS; ++i) {
        if (balloons[i] == null) {
          SpawnBalloon(i);
          return;
        }
      }
    }

    private void SpawnBalloon(int balloonIx) {
      // Get random cylindrical coordinates.
      float spawnRadius = Random.Range(MIN_SPAWN_RADIUS, MAX_SPAWN_RADIUS);
      float spawnHeight = Random.Range(MIN_SPAWN_HEIGHT, MAX_SPAWN_HEIGHT);
      float spawnAngle = Random.Range(-Mathf.PI, Mathf.PI);

      // Convert cylindrical coordinates to Cartesian coordinates.
      float spawnX = spawnRadius * Mathf.Cos(spawnAngle);
      float spawnY = spawnHeight;
      float spawnZ = spawnRadius * Mathf.Sin(spawnAngle);
      Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);

      // If the balloon is too close to other balloons, then try again in
      // a later frame.  This makes sure balloons appear intermittently
      // rather than all on the same frame.
      if (IsTooClose(spawnPosition)) {
        return;
      }

      // Spawn a new balloon at the random coordinate.
      GameObject balloonSpawn = Instantiate(balloonPrefab);
      balloonSpawn.transform.position = spawnPosition;
      balloonSpawn.GetComponent<Balloon>().spawner = this;
      balloonSpawn.GetComponent<Balloon>().balloonIx = balloonIx;

      // Update the balloon count.
      balloons[balloonIx] = balloonSpawn;
    }

    public void BalloonDestroyed(int balloonIx) {
      // Update the balloon count.
      balloons[balloonIx] = null;
      numBalloons--;
    }

    private bool IsTooClose(Vector3 position) {
      for (int i = 0; i < TARGET_NUM_BALLOONS; ++i) {
        if (balloons[i]) {
          float dist = Vector3.Distance(balloons[i].transform.position, position);
          if (dist < MIN_DIST_BETWEEN_BALLOONS) {
            return true;
          }
        }
      }
      return false;
    }
  }
}