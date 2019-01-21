//-----------------------------------------------------------------------
// <copyright file="CloudServiceManager.cs" company="Google">
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

namespace GoogleARCoreInternal.CrossPlatform
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using GoogleARCore.CrossPlatform;
    using UnityEngine;

    internal class CloudServiceManager
    {
        private static CloudServiceManager s_Instance;

        private List<CloudAnchorRequest> m_CloudAnchorRequests = new List<CloudAnchorRequest>();

        public static CloudServiceManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new CloudServiceManager();
                    LifecycleManager.Instance.EarlyUpdate += s_Instance._OnEarlyUpdate;
                }

                return s_Instance;
            }
        }

        public GoogleARCore.AsyncTask<CloudAnchorResult> CreateCloudAnchor(GoogleARCore.Anchor anchor)
        {
            Action<CloudAnchorResult> onComplete;
            GoogleARCore.AsyncTask<CloudAnchorResult> task;
            if (!_CreateCloudAnchorResultAsyncTask(out onComplete, out task))
            {
                return task;
            }

            _CreateCloudAnchor(onComplete, anchor.m_NativeHandle);

            return task;
        }

        public GoogleARCore.AsyncTask<CloudAnchorResult> CreateCloudAnchor(UnityEngine.Pose pose)
        {
            Action<CloudAnchorResult> onComplete;
            GoogleARCore.AsyncTask<CloudAnchorResult> task;
            if (!_CreateCloudAnchorResultAsyncTask(out onComplete, out task))
            {
                return task;
            }

            // Create an native Pose and Anchor.
            var poseHandle = LifecycleManager.Instance.NativeSession.PoseApi.Create(pose);
            IntPtr arkitAnchorHandle = IntPtr.Zero;
            ExternApi.ARKitAnchor_create(poseHandle, ref arkitAnchorHandle);

            _CreateCloudAnchor(onComplete, arkitAnchorHandle);

            // Clean up handles for the Pose and ARKitAnchor.
            LifecycleManager.Instance.NativeSession.PoseApi.Destroy(poseHandle);
            ExternApi.ARKitAnchor_release(arkitAnchorHandle);

            return task;
        }

        public GoogleARCore.AsyncTask<CloudAnchorResult> ResolveCloudAnchor(String cloudAnchorId)
        {
            Action<CloudAnchorResult> onComplete;
            GoogleARCore.AsyncTask<CloudAnchorResult> task;
            if (!_CreateCloudAnchorResultAsyncTask(out onComplete, out task))
            {
                return task;
            }

            IntPtr cloudAnchorHandle = IntPtr.Zero;
            var status = LifecycleManager.Instance.NativeSession.SessionApi
                .ResolveCloudAnchor(cloudAnchorId, out cloudAnchorHandle);

            if (status != ApiArStatus.Success)
            {
                onComplete(new CloudAnchorResult()
                {
                    Response = status.ToCloudServiceResponse(),
                    Anchor = null,
                });

                return task;
            }

            var request = new CloudAnchorRequest()
            {
                IsComplete = false,
                NativeSession = LifecycleManager.Instance.NativeSession,
                AnchorHandle = cloudAnchorHandle,
                OnTaskComplete = onComplete,
            };

            _UpdateCloudAnchorRequest(request, true);
            return task;
        }

        /// <summary>
        /// Helper for creating and initializing the Action and AsyncTask for CloudAnchorResult.
        /// </summary>
        /// <param name="onComplete">The on complete Action initialized from the AsyncTask.
        /// This will always contain a valid task even when function returns false.</param>
        /// <param name="task">The created task.
        /// This will always contain a valid task even when function returns false.</param>
        /// <returns>Returns true if cloud anchor creation should continue. Returns false if cloud creation
        /// should abort.</returns>
        protected internal bool _CreateCloudAnchorResultAsyncTask(out Action<CloudAnchorResult> onComplete,
            out GoogleARCore.AsyncTask<CloudAnchorResult> task)
        {
            // Action<CloudAnchorResult> onComplete;
            task = new GoogleARCore.AsyncTask<CloudAnchorResult>(out onComplete);

            if (LifecycleManager.Instance.NativeSession == null)
            {
                onComplete(new CloudAnchorResult()
                {
                    Response = CloudServiceResponse.ErrorNotSupportedByConfiguration,
                    Anchor = null,
                });

                return false;
            }

            return true;
        }

        /// <summary>
        /// Helper for creating a cloud anchor for the given anchor handle.
        /// </summary>
        /// <param name="onComplete">The on complete Action that was created for the
        ///  AsyncTask<CloudAnchorResult>.</param>
        /// <param name="anchorNativeHandle">The native handle for the anchor.</param>
        protected internal void _CreateCloudAnchor(Action<CloudAnchorResult> onComplete,
            IntPtr anchorNativeHandle)
        {
            IntPtr cloudAnchorHandle = IntPtr.Zero;
            var status = LifecycleManager.Instance.NativeSession.SessionApi
                .CreateCloudAnchor(anchorNativeHandle, out cloudAnchorHandle);

            if (status != ApiArStatus.Success)
            {
                onComplete(new CloudAnchorResult()
                {
                    Response = status.ToCloudServiceResponse(),
                    Anchor = null,
                });

                return;
            }

            var request = new CloudAnchorRequest()
            {
                IsComplete = false,
                NativeSession = LifecycleManager.Instance.NativeSession,
                AnchorHandle = cloudAnchorHandle,
                OnTaskComplete = onComplete,
            };

            _UpdateCloudAnchorRequest(request, true);
            return;
        }

        private void _OnEarlyUpdate()
        {
            foreach (var request in m_CloudAnchorRequests)
            {
                _UpdateCloudAnchorRequest(request);
            }

            m_CloudAnchorRequests.RemoveAll(x => x.IsComplete);
        }

        private void _UpdateCloudAnchorRequest(CloudAnchorRequest request, bool isNewRequest = false)
        {
            var cloudState = request.NativeSession.AnchorApi.GetCloudAnchorState(request.AnchorHandle);

            if (cloudState == ApiCloudAnchorState.Success)
            {
                XPAnchor xpAnchor = null;
                CloudServiceResponse response = CloudServiceResponse.Success;
                try
                {
                     xpAnchor = XPAnchor.Factory(request.NativeSession, request.AnchorHandle);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to create XP Anchor: " + e.Message);
                    response = CloudServiceResponse.ErrorInternal;
                }

                var result = new CloudAnchorResult()
                {
                    Response = response,
                    Anchor = xpAnchor,
                };

                request.OnTaskComplete(result);
                request.IsComplete = true;
            }
            else if (cloudState != ApiCloudAnchorState.TaskInProgress)
            {
                request.NativeSession.AnchorApi.Detach(request.AnchorHandle);
                request.NativeSession.AnchorApi.Release(request.AnchorHandle);

                var result = new CloudAnchorResult()
                {
                    Response = cloudState.ToCloudServiceResponse(),
                    Anchor = null
                };

                request.OnTaskComplete(result);
                request.IsComplete = true;
            }
            else if (isNewRequest)
            {
                m_CloudAnchorRequests.Add(request);
            }
        }

        private struct ExternApi
        {
            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ARKitAnchor_create(IntPtr poseHandle, ref IntPtr arkitAnchorHandle);

            [DllImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ARKitAnchor_release(IntPtr arkitAnchorHandle);
        }

        private class CloudAnchorRequest
        {
            public bool IsComplete;

            public NativeSession NativeSession;

            public IntPtr AnchorHandle;

            public Action<CloudAnchorResult> OnTaskComplete;
        }
    }
}
