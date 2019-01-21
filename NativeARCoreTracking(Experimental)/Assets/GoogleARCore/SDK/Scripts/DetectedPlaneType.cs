//-----------------------------------------------------------------------
// <copyright file="DetectedPlaneType.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore
{
    /// <summary>
    /// Simple summary of the normal vector of a plane.
    /// </summary>
    public enum DetectedPlaneType
    {
        /// <summary>
        /// A horizontal plane facing upward.
        /// </summary>
        HorizontalUpwardFacing = 0,

        /// <summary>
        /// A horizontal plane facing downward.
        /// </summary>
        HorizontalDownwardFacing = 1,

        /// <summary>
        /// A vertical plane.
        /// </summary>
        Vertical = 2,
    }
}