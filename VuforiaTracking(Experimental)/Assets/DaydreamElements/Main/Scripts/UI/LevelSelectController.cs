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
using UnityEngine.EventSystems;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

namespace DaydreamElements.Main {

  /// Controls the state of the level select menu.
  /// Toggles the menu when the App Button is clicked.
  /// Animates the menu using mecanim.
  public class LevelSelectController : MonoBehaviour {
    private static readonly float APP_LONG_PRESS_DURATION = .65f;

    /// The singleton instance of the LevelSelectController class.
    public static LevelSelectController Instance {
      get {
        return instance;
      }
    }

    private static LevelSelectController instance = null;

    public enum MenuState {
      Closing,
      Closed,
      Opening,
      Open
    }

    public delegate void OnStateChangedDelegate(MenuState currentState,MenuState previousState);

    public event OnStateChangedDelegate OnStateChanged;

    [SerializeField]
    private GameObject levelSelectMenu;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private bool startOpen = true;

    [SerializeField]
    private Renderer backgroundRenderer;

    [SerializeField]
    private Camera levelSelectCamera;

    [SerializeField]
    private GameObject pointer;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float backgroundMaxAlpha = 0.5f;

    [SerializeField]
    private float backgroundLerpSpeed = 12.0f;


    private MaterialPropertyBlock materialPropertyBlock;
    private int colorId;
    private float backgroundAlpha = 0.0f;
    private bool isTransitioningLevel;

    private const string LEVEL_SELECT_LAYER_NAME = "UI";

    public MenuState CurrentState {
      get {
        return currentState;
      }
      private set {
        if (currentState == value) {
          return;
        }

        MenuState previousState = currentState;
        currentState = value;
        if (OnStateChanged != null) {
          OnStateChanged(currentState, previousState);
        }
      }
    }

    public Camera LevelSelectCamera {
      get {
        return levelSelectCamera;
      }
    }

    public GameObject LevelSelectPointer {
      get {
        return pointer;
      }
    }

    public static int LevelSelectLayer {
      get {
        return LayerMask.NameToLayer(LEVEL_SELECT_LAYER_NAME);
      }
    }

    private MenuState currentState = MenuState.Closed;
    private int animatorOpenId;

    public void OpenMenu() {
      if (CurrentState == MenuState.Open || CurrentState == MenuState.Opening) {
        return;
      }

      ShowPointer();
      levelSelectCamera.enabled = true;

      CurrentState = MenuState.Opening;
      levelSelectMenu.SetActive(true);
      RepositionMenu();
      animator.SetBool(animatorOpenId, true);
    }

    public void CloseMenu() {
      if (CurrentState == MenuState.Closed || CurrentState == MenuState.Closing) {
        return;
      }

      CurrentState = MenuState.Closing;
      RemoveListeners();
      animator.SetBool(animatorOpenId, false);
    }

    public void CloseMenuImmediate() {
      animator.SetBool(animatorOpenId, false);
      OnCloseAnimationFinished();
    }

    public void PreFadeToLevel() {
      isTransitioningLevel = true;
      EventSystem.current.enabled = false;
    }

    public void PreLoadLevel() {
      // Pointer must be re-created for each level.
      // This is due to the lifecycle of the main camera and GvrEventSystem.
      HidePointer();
    }

    public void PostLoadLevel() {
      CloseMenuImmediate();
    }

    public void PostFadeToLevel() {
      isTransitioningLevel = false;
    }

    void Awake() {
      if (instance == null) {
        instance = this;
      } else {
        Debug.LogError("There must be only one LevelSelectController object in a scene.");
        GameObject.Destroy(gameObject);
        return;
      }

      materialPropertyBlock = new MaterialPropertyBlock();
      colorId = Shader.PropertyToID("_Color");

      DontDestroyOnLoad(gameObject);
      animatorOpenId = Animator.StringToHash("Open");

      if (startOpen) {
        OpenMenu();
      } else {
        CloseMenuImmediate();
      }
    }

    void Update() {
      if (GvrControllerInput.AppButtonDown) {
        if (CurrentState == MenuState.Open || CurrentState == MenuState.Opening) {
          LevelSelectPageProvider levelSelect = levelSelectMenu.GetComponentInChildren<LevelSelectPageProvider>();
          if (levelSelect.IsOnRootPage && CanCloseMenu()) {
            CloseMenu();
          }
        } else {
          Invoke("OpenMenu", APP_LONG_PRESS_DURATION);
        }
      } else if (GvrControllerInput.AppButtonUp) {
        CancelInvoke("OpenMenu");
      }

      if (GvrControllerInput.Recentered) {
        StartCoroutine(RepsositionMenuDelayed());
      }

      UpdateBackgroundAndIPD();
    }

    private bool CanCloseMenu() {
      return !IsInitialScene() && !isTransitioningLevel;
    }

    private IEnumerator RepsositionMenuDelayed() {
      yield return null;
      RepositionMenu();
    }

    private void RepositionMenu() {
      Camera mainCamera = Camera.main;
      Vector3 cameraPosition = mainCamera.transform.position;

      Transform mainParent = mainCamera.transform.parent;
      Transform levelParent = levelSelectCamera.transform.parent;
      levelParent.position = mainParent.position;
      levelParent.rotation = mainParent.rotation;

      Vector3 cameraRotation = mainCamera.transform.rotation.eulerAngles;
      cameraRotation.x = 0.0f;
      cameraRotation.z = 0.0f;
      Quaternion newRotation = Quaternion.Euler(cameraRotation);

      levelSelectMenu.transform.position = cameraPosition;
      levelSelectMenu.transform.rotation = newRotation;
    }

    private void UpdateBackgroundAndIPD() {
      Transform camParent = GetCameraParent();
      if (camParent == null) {
        return;
      }

      float targetIpd = 1.0f;

      // Don't modify the Ipd or turn on the background if the level select controller is
      // open in the starting scene.
      bool isInitialScene = IsInitialScene();

      bool isOpen = CurrentState == MenuState.Open || CurrentState == MenuState.Opening;
      if (isOpen && !isInitialScene) {
        targetIpd = 0.0f;
      }

      // Change the IPD of the camera by scaling it's parent.
      Vector3 currentScale = camParent.localScale;
      Vector3 targetScale = new Vector3(targetIpd, targetIpd, targetIpd);
      currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * backgroundLerpSpeed);
      camParent.localScale = currentScale;

      // Calculate what the target alpha should be.
      float alphaTarget = (1.0f - targetIpd) * backgroundMaxAlpha;
      Color color = backgroundRenderer.sharedMaterial.color;

      // Lerp the alpha.
      backgroundAlpha = Mathf.Lerp(backgroundAlpha, alphaTarget, Time.deltaTime * backgroundLerpSpeed);
      if (backgroundAlpha < 0.01f) {
        backgroundAlpha = 0.0f;
      }

      // Set the alpha on of the background.
      color.a = backgroundAlpha;
      backgroundRenderer.GetPropertyBlock(materialPropertyBlock);
      materialPropertyBlock.SetColor(colorId, color);
      backgroundRenderer.SetPropertyBlock(materialPropertyBlock);

      // Update the position of the background so that it matches the camera.
      backgroundRenderer.transform.position = Camera.main.transform.position;

      // Turn off the background if the alpha is near zero.
      backgroundRenderer.enabled = !Mathf.Approximately(backgroundAlpha, 0.0f);
    }

    private Transform GetCameraParent() {
      Camera cam = Camera.main;
      if (cam == null) {
        return null;
      }

      Transform camParent = cam.transform.parent;
      if (camParent == null) {
        return null;
      }

      // If the camera has siblings, then we need to make a new parent
      // So that we can scale the IPD without it effecting anything else.
      // We do this here so we don't need to depend on a demo scene being setup in any
      // particular way for this to work.
      if (camParent.childCount > 1) {
        GameObject parentObj = new GameObject("CameraParent");
        parentObj.transform.SetParent(camParent, false);
        cam.transform.SetParent(parentObj.transform, false);
        camParent = parentObj.transform;
      }

      return camParent;
    }

    private void OnCloseAnimationFinished() {
      if (CurrentState == MenuState.Closed) {
        return;
      }

      levelSelectMenu.SetActive(false);

      HidePointer();

      LevelSelectCamera.enabled = false;

      CurrentState = MenuState.Closed;
    }

    private void OnOpenAnimationFinished() {
      CurrentState = MenuState.Open;
      AddListeners();
    }

    private void ShowPointer() {
      pointer.SetActive(true);

      GvrLaserPointer laser = pointer.GetComponentInChildren<GvrLaserPointer>(true);
      Assert.IsNotNull(laser);
      GvrPointerInputModule.Pointer = laser;
    }

    private void HidePointer() {
      if (pointer == null) {
        return;
      }

      pointer.SetActive(false);
      if (GvrPointerInputModule.Pointer == pointer) {
        GvrPointerInputModule.Pointer = null;
      }
    }

    private bool IsInitialScene() {
      bool isInitialScene = SceneManager.GetActiveScene().buildIndex == 0;
      return isInitialScene;
    }

    private void AddListeners() {
      GvrEventExecutor executor = GvrPointerInputModule.FindEventExecutor();
      if (executor == null) {
        return;
      }

      executor.OnPointerClick += OnPointerClick;
    }

    private void RemoveListeners() {
      GvrEventExecutor executor = GvrPointerInputModule.FindEventExecutor();
      if (executor == null) {
        return;
      }

      executor.OnPointerClick -= OnPointerClick;
    }

    private void OnPointerClick(GameObject target, PointerEventData eventData) {
      if (target == null && CanCloseMenu()) {
        CloseMenu();
      }
    }
  }
}