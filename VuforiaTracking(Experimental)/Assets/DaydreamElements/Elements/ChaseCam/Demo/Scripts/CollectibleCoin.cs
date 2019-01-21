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
using System;
using System.Collections;
using System.Collections.Generic;

namespace DaydreamElements.Chase {
  /// Class manages a collectable game coin.
  public class CollectibleCoin : MonoBehaviour {
    public delegate void CoinCollected(CollectibleCoin coin);

    /// Speed the coin rotates at.
    public float speed = 100.0f;

    // Delegate to get invoked when coin is collected.
    public CoinCollected onCoinCollected;

    /// Sound to play when collected.
    public AudioClip collectedSound;

    public GameObject particleRoot;

    /// Audio source for playing collected sound.
    private AudioSource audioSource;

    private bool isCollected;

    private float bounceTime = 0.0f;
    public float bounceDelta = 0.0f;

    private float scaleSpeed = .95f;
    private Vector3 initPosition;
    private Vector3 newPosition;

    void Start() {
      if (audioSource == null) {
        audioSource = GetComponentInChildren<AudioSource>();
      }

      initPosition = transform.position;
    }

    // Update is called once per frame
    void Update () {
      transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));

      if (isCollected==true) {
        BounceCoin();
      }
    }

    void OnTriggerEnter(Collider c) {
      if (c.GetComponent<FoxPositionedCharacter>() == null) {
        return;
      }

      if (isCollected) {
        return;
      }

      CollectCoin();
    }

    void StartParticleEffects() {
      ParticleSystem[] systems = particleRoot.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem system in systems) {
        system.Play();
      }
    }

    void CollectCoin() {
      isCollected = true;

      // Play a sound.
      if (collectedSound) {
        audioSource.PlayOneShot(collectedSound);
      }

      StartParticleEffects();

      if (onCoinCollected != null) {
        onCoinCollected(this);
      }

      Destroy(this.gameObject, 10.0f);
    }

    void BounceCoin() {
      float dt = Time.deltaTime;

      bounceTime += 3.0f * dt;
      bounceDelta = -(1.5f * bounceTime - 1.0f) * (bounceTime - 1.5f) + 1.5f;
      bounceDelta = Mathf.Clamp(bounceDelta, -0.2f, bounceDelta);
      newPosition = new Vector3(transform.position.x, initPosition.y + bounceDelta, transform.position.z);
      transform.position = newPosition;

      transform.localScale = new Vector3(
        transform.localScale.x * scaleSpeed,
        transform.localScale.y * scaleSpeed,
        transform.localScale.z * scaleSpeed);
    }
  }
}
