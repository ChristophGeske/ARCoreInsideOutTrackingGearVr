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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaydreamElements.ArmModels {

  [RequireComponent(typeof(MeshFilter))]
  public class Trail : MonoBehaviour {
    private struct Section {
      public Vector3 point;
      public Vector3 direction;
      public float time;
    }

    public enum Shape {
      Flat,
      Tube
    }

    public Shape shape = Shape.Flat;
    public float sectionLifetime = 0.1f;
    public float minStartVelocity = 1.0f;
    public float minStopVelocity = 0.2f;
    public float minDistance = 0.01f;
    public float minAngularVelocityX = 120.0f;
    public int maxSections = 20;
    public float width = 0.5f;
    public Color startColor = Color.blue;
    public Color endColor = Color.clear;
    public int tubeCrossSegments = 16;

    [Header("Audio")]
    public GvrAudioSource trailSound;
    public float startSoundSpeed = 2.0f;
    public float resetSoundSpeed = 1.5f;
    public float maxPitchSpeed = 5.0f;
    public float maxPitch = 1.5f;
    public float minPitch = 0.75f;

    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector3 frameVelocity;
    private Vector3 velocity;
    private Vector3 angularVelocity;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Shape lastAllocatedShape;

    private List<Section> sections;
    private List<Vector3> vertices;
    private List<Color> colors;
    private List<Vector2> uv;
    private List<int> triangles;
    private Vector3[] tubeCrossPoints;
    private Vector3[] tubeCrossPointsRotated;

    private bool canPlaySound;

    void Awake() {
      meshFilter = GetComponent<MeshFilter>();
      mesh = new Mesh();
      meshFilter.sharedMesh = mesh;
    }

    void OnEnable() {
      angularVelocity = Vector3.zero;
      velocity = Vector3.zero;
      lastPosition = transform.position;
      lastRotation = transform.rotation;
    }

    void OnDisable() {
      mesh.Clear();
      if (sections != null) {
        sections.Clear();
      }
    }

    void OnDestroy() {
      Destroy(mesh);
    }

    void LateUpdate() {
      AllocateSectionsIfNeeded();
      UpdateVelocity();

      float now = Time.time;

      // Remove old sections.
      while (sections.Count > 0 && now > (sections[0].time + sectionLifetime)) {
        sections.RemoveAt(0);
      }

      // Add a new trail section.
      if (sections.Count == 0 || ShouldCreateNewSection()) {
        Section section = new Section();
        section.point = transform.position;
        section.direction = transform.forward;
        section.time = now;

        if (sections.Count == sections.Capacity) {
          sections.RemoveAt(0);
        }

        sections.Add(section);
      }

      UpdateTrailSound();

      switch (shape) {
        case Shape.Tube:
          GenerateTubeMesh();
          break;
        case Shape.Flat:
        default:
          GenerateFlatMesh();
          break;
      }
    }

    private void AllocateSectionsIfNeeded() {
      if (sections == null || sections.Capacity != maxSections || shape != lastAllocatedShape) {
        sections = new List<Section>(maxSections);

        switch (shape) {
          case Shape.Tube:
            // Add two extra segments for the front and back caps.
            int tubeSegmentsCapacity = sections.Capacity + 2;
            int numVertices = tubeSegmentsCapacity * tubeCrossSegments;
            vertices = new List<Vector3>(numVertices);
            uv = new List<Vector2>(numVertices);
            colors = new List<Color>(numVertices);
            triangles = new List<int>(numVertices * 6);
            break;
          case Shape.Flat:
          default:
            vertices = new List<Vector3>(sections.Capacity * 2);
            colors = new List<Color>(sections.Capacity * 2);
            uv = new List<Vector2>(sections.Capacity * 2);
            triangles = new List<int>((sections.Capacity - 1) * 2 * 3);
            break;
        }

        lastAllocatedShape = shape;
      }
    }

    private void UpdateVelocity() {
      float smoothingFactor = 0.2f;

      Vector3 position = transform.position;
      Vector3 diff = position - lastPosition;
      frameVelocity = diff / Time.deltaTime;
      velocity = (frameVelocity * smoothingFactor) + (velocity * (1.0f - smoothingFactor));
      lastPosition = position;

      Quaternion rotation = transform.rotation;
      Quaternion delta = rotation * Quaternion.Inverse(lastRotation);
      float angle;
      Vector3 axis;
      delta.ToAngleAxis(out angle, out axis);
      Vector3 frameAngularVelocity = (axis * angle) / Time.deltaTime;
      angularVelocity = (frameAngularVelocity * smoothingFactor) + (angularVelocity * (1.0f - smoothingFactor));
      lastRotation = rotation;
    }

    private void UpdateTrailSound() {
      if (trailSound == null) {
        return;
      }

      float speed = velocity.magnitude;

      bool aboveStartSoundSpeed = speed >= startSoundSpeed;
      bool belowResetSoundSpeed = speed <= resetSoundSpeed;

      if (belowResetSoundSpeed) {
        canPlaySound = true;
      }

      if (aboveStartSoundSpeed && canPlaySound) {
        trailSound.Play();
        canPlaySound = false;
      }

      float pitchSpeedRange = maxPitchSpeed - startSoundSpeed;
      float ratio = Mathf.Clamp01((speed - startSoundSpeed) / pitchSpeedRange);
      float pitchRange = maxPitch - minPitch;
      float pitch = (ratio * pitchRange) + minPitch;
      trailSound.pitch = pitch;
    }

    private bool ShouldCreateNewSection() {
      Vector3 localAngularVelocity = transform.parent.InverseTransformDirection(angularVelocity);
      bool isAngularVelocityAllowed = Mathf.Abs(localAngularVelocity.x) >= minAngularVelocityX;

      if (sections.Count > 0) {
        float distanceSquared = (sections[sections.Count - 1].point - transform.position).sqrMagnitude;
        if (distanceSquared < minDistance * minDistance) {
          return false;
        }
      }

      if (sections.Count <= 1) {
        return velocity.sqrMagnitude >= minStartVelocity * minStartVelocity && isAngularVelocityAllowed;
      } else {
        return velocity.sqrMagnitude >= minStopVelocity * minStopVelocity && isAngularVelocityAllowed;
      }
    }

    private void GenerateFlatMesh() {
      mesh.Clear();
      vertices.Clear();
      colors.Clear();
      uv.Clear();
      triangles.Clear();

      int sectionsCount = sections.Count;

      // We need at least 2 sections to create the mesh.
      if (sectionsCount < 2) {
        return;
      }

      // Use matrix instead of transform.TransformPoint for performance reasons.
      Matrix4x4 localSpaceTransform = transform.worldToLocalMatrix;

      // Generate vertex, uv and colors.
      for (int i = 0; i < sectionsCount; i++) {
        Section currentSection = sections[i];

        // Calculate u for texture uv and color interpolation.
        float u = 0.0f;
        if (i != 0) {
          u = Mathf.Clamp01((Time.time - currentSection.time) / sectionLifetime);
        }

        // Calculate upwards direction.
        Vector3 direction = currentSection.direction;

        // Generate vertices.
        float halfWidth = width * 0.5f;
        vertices.Add(localSpaceTransform.MultiplyPoint(currentSection.point + direction * -halfWidth));
        vertices.Add(localSpaceTransform.MultiplyPoint(currentSection.point + direction * halfWidth));

        uv.Add(new Vector2(u, 0.0f));
        uv.Add(new Vector2(u, 1.0f));

        // Fade colors out over time.
        float lerpValue = 1.0f - (float)i / (float)sections.Count;
        Color interpolatedColor = Color.Lerp(startColor, endColor, lerpValue);
        colors.Add(interpolatedColor);
        colors.Add(interpolatedColor);
      }

      // Generate triangles indices.
      int trianglesCount = (sections.Count - 1) * 2 * 3;
      for (int i = 0; i < trianglesCount / 6; i++) {
        triangles.Add(i * 2);
        triangles.Add(i * 2 + 1);
        triangles.Add(i * 2 + 2);
        triangles.Add(i * 2 + 2);
        triangles.Add(i * 2 + 1);
        triangles.Add(i * 2 + 3);
      }

      ApplyMesh();
    }

    private void GenerateTubeMesh() {
      mesh.Clear();
      vertices.Clear();
      colors.Clear();
      uv.Clear();
      triangles.Clear();

      // We need at least 2 sections to create the mesh.
      if (sections.Count < 2) {
        return;
      }

      if (tubeCrossPoints == null || tubeCrossPoints.Length != tubeCrossSegments) {
        tubeCrossPoints = new Vector3[tubeCrossSegments];
        tubeCrossPointsRotated = new Vector3[tubeCrossSegments];
        Quaternion forwardRotation = Quaternion.LookRotation(Vector3.forward);
        float theta = 2.0f * Mathf.PI / tubeCrossSegments;
        for (int c = 0; c < tubeCrossSegments; c++) {
          tubeCrossPoints[c] = new Vector3(Mathf.Cos(theta * c), Mathf.Sin(theta * c), 0.0f) * width;
          tubeCrossPointsRotated[c] = forwardRotation * tubeCrossPoints[c];
        }
      }

      int sectionsCount = sections.Count;
      int finalSectionsCount = sectionsCount + 2;

      // Use matrix instead of transform.TransformPoint for performance reasons.
      Matrix4x4 localSpaceTransform = transform.worldToLocalMatrix;
      Quaternion identityRotation = Quaternion.identity;

      for (int i = 0; i < finalSectionsCount; i++) {
        Section currentSection;
        bool isCap = false;
        if (i == 0) {
          // Front cap.
          Section firstSection = sections[0];
          Vector3 frontOffset = (firstSection.point - sections[1].point) * 0.01f;
          currentSection = new Section();
          currentSection.point = frontOffset + firstSection.point;
          currentSection.time = firstSection.time;
          currentSection.direction = firstSection.direction;
          isCap = true;
        } else if (i == finalSectionsCount - 1) {
          // End cap.
          Section lastSection = sections[sectionsCount - 1];
          Vector3 endOffset = (lastSection.point - sections[sectionsCount - 2].point) * 0.01f;
          currentSection = new Section();
          currentSection.point = endOffset + lastSection.point;
          currentSection.time = lastSection.time;
          currentSection.direction = lastSection.direction;
          isCap = true;
        } else {
          // Regular section.
          currentSection = sections[i - 1];
        }

        float lerpValue = 1.0f - (float)i / (float)finalSectionsCount;
        lerpValue = (Time.time - currentSection.time) / sectionLifetime;
        Color interpolatedColor = Color.Lerp(startColor, endColor, lerpValue);

        // Determine the rotation of the section.
        Vector3[] crossPoints;
        if (i < sectionsCount - 1) {
          crossPoints = tubeCrossPointsRotated;
        } else {
          crossPoints = tubeCrossPoints;
        }

        Vector3 worldPoint = localSpaceTransform.MultiplyPoint(currentSection.point);

        // Calculate Vertices, UVs, and Colors for each section.
        Vector2 sectionUV = new Vector2(0.0f, i / finalSectionsCount);
        for (int c = 0; c < tubeCrossSegments; c++) {
          if (isCap) {
            vertices.Add(worldPoint);
          } else {
            vertices.Add(worldPoint + crossPoints[c]);
          }

          sectionUV.x = c / tubeCrossSegments;
          uv.Add(sectionUV);
          colors.Add(interpolatedColor);
        }

        // Assign Triangles.
        if (i > 0) {
          for (int c = 0; c < tubeCrossSegments; c++) {
            int cMod = (c + 1) % tubeCrossSegments;

            triangles.Add((i - 1) * tubeCrossSegments + c);

            int startPlusOne = (i - 1) * tubeCrossSegments + cMod;
            triangles.Add(startPlusOne);

            int startPlusTwo = i * tubeCrossSegments + c;
            triangles.Add(startPlusTwo);

            triangles.Add(startPlusTwo);
            triangles.Add(startPlusOne);
            triangles.Add(i * tubeCrossSegments + cMod);
          }
        }
      }

      ApplyMesh();
    }

    private void ApplyMesh() {
      mesh.SetVertices(vertices);
      mesh.SetColors(colors);
      mesh.SetUVs(0, uv);
      mesh.SetTriangles(triangles, 0);

      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
    }
  }
}