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

namespace DaydreamElements.Common {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  [System.Serializable]
  public class DynamicWindColorEffector {
    public Transform source;
    public float targetGrowthSpeed = 0.5f;
    [HideInInspector] public float growthSpeed = 0.0f;
    public float decaySpeed = 1.0f;
    public float maxRadius;
    float targetRadius = 0.0f;
    [HideInInspector] public float radius = 0.0f;

    public float growthSpeedPrewarm = 0.1f;

    [HideInInspector] public Vector3 position;
    Vector3 lastPosition;
    Vector3 posDelta;
    float velocity;

    //Values read by global effect system
    [HideInInspector] public Vector4 effectorProperties;

    public void DrawGizmos(){
      if(source != null) {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(source.position,maxRadius);
      }
    }

    public void Initialize() {
      lastPosition = position = source.position;
      velocity = 0.0f;
      growthSpeed = 0.0f + growthSpeedPrewarm;
    }
    
    public void UpdateEffector() {
      float dt = Time.deltaTime;

      //Calculate velocity
      position = source.position;
      posDelta = lastPosition - position;
      velocity = posDelta.magnitude;

      //Update effector radius
      velocity = Mathf.Clamp(velocity, 0.0f, 1.0f);
      targetRadius -= decaySpeed * velocity;
      growthSpeed -= decaySpeed * dt;
      growthSpeed = Mathf.Clamp(growthSpeed, 0.0f, targetGrowthSpeed);
      growthSpeed = Mathf.Lerp(growthSpeed, targetGrowthSpeed, dt);
      targetRadius += growthSpeed * dt;
      targetRadius = Mathf.Clamp(targetRadius, 0.001f, maxRadius);
      radius = Mathf.Lerp(radius, targetRadius, dt);
       
      //Save values to be passed to global shader value
      effectorProperties = new Vector4(position.x, position.y, position.z, radius);

      //Record position for velocity calculation next frame
      lastPosition = position;
    }
  }
}