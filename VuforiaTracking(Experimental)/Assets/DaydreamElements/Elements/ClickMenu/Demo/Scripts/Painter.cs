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
using System.Collections.Generic;

namespace DaydreamElements.ClickMenu {

  /// The Painter manages a collection of paint strokes and is responsible
  /// for creating, modifying, and removing the strokes.
  public class Painter : MonoBehaviour {
    private const float PENCIL_THICKNESS = 0.004f;
    private const float PEN_THICKNESS = 0.005f;
    private const float MARKER_THICKNESS = 0.015f;
    private const float ROLLER_THICKNESS = 0.08f;
    private const float MIN_VERTEX_DISPLACEMENT = 0.003f;

    public class Stroke {
      public GameObject obj;
      public List<Vector3> vertices = new List<Vector3>();
      public List<int> indices = new List<int>();
      public Material savedMat;
    }

    private Vector3 lastPoint;
    private List<Stroke> strokes;
    private List<Stroke> savedStrokes;
    private Stroke curStroke;
    private Mesh curMesh;
    private MeshRenderer curMeshRenderer;
    private float brushThickness;
    private bool useControllerAngle;
    private Material paintMaterial;

    public bool IsEraser { get; private set; }

    private MaterialPropertyBlock propertyBlock;

    public ClickMenuRoot menuRoot;
    public GvrLaserPointer laserPointer;
    public MeshRenderer reticle;

    public Material red;
    public Material orange;
    public Material yellow;
    public Material green;
    public Material blue;
    public Material purple;
    public Material white;
    public Material black;

    public Material cursorPencil;
    public Material cursorPen;
    public Material cursorHighlighter;
    public Material cursorRoller;
    public Material cursorEraser;

    private Material initialReticleMaterial;
    private Material currentBrushMaterial;

    public void UseInitialMaterial() {
      reticle.sharedMaterial = initialReticleMaterial;
    }

    public void UseBrushMaterial() {
      reticle.sharedMaterial = currentBrushMaterial;
    }

    public void SetMaterial(Material material) {
      paintMaterial = material;
      if (material && !IsEraser) {
        reticle.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", paintMaterial.color);
        reticle.SetPropertyBlock(propertyBlock);
      }
    }

    private void SetReticleMaterial(Material material, Color color) {
      reticle.sharedMaterial = material;
      currentBrushMaterial = material;
      reticle.GetPropertyBlock(propertyBlock);
      propertyBlock.SetColor("_Color", color);
      reticle.SetPropertyBlock(propertyBlock);
    }

    public void SetBrushPencil() {
      SetReticleMaterial(cursorPencil, paintMaterial.color);
      brushThickness = PENCIL_THICKNESS;
      useControllerAngle = false;
      IsEraser = false;
    }

    public void SetBrushPen() {
      SetReticleMaterial(cursorPen, paintMaterial.color);
      brushThickness = PEN_THICKNESS;
      useControllerAngle = true;
      IsEraser = false;
    }

    public void SetBrushMarker() {
      SetReticleMaterial(cursorHighlighter, paintMaterial.color);
      brushThickness = MARKER_THICKNESS;
      useControllerAngle = false;
      IsEraser = false;
    }

    public void SetBrushRoller() {
      SetReticleMaterial(cursorRoller, paintMaterial.color);
      brushThickness = ROLLER_THICKNESS;
      useControllerAngle = true;
      IsEraser = false;
    }

    public void SetBrushEraser() {
      SetReticleMaterial(cursorEraser, Color.white);
      brushThickness = 0.0f;
      useControllerAngle = false;
      IsEraser = true;
    }

    public void Undo() {
      curStroke = null;
      if (strokes.Count > 0) {
        Destroy(strokes[strokes.Count - 1].obj);
        strokes.RemoveAt(strokes.Count - 1);
      }
      System.GC.Collect();
    }

    public void Clear() {
      foreach (Stroke stroke in strokes) {
        Destroy(stroke.obj);
      }
      strokes.Clear();
      System.GC.Collect();
    }

    public void RemoveStroke(Stroke stroke) {
      Debug.Log("RemoveStroke");
      if (strokes.Remove(stroke)) {
        Destroy(stroke.obj);
        Debug.Log("DestroyStroke");
      }
    }

    public void Save() {
      savedStrokes = new List<Stroke>();
      for (int i = 0; i < strokes.Count; ++i) {
        Stroke strokeCopy = new Stroke();
        strokeCopy.indices = new List<int>(strokes[i].indices);
        strokeCopy.vertices = new List<Vector3>(strokes[i].vertices);
        strokeCopy.savedMat = strokes[i].obj.GetComponent<MeshRenderer>().sharedMaterial;
        savedStrokes.Add(strokeCopy);
      }
    }

    public void Load() {
      if (savedStrokes != null) {
        Clear();
        for (int i = 0; i < savedStrokes.Count; ++i) {
          CreateCurrentStroke();
          curMeshRenderer.sharedMaterial = savedStrokes[i].savedMat;
          curStroke.indices = new List<int>(savedStrokes[i].indices);
          curStroke.vertices = new List<Vector3>(savedStrokes[i].vertices);
          UpdateCurMesh();
          strokes.Add(curStroke);
        }
        curStroke = null;
      }
    }

    void Awake() {
      propertyBlock = new MaterialPropertyBlock();
      initialReticleMaterial = reticle.sharedMaterial;
      strokes = new List<Stroke>();
      paintMaterial = blue;
      SetBrushPencil();
      menuRoot.OnItemSelected += OnItemSelected;
    }

    private void OnItemSelected(ClickMenuItem item) {
      int id = (item ? item.id : -1);
      switch (id) {
      // Brushes
        case 1:
          SetBrushPencil();
          break;
        case 2:
          SetBrushPen();
          break;
        case 3:
          SetBrushMarker();
          break;
        case 4:
          SetBrushRoller();
          break;
        case 5:
          SetBrushEraser();
          break;

      // Colors
        case 10:
          SetMaterial(red);
          break;
        case 11:
          SetMaterial(orange);
          break;
        case 12:
          SetMaterial(yellow);
          break;
        case 13:
          SetMaterial(green);
          break;
        case 14:
          SetMaterial(blue);
          break;
        case 15:
          SetMaterial(purple);
          break;
        case 16:
          SetMaterial(white);
          break;
        case 17:
          SetMaterial(black);
          break;

      // Undo
        case 20:
          Undo();
          break;

      // Painting
        case 30:
          Save();
          break;
        case 31:
          Load();
          break;
        case 32:
          Clear();
          break;
      }
    }

    void Update() {
      // It is expected that this object will be positioned at the same location as the
      // parent of the camera.
      Transform cameraParent = Camera.main.transform.parent;
      transform.position = cameraParent.position;
      transform.rotation = cameraParent.rotation;

      // Do not allow painting if the menu system is open
      if (menuRoot.IsMenuOpen()) {
        EndStroke();
        return;
      }

      // Start, stop, or continue painting
      if (GvrControllerInput.ClickButtonDown) {
        StartStroke();
      } else if (GvrControllerInput.ClickButtonUp) {
        EndStroke();
      } else if (GvrControllerInput.ClickButton) {
        ContinueStroke();
      }
    }

    void OnDestory() {
      Clear();
    }

    private void CreateCurrentStroke() {
      curStroke = new Stroke();
      curStroke.obj = new GameObject("Stroke " + strokes.Count);
      curStroke.obj.transform.SetParent(transform, false);
      curStroke.obj.AddComponent<PainterStroke>().Init(this, curStroke);
      curMesh = curStroke.obj.GetComponent<MeshFilter>().sharedMesh;
      curMeshRenderer = curStroke.obj.GetComponent<MeshRenderer>();
      curMeshRenderer.sharedMaterial = paintMaterial;
    }

    private void StartStroke() {
      if (!IsEraser) {
        lastPoint = GetBrushPosition();
        CreateCurrentStroke();
        strokes.Add(curStroke);
      }
    }

    private void EndStroke() {
      curStroke = null;
    }

    private void ContinueStroke() {
      if (!IsEraser && curStroke != null) {
        AddNextVertex();
      }
    }

    private Vector3 GetBrushPosition() {
      GvrLaserPointer pointer = GvrPointerInputModule.Pointer as GvrLaserPointer;
      if (pointer == null) {
        return Vector3.zero;
      }

      Vector3 pointerEndPoint;
      if (pointer.CurrentRaycastResult.gameObject != null) {
        pointerEndPoint = pointer.CurrentRaycastResult.worldPosition;
      } else {
        pointerEndPoint = pointer.GetPointAlongPointer(pointer.defaultReticleDistance);
      }

      Vector3 result = transform.InverseTransformPoint(pointerEndPoint);

      return result;
    }

    private void UpdateCurMesh() {
      curMesh.vertices = curStroke.vertices.ToArray();
      curMesh.SetIndices(curStroke.indices.ToArray(), MeshTopology.Triangles, 0);
      curMesh.RecalculateBounds();
    }

    private void AddNextVertex() {
      // Check if enough movement has occurred
      Vector3 newPoint = GetBrushPosition();
      Vector3 delta = newPoint - lastPoint;
      if (delta.magnitude < MIN_VERTEX_DISPLACEMENT) {
        return;
      }

      // If this is the first time here, add, the base-vertex
      if (curStroke.vertices.Count == 0) {
        Vector3 perpLast = GetPerpVector(delta, lastPoint);
        curStroke.vertices.Add(lastPoint + perpLast);
        curStroke.vertices.Add(lastPoint - perpLast);
      }

      // Add the next vertex
      Vector3 perp = GetPerpVector(delta, newPoint);
      curStroke.vertices.Add(lastPoint + perp);
      curStroke.vertices.Add(lastPoint - perp);

      // Add the next triangles
      int vIndex = curStroke.vertices.Count;
      curStroke.indices.Add(vIndex - 1);
      curStroke.indices.Add(vIndex - 2);
      curStroke.indices.Add(vIndex - 3);
      curStroke.indices.Add(vIndex - 4);
      curStroke.indices.Add(vIndex - 3);
      curStroke.indices.Add(vIndex - 2);
      if (useControllerAngle) {
        curStroke.indices.Add(vIndex - 3);
        curStroke.indices.Add(vIndex - 2);
        curStroke.indices.Add(vIndex - 1);
        curStroke.indices.Add(vIndex - 2);
        curStroke.indices.Add(vIndex - 3);
        curStroke.indices.Add(vIndex - 4);
      }

      // Update the mesh
      UpdateCurMesh();

      // Update the last point
      lastPoint = newPoint;
    }

    private Vector3 GetPerpVector(Vector3 delta, Vector3 point) {
      Vector3 sideDir = useControllerAngle ? GvrControllerInput.Orientation * Vector3.up : delta;
      return Vector3.Cross(sideDir, point).normalized * brushThickness;
    }
  }
}
