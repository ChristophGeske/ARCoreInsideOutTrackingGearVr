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

  [RequireComponent(typeof(GvrAudioSource))]
  public class PaperPlaneSpawner : MonoBehaviour {
    private const float MENU_OFFSET = 0.2f;
    private const float MIN_TIMEOUT = 0.36f;

    private float timeOut = MIN_TIMEOUT;
    private Vector3 startScale;
    private Vector3 startLocalPosition;
    private GvrAudioSource audioSource;

    public GameObject rigidPaperPlane;
    public GameObject swipeMenu;

    public GameObject launcher;
    public ColorUtil.Type type = ColorUtil.Type.Yellow;

    void Start() {
      audioSource = GetComponent<GvrAudioSource>();
      startScale = transform.localScale;
      startLocalPosition = transform.localPosition;
      ColorUtil.Colorize(type, gameObject);
      swipeMenu.GetComponent<SwipeMenu>().OnSwipeSelect += OnSwipeSelect;
    }

    void Update() {
      if (GvrControllerInput.ClickButtonDown && timeOut >= MIN_TIMEOUT) {
        audioSource.Play();
        GameObject newPaperPlane = Instantiate(rigidPaperPlane);
        newPaperPlane.GetComponent<RigidPaperAirplane>().type = type;
        newPaperPlane.transform.position = transform.position;
        newPaperPlane.transform.rotation = transform.rotation;
        Rigidbody rigidBody = newPaperPlane.GetComponent<Rigidbody>();
        rigidBody.velocity = transform.rotation * Vector3.forward * 20.0f;
        timeOut = 0.0f;
      }

      float scale = Mathf.Clamp(2.0f * timeOut / MIN_TIMEOUT - 1.0f, 0.0f, 1.0f);
      launcher.GetComponent<Launcher>().targetPull = timeOut / MIN_TIMEOUT;
      transform.localScale = startScale * scale;
      transform.localPosition = startLocalPosition + Vector3.forward * (1.0f - scale) * 0.1f;
      timeOut += Time.deltaTime;
    }

    private void OnSwipeSelect(int ix) {
      type = (ColorUtil.Type)ix;
      ColorUtil.Colorize(type, gameObject);
    }
  }
}
