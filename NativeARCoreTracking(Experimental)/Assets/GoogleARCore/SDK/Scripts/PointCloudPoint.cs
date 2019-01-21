//-----------------------------------------------------------------------
// <copyright file="PointCloudPoint.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
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
    using UnityEngine;

    /// <summary>
    /// A point in a point cloud.
    /// </summary>
    public struct PointCloudPoint
    {
        /// <summary>
        /// A number that represents an invalid point id.
        /// </summary>
        public const int InvalidPointId = -1;

        /// <summary>
        /// Constructs a new PointCloudPoint.
        /// </summary>
        /// <param name="id">The id of the point within the session.</param>
        /// <param name="position">The position of the point in world space.</param>
        /// <param name="confidence">The normalized confidence of the point.</param>
        public PointCloudPoint(int id, Vector3 position, float confidence) : this()
        {
            this.Id = id;
            this.Position = position;
            this.Confidence = confidence;
        }

        /// <summary>
        /// Gets a number that identifies the point within a point cloud and ARCore session.
        ///
        /// This value is guarenteed to be unique if the ARCore session has been running for less than 24 hours.
        /// </summary>
        /// <value>A number that identifies the point within a point cloud and ARCore session.</value>
#if !UNITY_EDITOR
        public int Id { get; private set; }
#else
        public int Id
        {
            get
            {
                Debug.Log("Instant Preview does not currently support point cloud ids. " +
                    "PointCloudPoint.Id will always evaluate to 0 when accessed in the editor.");
                return 0;
            }
            private set {}
        }
#endif

        /// <summary>
        /// Gets the position of the point in world space.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Gets a normalized confidence value for the point.
        /// </summary>
        public float Confidence { get; private set; }

        /// <summary>
        /// Implicitly converts a PointCloudPoint to a Vector3 that represents its position.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        public static implicit operator Vector3(PointCloudPoint point)
        {
            return point.Position;
        }
    }
}
