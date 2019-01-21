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

  public class Balloon : MonoBehaviour {
    /// Amplitude of floating motion for the balloon (meters).
    private const float FLOATING_AMPLITUDE = 0.4f;

    /// Period of floating motion for the balloon (seconds).
    private const float FLOATING_PERIOD = 12.0f;

    /// Amount of time to grow the balloon before popping (seconds).
    private const float POP_TIMER = 0.2f;

    /// Growth of the balloon in scale per second.
    private const float POP_SCALE = 0.8f;

    /// Number of exploded quads to create
    private const int NUM_EXPLODED_QUADS = 100;

    /// Number of seconds for a balloon to fully appear.
    private const float APPEARING_TIME = 0.5f;

    private Vector3 startScale;
    private Vector3 startPosition;
    private float startTime;
    private ColorUtil.Type type;
    private float appearingTimer = 0.0f;
    private bool isPopping = false;
    private bool isAppearing = true;

    public BalloonSpawner spawner;
    public int balloonIx;

    public GameObject explodedQuad;
    public GameObject popSound;

    void Start() {
      startPosition = transform.localPosition;
      startScale = transform.localScale;
      startTime = Time.realtimeSinceStartup + Random.Range(0.0f, FLOATING_PERIOD);
      type = ColorUtil.RandomColor();
      ColorUtil.Colorize(type, gameObject);
      transform.localScale = Vector3.zero;
      float randAngle = Random.Range(0.0f, 360.0f);
      transform.localRotation = Quaternion.Euler(0.0f, randAngle, 0.0f);
    }

    void Update() {
      if (isAppearing) {
        float scale = Mathf.Min(appearingTimer / APPEARING_TIME, 1.0f);
        appearingTimer += Time.deltaTime;
        transform.localScale = startScale * scale;
        if (scale >= 1.0f) {
          isAppearing = false;
        }
        ApplyFloatingAnimation();
      } else if (isPopping) {
        CreateExplosion();
        Instantiate(popSound, transform.position, Quaternion.identity);
        Destroy(gameObject);
      } else {
        ApplyFloatingAnimation();
      }
    }

    void OnDestroy() {
      Sign.IncScore();
      if (spawner != null) {
        spawner.BalloonDestroyed(balloonIx);
      }
    }

    void OnCollisionEnter(Collision collision) {
      RigidPaperAirplane rigidPencil = collision.gameObject.GetComponent<RigidPaperAirplane>();
      if (rigidPencil == null) {
        return;
      }
      if (!rigidPencil.isSpinning && rigidPencil.type == type) {
        Destroy(collision.gameObject);
        isPopping = true;
      } else {
        rigidPencil.Spin();
      }
    }

    private void ApplyFloatingAnimation() {
      float t = startTime - Time.realtimeSinceStartup;
      float w = (2 * Mathf.PI / FLOATING_PERIOD);
      float delta = Mathf.Sin(t * w) * FLOATING_AMPLITUDE;
      transform.localPosition = startPosition + Vector3.up * delta;
    }

    private void CreateExplosion() {
      SphereCollider collider = GetComponent<SphereCollider>();
      Vector3 center = transform.localToWorldMatrix.MultiplyPoint3x4(collider.center);
      for (int i = 0; i < NUM_EXPLODED_QUADS; ++i) {
        GameObject quad = Instantiate(explodedQuad);
        Vector3 delta = Random.onUnitSphere * 1.5f;
        float sx = Random.Range(0.1f, 0.5f);
        float sy = Random.Range(0.1f, 0.5f);
        quad.transform.position = center + delta;
        quad.transform.rotation = Quaternion.FromToRotation(Vector3.forward, delta);
        quad.transform.localScale = new Vector3(sx, sy, 1.0f);
        Rigidbody rigidBody = quad.GetComponent<Rigidbody>();
        float ax = Random.Range(-1.0f, 1.0f);
        float ay = Random.Range(-1.0f, 1.0f);
        float az = Random.Range(-1.0f, 1.0f);
        rigidBody.angularVelocity = new Vector3(ax, ay, az);
        rigidBody.velocity = delta * 3.0f;
        Color color = ColorUtil.ToColor(type) * Random.Range(0.5f, 1.0f);
        ColorUtil.Colorize(color, quad);
      }
    }
  }
}
