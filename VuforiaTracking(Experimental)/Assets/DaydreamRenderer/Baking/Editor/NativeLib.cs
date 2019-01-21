///////////////////////////////////////////////////////////////////////////////
//Copyright 2017 Google Inc.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
///////////////////////////////////////////////////////////////////////////////

#if DDR_RUNTIME_DLL_LINKING && !UNITY_EDITOR_OSX
#define DDR_RUNTIME_DLL_LINKING_
#endif

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace daydreamrenderer
{
    public class NativeLib
    {
        public NativeLib(string libFilePath)
        {
            FileInfo fi = new FileInfo(libFilePath);
            m_libPath = fi.DirectoryName;
            m_libName = fi.Name;
            m_sourceFile = libFilePath;
            m_targetFile = m_libPath + "/" + "loaded_" + m_libName;
        }

        ~NativeLib()
        {
            InternalUnload();
        }

        public void StartFileWatcher()
        {
#if DDR_RUNTIME_DLL_LINKING_
        StopFileWatcher();
        m_fileWatch = new FileWatch();
        m_fileWatch.m_sourceFile = m_sourceFile;
        m_fileWatch.m_targetFile = m_targetFile;
        m_fileWatch.m_callback = OnChanged;
        
        // Begin watching.
        m_fileWatch.Start();
#endif
        }
        public void StopFileWatcher()
        {
#if DDR_RUNTIME_DLL_LINKING_
        if(m_fileWatch != null)
        {
            m_fileWatch.Stop();
        }
#endif
        }

        public bool LibLoaded {
            get {
#if DDR_RUNTIME_DLL_LINKING_
            return m_libHandle != IntPtr.Zero;
#else
                return true;
#endif
            }
        }

        public virtual bool LoadLib()
        {
#if DDR_RUNTIME_DLL_LINKING_
            bool res = false;
            lock (m_watchLock)
            {
                res = InternalLoadLib();
            }

            return res;
#else
            return true;
#endif
        }

        private void OnChanged()
        {
            lock (m_watchLock)
            {
                if (this != null)
                {
                    InternalUnload();
                    Thread.Sleep(300);
                    try
                    {
                        File.Copy(m_sourceFile, m_targetFile, true);
                        File.SetLastAccessTime(m_targetFile, DateTime.Now);
                        LoadLib();
                        Debug.Log("---=== " + m_libName + " Updated ===---");

                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        Thread.Sleep(1000);
                    }
                }
            }
        }


        private bool InternalLoadLib()
        {
#if UNITY_EDITOR
            if (m_libHandle == IntPtr.Zero)
            {
                m_libHandle = LoadLibrary(m_targetFile);
            }

            if (m_libHandle == IntPtr.Zero)
            {
                Debug.Log("Failed to load lib " + m_targetFile);
                return false;
            }
#endif
            return true;
        }

        public void UnloadLib()
        {
#if DDR_RUNTIME_DLL_LINKING_
        lock (m_watchLock)
        {
            InternalUnload();
        }
#endif
        }

        private void InternalUnload()
        {
            if (m_libHandle != IntPtr.Zero)
            {
                FreeLibrary(m_libHandle);
                m_libHandle = IntPtr.Zero;
            }
        }

        #region Interop methods
        protected T Invoke<T, METHOD>(params object[] pars)
        {
            T result = default(T);
            lock (m_watchLock)
            {
                if (!LibLoaded)
                {
                    Debug.LogError("Library is not loaded");
                    return default(T);
                }
                Type methodType = typeof(METHOD);
                string methodName = methodType.Name;

                IntPtr funcPtr = GetProcAddress(m_libHandle, methodName);
                if (funcPtr == IntPtr.Zero)
                {
                    Debug.LogError("Could not get proc address of " + methodType.Name);
                    return default(T);
                }

                var func = Marshal.GetDelegateForFunctionPointer(funcPtr, methodType);
                result = (T)func.DynamicInvoke(pars);
            }

            return result;
        }

        public T InvokeAsync<T, METHOD>(params object[] pars)
        {
            T result = default(T);
            if (!LibLoaded)
            {
                Debug.LogError("Library is not loaded");
                return default(T);
            }
            Type methodType = typeof(METHOD);
            string methodName = methodType.Name;

            IntPtr funcPtr = GetProcAddress(m_libHandle, methodName);
            if (funcPtr == IntPtr.Zero)
            {
                Debug.LogError("Could not get proc address of " + methodType.Name);
                return default(T);
            }

            var func = Marshal.GetDelegateForFunctionPointer(funcPtr, methodType);
            result = (T)func.DynamicInvoke(pars);

            return result;
        }

        protected void Invoke<METHOD>(params object[] pars)
        {
            lock (m_watchLock)
            {
                if (!LibLoaded)
                {
                    Debug.LogError("Library is not loaded");
                    return;
                }
                Type methodType = typeof(METHOD);
                string methodName = methodType.Name;

                IntPtr funcPtr = GetProcAddress(m_libHandle, methodName);
                if (funcPtr == IntPtr.Zero)
                {
                    Debug.LogError("Could not get proc address of " + methodType.Name);
                }

                var func = Marshal.GetDelegateForFunctionPointer(funcPtr, methodType);
                func.DynamicInvoke(pars);
            }

        }

        public class FileWatch
        {
            public Thread m_thread;
            public volatile bool m_run;
            public System.Action m_callback;
            public string m_sourceFile;
            public string m_targetFile;
            public void Start()
            {
                if (m_thread != null)
                {
                    m_run = false;
                    m_thread.Abort();
                }
                m_run = true;
                m_thread = new Thread(Watch);
                m_thread.Start();
            }
            public void Stop()
            {
                m_run = false;
            }
            public void Watch()
            {
                while (m_run)
                {
                    bool targetExists = File.Exists(m_targetFile);
                    bool sourceExists = File.Exists(m_sourceFile);
                    if ((!targetExists && sourceExists) || (targetExists && sourceExists &&
                        (File.GetLastAccessTime(m_targetFile) < File.GetLastAccessTime(m_sourceFile)
                        || File.GetLastWriteTime(m_targetFile) < File.GetLastWriteTime(m_sourceFile))))
                    {
                        if (m_callback != null)
                        {
                            Thread.Sleep(300);
                            m_callback();
                        }
                    }

                    Thread.Sleep(150);
                }
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
        #endregion

        protected string m_sourceFile;
        protected string m_targetFile;
        protected string m_libPath;
        protected string m_libName;
        protected static IntPtr m_libHandle;
        public object m_watchLock = new object();
        protected FileWatch m_fileWatch;

    }
}