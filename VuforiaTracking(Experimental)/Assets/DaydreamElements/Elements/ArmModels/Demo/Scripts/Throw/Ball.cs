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

  using System;
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  /// Ball used to demo Throwing objects.
  [RequireComponent(typeof(SphereCollider))]
  public class Ball : Throwable {
    [Tooltip("Material to use when the ball is highlighted.")]
    public Material outlineMaterial;

    [Tooltip("Sound played when the ball is taken.")]
    public GvrAudioSource takenSound;

    [Tooltip("Audio Played when the ball is thrown.")]
    public GvrAudioSource throwSound;

    [Tooltip("A random sound from this list is played when the ball bounces.")]
    public GvrAudioSource[] bounceSounds;

    private SphereCollider sphereCollider;
    private Trail trail;

    public FadeControllerVisual controllerVisual { get; private set; }

    public bool Ready { get; private set; }

    private MeshRenderer meshRenderer;
    private Coroutine spawnAnimationCoroutine;
    private Material initialMaterial;
    private Vector3 initialScale;
    private bool isTaken;
    private float lastBounceTime;

    private const float BOUNCE_SOUND_DELAY = 0.1f;
    private const float MIN_BOUNCE_VELOCITY = 0.5f;

    public override void Throw(Vector3 force) {
      base.Throw(force);

      if (controllerVisual != null) {
        controllerVisual.ArmModel = null;
      }

      trail.enabled = true;

      if (throwSound != null) {
        throwSound.Play();
      }
    }

    public void Reset() {
      Hold();

      if (controllerVisual != null) {
        controllerVisual.ArmModel = null;
      }

      if (spawnAnimationCoroutine != null) {
        StopCoroutine(spawnAnimationCoroutine);
        spawnAnimationCoroutine = null;
      }

      trail.enabled = false;

      if (meshRenderer != null) {
        meshRenderer.sharedMaterial = initialMaterial;
      }

      isTaken = false;
      Ready = false;
    }

    public void PlaySpawnAnimation() {
      if (spawnAnimationCoroutine != null) {
        StopCoroutine(spawnAnimationCoroutine);
      }

      spawnAnimationCoroutine = StartCoroutine(SpawnAnimation());
    }

    public void OnTaken() {
      if (meshRenderer != null) {
        meshRenderer.sharedMaterial = initialMaterial;
      }
      isTaken = true;

      if (takenSound != null) {
        takenSound.Play();
      }
    }

    public void SetHighlightEnabled(bool enabled) {
      if (isTaken || meshRenderer == null) {
        return;
      }

      if (enabled && outlineMaterial != null) {
        meshRenderer.sharedMaterial = outlineMaterial;
      } else {
        meshRenderer.sharedMaterial = initialMaterial;
      }
    }

    protected override void Awake() {
      base.Awake();

      sphereCollider = GetComponent<SphereCollider>();
      controllerVisual = GetComponentInChildren<FadeControllerVisual>();
      meshRenderer = GetComponentInChildren<MeshRenderer>();
      trail = GetComponentInChildren<Trail>();

      initialScale = transform.localScale;
      initialMaterial = meshRenderer.sharedMaterial;
    }

    void OnEnable() {
      Reset();

      // If the ball is disabled while playing it's spawn animation, it's spawn animation will
      // be cancelled in the middle and it will be left in a bad state.
      // This can happen when switching between Arm Model modes. To fix this, simply replay the spawn
      // animation when the ball is enabled.
      PlaySpawnAnimation();
    }

    void OnCollisionEnter() {
      // Only play sounds if the ball has already been thrown.
      if (rigidBody.isKinematic == false && bounceSounds.Length != 0) {
        if (Time.time > lastBounceTime + BOUNCE_SOUND_DELAY
          && rigidBody.velocity.magnitude > MIN_BOUNCE_VELOCITY) {
          int soundIndex = UnityEngine.Random.Range(0, bounceSounds.Length);
          bounceSounds[soundIndex].Play();
          lastBounceTime = Time.time;
        }
      }
    }

    private IEnumerator SpawnAnimation() {
      Ready = false;

      sphereCollider.enabled = false;
      transform.localScale = Vector3.zero;

      // Add a short delay before the ball scales in so that the animation feels less jarring.
      yield return new WaitForSeconds(0.5f);

      sphereCollider.enabled = true;

      const float duration = 0.8f;
      const float readyDuration = 0.2f;
      float elapsedTime = 0.0f;
      while (elapsedTime < duration) {
        elapsedTime += Time.deltaTime;
        float ratio = Mathf.Clamp01(elapsedTime / duration);
        transform.localScale = initialScale * EasingFunctions.RatioEaseOutElastic(ratio);

        if (elapsedTime >= readyDuration) {
          Ready = true;
        }
        yield return null;
      }

      transform.localScale = initialScale;
    }
  }
}