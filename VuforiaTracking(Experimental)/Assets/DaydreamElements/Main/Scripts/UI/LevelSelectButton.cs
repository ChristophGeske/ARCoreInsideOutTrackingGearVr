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
using UnityEngine.EventSystems;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using DaydreamElements.Common;

namespace DaydreamElements.Main {

  /// View for a button that allows the player to load a level.
  [RequireComponent(typeof(Image))]
  public class LevelSelectButton : MonoBehaviour, IPointerClickHandler {
    public delegate void OnButtonClickedDelegate(LevelSelectButtonData data);
    public event OnButtonClickedDelegate OnButtonClicked;

    [SerializeField]
    private Text displayNameText;

    private LevelSelectButtonData data;

    private const string MUSIC_OBJECT_NAME = "Music";

    public void Setup(LevelSelectButtonData buttonData) {
      Assert.IsNotNull(buttonData);
      data = buttonData;

      if (displayNameText != null) {
        displayNameText.text = data.DisplayName;
      }

      if (data.BackgroundSprite != null) {
        Image image = GetComponent<Image>();
        image.sprite = data.BackgroundSprite;
      }
    }

    public void OnPointerClick(PointerEventData eventData) {
      if (data == null) {
        return;
      }

      if (OnButtonClicked != null) {
        OnButtonClicked(data);
      }

      if (!string.IsNullOrEmpty(data.SceneName)) {
        LevelSelectController.Instance.StartCoroutine(FadeToScene(data.SceneName));
      }
    }

    private IEnumerator FadeToScene(string sceneName) {
      yield return null;
      LevelSelectController.Instance.PreFadeToLevel();

      Camera levelSelectCamera = LevelSelectController.Instance.LevelSelectCamera;
      ScreenFade screenFade = levelSelectCamera.GetComponent<ScreenFade>();

      // Fade to Out
      if (screenFade != null) {
        screenFade.FadeToColor();
        yield return new WaitForSeconds(screenFade.FadeTime);
      } else {
        Debug.LogWarning("Unable to find ScreenFade.");
      }

      LevelSelectController.Instance.StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene() {
      LevelSelectController.Instance.PreLoadLevel();

      // Load the new scene asynchronously and wait until it completes.
      AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(data.SceneName);
      asyncOperation.allowSceneActivation = false;

      while(asyncOperation.progress < 0.9f) {
        yield return null;
      }

      AudioFader fader = FadeMusic();
      if (fader != null) {
        while (fader.IsFading) {
          yield return null;
        }
      }

      asyncOperation.allowSceneActivation = true;
      yield return asyncOperation;

      Camera levelSelectCamera = LevelSelectController.Instance.LevelSelectCamera;
      ScreenFade screenFade = levelSelectCamera.GetComponent<ScreenFade>();
      screenFade.FadeToClear();

      // Force garbage collection to happen before we fade
      // the new scene in to prevent frame drops and make sure the
      // fade in animation is smooth.
      screenFade.AllowFade = false;
      yield return null;
      GC.Collect();
      yield return null;
      screenFade.AllowFade = true;

      LevelSelectController.Instance.PostLoadLevel();

      bool wasEnabled = levelSelectCamera.enabled;
      levelSelectCamera.enabled = true;

      CreateLevelSelectResponder();

      while (screenFade.Color.a > 0.0f) {
        yield return null;
      }

      levelSelectCamera.enabled = wasEnabled;

      LevelSelectController.Instance.PostFadeToLevel();
    }

    private void CreateLevelSelectResponder() {
      if (data.LevelSelectResponderPrefab == null) {
        return;
      }

      GameObject responderObject = GameObject.Instantiate(data.LevelSelectResponderPrefab);
      BaseLevelSelectResponder responder = responderObject.GetComponent<BaseLevelSelectResponder>();
      Assert.IsNotNull(responder);
    }

    private AudioFader FadeMusic() {
      GameObject music = GameObject.Find(MUSIC_OBJECT_NAME);

      if (music == null) {
        return null;
      }

      AudioFader fader = music.GetComponent<AudioFader>();

      if (fader == null) {
        return null;
      }

      fader.targetVolume = 0.0f;

      return fader;
    }
  }
}