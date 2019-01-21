//-----------------------------------------------------------------------
// <copyright file="ExperimentManager.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using GoogleARCore;

    internal class ExperimentManager
    {
        private static ExperimentManager s_Instance;
        private List<ExperimentBase> m_Experiments;

        public ExperimentManager()
        {
            // Experiments all derive from ExperimentBase to get hooks to the internal
            // state. Find and hook them up.
            m_Experiments = new List<ExperimentBase>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> allTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var assemblyTypes = assembly.GetTypes();
                    allTypes.AddRange(assemblyTypes);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    UnityEngine.Debug.Log("Unable to load types from assembly:: " + assembly.ToString() + ":: "
                        + ex.Message);
                }
            }

            foreach (var type in allTypes)
            {
                if (!type.IsClass || type.IsAbstract || !typeof(ExperimentBase).IsAssignableFrom(type))
                {
                    continue;
                }

                m_Experiments.Add(Activator.CreateInstance(type) as ExperimentBase);
            }
        }

        private delegate void OnBeforeSetConfigurationCallback(IntPtr sessionHandhle, IntPtr configHandle);

        public static ExperimentManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ExperimentManager();
                    LifecycleManager.Instance.EarlyUpdate += s_Instance._OnEarlyUpdate;
                }

                return s_Instance;
            }
        }

        public bool IsSessionExperimental { get; private set; }

        public bool IsConfigurationDirty
        {
            get
            {
                bool result = false;

                foreach (var experiment in m_Experiments)
                {
                    result = result || experiment.IsConfigurationDirty();
                }

                return result;
            }
        }

        public void OnBeforeSetConfiguration(IntPtr sessionHandle, IntPtr configHandle)
        {
            foreach (var experiment in m_Experiments)
            {
                experiment.OnBeforeSetConfiguration(sessionHandle, configHandle);
            }
        }

        public bool IsManagingTrackableType(int trackableType)
        {
            return _GetTrackableTypeManager(trackableType) != null;
        }

        public Trackable TrackableFactory(int trackableType, IntPtr trackableHandle)
        {
            ExperimentBase trackableManager = _GetTrackableTypeManager(trackableType);
            if (trackableManager != null)
            {
                return trackableManager.TrackableFactory(trackableType, trackableHandle);
            }

            throw new NotImplementedException(
                    "ExperimentManager.TrackableFactory::No constructor for requested trackable type.");
        }

        private void _OnEarlyUpdate()
        {
            foreach (var experiment in m_Experiments)
            {
                experiment.OnEarlyUpdate();
            }
        }

        private ExperimentBase _GetTrackableTypeManager(int trackableType)
        {
            foreach (var experiment in m_Experiments)
            {
                if (experiment.IsManagingTrackableType(trackableType))
                {
                    return experiment;
                }
            }

            return null;
        }
    }
}
