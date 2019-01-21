using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaydreamElements.Common.IconMenu;

namespace DaydreamElements.ConstellationMenu {

  /// Works with a ConstellationMenuRoot to create a model when one is selected.
  /// Moves the model around until a button is pressed.
  public class ModelManager : MonoBehaviour {
    public ConstellationMenuRoot menuRoot;
    public GvrLaserPointer laserPointer;
    public Quaternion defaultRotation = Quaternion.AngleAxis(90.0f, Vector3.up);
    private IconValue modelScale;
    private GameObject model;

    void Start() {
      menuRoot.OnItemSelected.AddListener(OnItemSelected);
    }

    void Update() {
      if (model != null) {
        model.transform.position = laserPointer.MaxPointerEndPoint;
        model.transform.localScale = Vector3.one * modelScale.ValueAtTime(Time.time);
        if (GvrControllerInput.ClickButtonDown) {
          // release the model
          model = null;
        }
      }
      if (GvrControllerInput.AppButtonDown) {
        if (model != null) {
          Destroy(model);
          model = null;
        }
      }
    }

    /// Creates an instance of the model referenced in item.
    private void OnItemSelected(ConstellationMenuItem item) {
      if (item.prefab == null) {
        return;
      }

      Vector3 laserEnd = laserPointer.MaxPointerEndPoint;

      // Calculate the intersection of the point with the head sphere.
      Vector3 headRight = Camera.main.transform.right;
      headRight.y = 0.0f;
      headRight.Normalize();
      Vector3 cameraCenter = Camera.main.transform.position;
      Vector3 rayFromCamera = (laserEnd - cameraCenter).normalized;
      Vector3 up = Vector3.Cross(rayFromCamera, headRight);

      if (model != null) {
        Destroy(model);
      }
      model = Instantiate(item.prefab);
      model.transform.localScale = Vector3.zero;
      model.transform.position = laserEnd;
      model.transform.rotation = Quaternion.LookRotation(rayFromCamera, up) *
        defaultRotation;
      modelScale = new IconValue(0.0f);
      modelScale.FadeTo(1.0f, 0.25f, Time.time);
    }
  }
}
