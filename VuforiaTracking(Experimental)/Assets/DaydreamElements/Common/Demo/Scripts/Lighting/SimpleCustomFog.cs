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

  using UnityEngine;
  using System.Collections;

  [ExecuteInEditMode]
  public class SimpleCustomFog : MonoBehaviour {

    [SerializeField] public Fog fog;

    void LateUpdate () {
      Shader.SetGlobalVector("_FogDistance", new Vector4(1f/(fog.end - fog.start),fog.start,0,0));
      Shader.SetGlobalVector("_FogColorZenith", fog.zenithColor);
      Shader.SetGlobalVector("_FogColorHorizon", fog.horizonColor);
      Shader.SetGlobalVector("_FogColorHorizonDistance", fog.horizonColorDistance);
    }

    [System.Serializable]
    public struct Fog{

      public float start;
      public float end;
      public Color horizonColor;
      public Color horizonColorDistance;
      public Color zenithColor;

      public static Fog operator +(Fog a1, Fog a2){
        Fog a = new Fog();
        a.start = a1.start + a2.start;
        a.end = a1.end + a2.end;
        a.horizonColor = a1.horizonColor + a2.horizonColor;
        a.horizonColorDistance = a1.horizonColorDistance + a2.horizonColorDistance;
        a.zenithColor = a1.zenithColor + a2.zenithColor;

        return a;
      }
      public static Fog operator *(float scaler, Fog a1){
        Fog a = new Fog();
        a.start = scaler *a1.start;
        a.end = scaler *a1.end;
        a.horizonColor = scaler *a1.horizonColor;
        a.horizonColorDistance = scaler *a1.horizonColorDistance;
        a.zenithColor = scaler *a1.zenithColor;

        return a;
      }
    }
  }
}