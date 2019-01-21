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

half4 _FogColorZenith;
half4 _FogColorHorizon;
half4 _FogColorHorizonDistance;
half4 _FogDistance;

inline half4 simplefog(half3 worldPosition, half3 norm, half distance){
  half linearAlpha = saturate((distance-_FogDistance.y) * _FogDistance.x);

  half yVal = saturate(2*norm.y);
  half3 horColor = (1-linearAlpha)*_FogColorHorizon.rgb + linearAlpha*_FogColorHorizonDistance.rgb;
  half3 col = (1-yVal)*horColor + yVal * _FogColorZenith.rgb;

  linearAlpha = linearAlpha*((1-yVal) + yVal * _FogColorZenith.a);
  return half4(col,linearAlpha);
}
