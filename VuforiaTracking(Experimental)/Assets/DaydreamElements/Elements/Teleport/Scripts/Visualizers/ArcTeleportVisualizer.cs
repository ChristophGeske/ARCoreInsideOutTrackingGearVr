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

namespace DaydreamElements.Teleport {
  /// This class works with the TeleportController to render
  /// a bezier arc for showing your teleport destination. This class
  /// will generate geometry used for the arc, and calculates the 3
  /// required verticies for the bezier arc curve start/end/control.
  /// These 3 verticies are passed to the bezier arc material which
  /// uses a vertex shader to reposition the arc geometry into place.
  /// When parented under a GvrTrackedController, the arc will inherit
  /// the near clip transparency behavior of the controller.
  [RequireComponent(typeof(MeshFilter))]
  [RequireComponent(typeof(MeshRenderer))]
  public class ArcTeleportVisualizer : BaseTeleportVisualizer, IGvrArmModelReceiver {
    /// An optional game object that is placed at the origin of the laser arc.
    [Tooltip("An optional game object that is placed at the origin of the laser arc.")]
    public GameObject lineOriginPrefab;

    /// Prefab for object to place at final teleport location.
    [Tooltip("Optional target to place at end of line for valid selections.")]
    public GameObject targetPrefab;

    /// Segments in the arc mesh.
    [Tooltip("Number of steps in the arc mesh.")]
    [Range(3, 200)]
    public int segments = 100;

    /// Offset for the starting angle of the line.
    [Tooltip("Line start angle offset.")]
    public float forwardAngleOffset = -15f;

    /// Visual offset for the line origin.
    [Tooltip("Visual offset for line origin.")]
    public Vector3 lineStartOffset = new Vector3(0, -0.0099f, 0.1079f);

    /// The minimum line length when no selection is available.
    [Tooltip("Resting line length.")]
    public float minLineLength = 0f;

    /// Speed of arc length transition.
    [Tooltip("Speed of arc length transition.")]
    public float arcTransitionSpeed = 15f;

    /// Percentage of the line that is visible when there is a valid selection.
    [Tooltip("Percentage of the line that is visible when there is a valid selection.")]
    [Range(0, 1f)]
    public float maxLineVisualCompletion = 0.8f;

    /// Percentage of the line that is visible when there is a valid selection.
    [Tooltip("Percentage of the line that is visible when there is a valid selection.")]
    [Range(0, 1f)]
    public float minLineVisualCompletion = 0f;

    /// The target becomes visible when this percentage of the line is visible.
    [Tooltip("The target becomes visible when this percentage of the line is visible.")]
    [Range(0, 1f)]
    public float completionThreshold = 0.6f;

    /// Duration of target scale transition in smoothed time.
    [Tooltip("Duration of target scale transition.")]
    public float targetScaleTransitionDuration = 0.04f;

    /// Duration of lineOrigin scale transition in smoothed time.
    [Tooltip("Duration of lineOrigin scale transition.")]
    public float lineOriginScaleTransitionDuration = 0.01f;

    /// The mesh renderer for the line.
    [Tooltip("The mesh renderer used by the line.")]
    public MeshRenderer arcMeshRenderer;

    /// The mesh renderer for the line origin visual.
    [Tooltip("The mesh renderer used by the line origin visual.")]
    public MeshRenderer lineOriginRenderer;

    public GvrBaseArmModel ArmModel { get; set; }

    // Property block for material values.
    private MaterialPropertyBlock propertyBlock;

    // Reference to the arc mesh for destroying it later.
    private Mesh mesh;

    // Whether a valid selection has been made.
    private bool validHitFound;

    private int startPositionID;
    private int endPositionID;
    private int controlPositionID;
    private int completionID;
    private int alphaID;

    private float arcCompletion = 0f;

    // Instance of the target prefab.
    private GameObject target;
    // Dampened velocity for target size transitions.
    private float targetSizeVelocity = 0.0f;
    private float targetCurrentSize = 0f;
    private float targetGoalSize = 0f;

    // Instance of the line origin visual prefab.
    private GameObject lineOrigin;
    private bool lineOriginActive = true;
    // Dampened velocity for line origin visual size transitions.
    private float lineOriginSizeVelocity = 0.0f;
    private float lineOriginCurrentSize = 0f;
    private float lineOriginGoalSize = 0f;

    // The forward vector of the line origin.
    private Vector3 forward;

    // The position of the last valid selection.
    private Vector3 lastValidHitPosition;

    // Control point positions for the bezier arc.
    private Vector3 start;
    private Vector3 end;
    private Vector3 control;

    // Smoothed end point position for the bezier arc.
    private Vector3 smoothEnd;

    private const float CONTROLLER_DOWN_ANGLE_THRESHOLD = 60f;
    private const float MAX_PITCH_WITH_OFFSET = 80f;
    private const float MAX_PITCH_BLEND = 10f;
    private const float MAX_ROLL_TOLERANCE = 10f;
    private const float TARGET_MIN_SCALE_THRESHOLD = 0.01f;

    void OnValidate() {
      if (!arcMeshRenderer) {
        arcMeshRenderer = GetComponent<MeshRenderer>();
      }
    }

    void OnEnable() {
      Reset();
    }

    void Awake() {
      Reset();
      GenerateMesh();
      startPositionID = Shader.PropertyToID("_StartPosition");
      endPositionID = Shader.PropertyToID("_EndPosition");
      controlPositionID = Shader.PropertyToID("_ControlPosition");
      completionID = Shader.PropertyToID("_Completion");
      alphaID = Shader.PropertyToID("_Alpha");
      propertyBlock = new MaterialPropertyBlock();
    }

    void Start() {
    }

    void OnDestroy() {
      if (mesh != null) {
        Destroy(mesh);
      }
    }

    // Generate a mesh ribbon in the 0-1 z+ direction for BezierArcShader to manipulate.
    private void GenerateMesh() {
      if (segments == 0) {
        Debug.LogError("Can't build line mesh with 0 segments.");
        return;
      }

      int vertexRowCount = segments;
      float increment = 1 / (float)vertexRowCount;

      int vertexCount = vertexRowCount * 2; // 2 vertices per row.
      Vector3[] verticies = new Vector3[vertexCount];
      Vector2[] uvs = new Vector2[vertexCount];
      int[] triangles = new int[(vertexRowCount - 1) * 6];

      // The mesh has a width of 2 (from x -1 to 1).
      // The final width is updated by the shader.
      float width = 1;

      // Generate verticies first with z-position from 0-1.
      for (int i = 0; i < vertexRowCount; i++) {
        float zOffset = i * increment;
        int vertOffset = i * 2;
        verticies[vertOffset] = new Vector3(width, 0, zOffset); // Right vertex.
        verticies[vertOffset + 1] = new Vector3(-width, 0, zOffset); // Left vertex.

        uvs[vertOffset] = new Vector2(0f, zOffset); // Right vertex.
        uvs[vertOffset + 1] = new Vector2(1f, zOffset); // Left vertex.
      }

      // Create triangles by connecting verticies in step ahead of it.
      for (int i = 0; i < (vertexRowCount - 1); i++) {
        // Index of verticies for triangles.
        int vertexOffset = i * 2; // 2 verticies per row, so skip over previous.
        int backRight = vertexOffset;
        int backLeft = vertexOffset + 1;
        int frontRight = vertexOffset + 2;
        int frontLeft = vertexOffset + 3;

        // Right triangle.
        int triangleOffset = i * 6; // We create 4 triangles for each row.
        triangles[triangleOffset] = frontRight;
        triangles[triangleOffset + 1] = backRight;
        triangles[triangleOffset + 2] = frontLeft;

        // Left triangle.
        triangles[triangleOffset + 3] = frontLeft;
        triangles[triangleOffset + 4] = backRight;
        triangles[triangleOffset + 5] = backLeft;
      }

      // We hold onto the mesh since Unity doesn't automatically deallocate meshes.
      mesh = new Mesh ();
      GetComponent<MeshFilter>().mesh = mesh;
      mesh.vertices = verticies;
      mesh.uv = uvs;
      mesh.triangles = triangles;

      // Force the mesh to always have visible bounds.
      float boundValue = 10000;
      // Generate the bounds in local space.
      mesh.bounds = new Bounds(Vector3.zero, new Vector3(boundValue, boundValue, boundValue));
    }

    // Behavior during teleport.
    public override void OnTeleport() {
      arcMeshRenderer.enabled = false;
      Reset();
    }

    // Start teleport selection.
    public override void StartSelection(Transform controller) {
      arcMeshRenderer.enabled = true;
    }

    // End teleport selection.
    public override void EndSelection() {
      validHitFound = false;
      targetGoalSize = 0f;
    }

    // Update the visualization.
    public override void UpdateSelection(
        Transform controllerTransform,
        BaseTeleportDetector.Result selectionResult) {

      // Calculate the position for the 3 control verticies on the line.
      UpdateLine(controllerTransform,
                 selectionResult,
                 out start, out end, out control);

      // Update the target object's position at end of line.
      UpdateTarget(selectionResult);

      // Update the object used to represent the origin of the line.
      UpdateLineOrigin(selectionResult);

      // Update mesh renderers.
      UpdateMaterials(selectionResult.selectionIsValid,
        start, end, control);
    }

    private void UpdateLine(Transform controller,
        BaseTeleportDetector.Result selectionResult,
        out Vector3 start,
        out Vector3 end,
        out Vector3 control) {
      // Start point of line.
      start = controller.position +
              controller.up * lineStartOffset.y +
              controller.forward * lineStartOffset.z;

      // End line at selection or max distance.
      if (selectionResult.selectionIsValid) {
        validHitFound = true;
        end = lastValidHitPosition = selectionResult.selection;
      } else {
        // When there isn't a selection available, retract the line.
        Vector3 defaultEndPosition = start + forward * minLineLength;

        // If a valid hit was previously cached, interpolate from that position.
        if (validHitFound) {
          smoothEnd = lastValidHitPosition;
          defaultEndPosition.x = lastValidHitPosition.x;
          defaultEndPosition.z = lastValidHitPosition.z;
          validHitFound = false;
        // Otherwise, just retract the laser to its default length.
        } else {
          smoothEnd = defaultEndPosition;
        }
        
        end = Vector3.Lerp(smoothEnd, defaultEndPosition,
                           Time.deltaTime * arcTransitionSpeed);
      }

      // In order to apply an offset to the arc's start angle and have the arc
      // ignore controller roll, we need to manually recalculate controller forward.

      // Get the current forward vector of the controller.
      forward = controller.forward;
      // Get the pitch of the controller.
      float pitch = Mathf.Rad2Deg * Mathf.Asin(forward.y);
      float absPitch = Mathf.Abs(pitch);
      // Safeguards to prevent undesired behavior around rotation poles.
      if (absPitch < MAX_PITCH_WITH_OFFSET) {
        float pitchBlend = 1 - Mathf.Clamp01((absPitch - (MAX_PITCH_WITH_OFFSET - MAX_PITCH_BLEND)) / MAX_PITCH_BLEND);
        // Apply the visual offset to the pitch of the arc. Blend the offset to
        // zero as controller pitch approaches -90 or 90 degrees.
        float angleOffset = pitchBlend * forwardAngleOffset;
        float yaw = Mathf.Rad2Deg * Mathf.Atan2(forward.x, forward.z);
        float pitchRad = Mathf.Deg2Rad * (angleOffset + pitch);
        Vector3 pitchVec = new Vector3(0, Mathf.Sin(pitchRad), Mathf.Cos(pitchRad));
        // Calculate the axes of the forward vector in the appropriate order.
        forward = Quaternion.AngleAxis(yaw, Vector3.up) * pitchVec;
      }

      // Blend forward back to controller forward as the controller unrolls.
      float blend = Mathf.Clamp01(Vector3.Angle(controller.up, Vector3.up) / MAX_ROLL_TOLERANCE);
      forward = Vector3.Lerp(forward, controller.forward, blend);

      // Get control point used to bend the line into an arc.
      control = ControlPointForLine(
        start,
        end,
        forward);
    }

    private void ShowTarget() {
      // Wait until the arc is over the desired threshold to show the target.
      if (arcCompletion >= completionThreshold) {
        targetGoalSize = 1f;
      } else {
        targetGoalSize = 0f;
      }
    }

    private void HideTarget() {
      targetGoalSize = 0f;
    }

    protected void UpdateTarget(BaseTeleportDetector.Result selectionResult) {
      if (targetPrefab == null) {
        return;
      }

      if (target == null) {
        target = Instantiate(targetPrefab) as GameObject;
      }

        targetCurrentSize = Mathf.SmoothDamp(targetCurrentSize, targetGoalSize, ref targetSizeVelocity, targetScaleTransitionDuration);
        target.transform.localScale = new Vector3(targetCurrentSize, targetCurrentSize, targetCurrentSize);

      if (selectionResult.selectionIsValid) {
        target.transform.position = selectionResult.selection;
        // Make visible and grow the teleport target object when there is a valid selection.
        ShowTarget();
      } else if (validHitFound) {
        // If there isn't a valid selection, but one was previously found and cached, show the
        // target at that position instead.
        target.transform.position = lastValidHitPosition;
        HideTarget();
      } else {
        // Otherwise, just hide the teleport target.
        HideTarget();
      }
    }

    protected void UpdateLineOrigin(BaseTeleportDetector.Result selectionResult) {
      if (lineOriginPrefab == null) {
        return;
      }

      if (lineOrigin == null) {
        lineOrigin = Instantiate(lineOriginPrefab) as GameObject;
      }

      if (lineOriginRenderer == null) {
        lineOriginRenderer = lineOrigin.GetComponent<MeshRenderer>();
      }

      // Move the line origin prefab to the start of the line.
      lineOrigin.transform.position = start;

      // Show the lineOrigin if there is a valid selection.
      // Hide the lineOrigin if there is no valid selection and the line
      // is also hidden.
      if (lineOrigin && minLineLength == 0f) {
        lineOriginActive = selectionResult.selectionIsValid;
      }

      if (lineOriginActive) {
        lineOriginGoalSize = 1f;
      } else {
        lineOriginGoalSize = 0f;
      }

      lineOriginCurrentSize = Mathf.SmoothDamp(lineOriginCurrentSize, lineOriginGoalSize, ref lineOriginSizeVelocity, lineOriginScaleTransitionDuration);
      lineOrigin.transform.localScale = new Vector3(lineOriginCurrentSize, lineOriginCurrentSize, lineOriginCurrentSize);
    }

    protected void UpdateMaterials(bool isValidSelection,
        Vector3 start,
        Vector3 end,
        Vector3 control) {

      // Drive transparency with the preferred alpha from the arm model.
      float alpha = ArmModel != null ? ArmModel.PreferredAlpha : 1.0f;

      if (isValidSelection) {
        // When there is a valid selection, visually extend the arc towards it.
        arcCompletion = Mathf.Lerp(arcCompletion, maxLineVisualCompletion,
                                   Time.deltaTime * arcTransitionSpeed);
      } else {
        // When there is not a valid selection, visually retract the arc.
        arcCompletion = Mathf.Lerp(arcCompletion, minLineVisualCompletion,
                                   Time.deltaTime * arcTransitionSpeed);
      }

      propertyBlock.SetFloat(alphaID, alpha);

      if (lineOriginRenderer != null) {
        lineOriginRenderer.SetPropertyBlock(propertyBlock);
      }

      propertyBlock.SetVector(startPositionID, transform.InverseTransformPoint(start));
      propertyBlock.SetVector(endPositionID, transform.InverseTransformPoint(end));
      propertyBlock.SetVector(controlPositionID, transform.InverseTransformPoint(control));
      propertyBlock.SetFloat(completionID, arcCompletion);

      if (arcMeshRenderer != null) {
        arcMeshRenderer.SetPropertyBlock(propertyBlock);
      }
    }

    // Returns the intermediate control point in a quadratic Bezier curve.
    public static Vector3 ControlPointForLine(Vector3 start,
                                              Vector3 end,
                                              Vector3 forward) {

      Vector3 StartToEndVector = end - start;

      // As the arc forward vector approaches world down, straighten out the curve.
      float angleToDown = Vector3.Angle(forward, Vector3.down);
      float interpolation;
      interpolation = Mathf.Clamp(0, 1, Mathf.Abs(angleToDown /
                                                  CONTROLLER_DOWN_ANGLE_THRESHOLD)) *
                                        0.5f * StartToEndVector.magnitude;

      Vector3 controlPoint = start + forward * interpolation;

      return controlPoint;
    }

    public void Reset() {
      validHitFound = false;
      arcCompletion = 0f;
      targetCurrentSize = 0f;
      targetGoalSize = 0f;
      targetSizeVelocity = 0f;
      lineOriginSizeVelocity = 0f;
    }
  }
}