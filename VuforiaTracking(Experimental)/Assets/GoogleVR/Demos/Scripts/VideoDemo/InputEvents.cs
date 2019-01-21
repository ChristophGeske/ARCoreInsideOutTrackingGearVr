﻿// Copyright 2016 Google Inc. All rights reserved.
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

namespace GoogleVR.VideoDemo {
  using System;
  using UnityEngine;
  using UnityEngine.Events;

  [Serializable]
  public class Vector3Event : UnityEvent<Vector3> { }

  [Serializable]
  public class Vector2Event : UnityEvent<Vector2> { }

  [Serializable]
  public class FloatEvent : UnityEvent<float> { }

  [Serializable]
  public class BoolEvent : UnityEvent<bool> { }

  [Serializable]
  public class ButtonEvent : UnityEvent { }

  [Serializable]
  public class TouchPadEvent : UnityEvent { }

  [Serializable]
  public class TransformEvent : UnityEvent<Transform> { }

  [Serializable]
  public class GameObjectEvent : UnityEvent<GameObject> { }
}
