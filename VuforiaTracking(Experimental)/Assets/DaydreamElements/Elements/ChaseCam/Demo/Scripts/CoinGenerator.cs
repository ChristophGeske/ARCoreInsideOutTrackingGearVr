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
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DaydreamElements.Chase {
  /// Class generates a new coin at current position if coin was collected.
  public class CoinGenerator : MonoBehaviour {
    /// Prefab for a coin object.
    [Tooltip("Coin prefab to spawn")]
    public GameObject coinPrefab;

    /// Transform of the player, so we don't respawn when nearby.
    [Tooltip("Player objects transform")]
    public Transform player;

    /// Amount of time before spawning new coin after collection.
    [Tooltip("Amount of time between spawning coins")]
    public float spawnDelay = 5.0f;

    /// Event sent when a coin is collected.
    [Tooltip("Event sent when a coin is collected")]
    public UnityEvent coinCollectedEvent = new UnityEvent();

    /// Layer mask for downward castign to detect floor distance.
    public LayerMask groundLayerMask;

    /// Offset from the ground to position the coin prefab.
    public float coinGroundOffset = .2f;

    /// Distance player must exceed before respaning collectible.
    private float minRespawnDistance = 4.0f;

    /// Current coin generated.
    private CollectibleCoin currentCoin;

    /// Flag to always create the first coin.
    private bool isFirstCoin = true;

    void Start() {
      GenerateCoin();
    }

    void OnDrawGizmos() {
      Gizmos.color = Color.green;
      Gizmos.DrawWireCube(transform.position, new Vector3(.5f, .5f, .5f));
    }

    private void GenerateCoin() {
      if (coinPrefab == null) {
        Debug.LogError("Can't generate coin from null prefab");
        return;
      }

      // Don't respawn a collectible if player is nearby.
      if (isFirstCoin) {
        isFirstCoin = false;
      } else {
        float playerDistance = (player.position - transform.position).magnitude;
        if (playerDistance < minRespawnDistance) {
          StartRespawnTimer();
          return;
        }
      }

      GameObject coinGameObject = Instantiate(
        coinPrefab,
        CoinPosition(),
        Quaternion.identity,
        transform) as GameObject;
      if (coinGameObject == null) {
        Debug.LogError("Failed to instantiate coin");
        return;
      }

      currentCoin = coinGameObject.GetComponentInChildren<CollectibleCoin>();
      if (currentCoin == null) {
        Debug.LogError("Can't spawn coin that doesn't have CollectibleCoin component");
        Destroy(currentCoin);
        return;
      }

      currentCoin.onCoinCollected = CoinCollected;
    }

    // Returns best position for the coin, by locating the ground below.
    public Vector3 CoinPosition() {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayerMask)) {
        return hit.point + new Vector3(0, coinGroundOffset, 0);
      } else {
        Debug.LogError("Failed to locate ground to place coin prefab");
        return transform.position;
      }
    }

    public void StartRespawnTimer() {
      currentCoin = null;
      Invoke("GenerateCoin", spawnDelay);
    }

    private void CoinCollected(CollectibleCoin coin) {
      coinCollectedEvent.Invoke();
      StartRespawnTimer();
    }
  }
}

