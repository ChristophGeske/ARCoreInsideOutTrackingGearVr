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

  public class ColorUtil {
    private static readonly int colorId = Shader.PropertyToID("_Color");
    private static MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

    public enum Type {
      Blue = 0,
      Red = 1,
      Yellow = 2,
      Green = 3,
    }
    public const int NumColors = 4;

    public static void Colorize(Type type, GameObject obj) {
      Colorize(ToColor(type), obj);
    }

    public static void Colorize(Color newColor, GameObject obj) {
      Renderer renderer = obj.GetComponent<Renderer>();
      renderer.GetPropertyBlock(materialPropertyBlock);
      materialPropertyBlock.SetColor(colorId, newColor);
      renderer.SetPropertyBlock(materialPropertyBlock);
    }

    public static Color ToColor(Type type) {
      switch (type) {
        case Type.Blue:
          return new Color32(233, 30, 99, 255);
        case Type.Red:
          return new Color32(205, 220, 57, 255);
        case Type.Yellow:
          return new Color32(103, 58, 183, 255);
        case Type.Green:
          return new Color32(0, 150, 136, 255);
        default:
          return new Color32(0, 0, 0, 255);
      }
    }

    public static Type RandomColor() {
      return (Type)Random.Range(0, NumColors);
    }
  }
}
