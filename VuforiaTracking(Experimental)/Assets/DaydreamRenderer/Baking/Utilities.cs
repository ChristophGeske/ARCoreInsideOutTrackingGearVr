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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace daydreamrenderer
{
    public static class Utilities {

        public static List<GameObject> GetAllRoots(ref List<GameObject> roots)
        {
            int count = SceneManager.sceneCount;
            for (int s = 0; s < count; ++s)
            {
                roots.AddRange(SceneManager.GetSceneAt(s).GetRootGameObjects());
            }

            return roots;
        }

        public static List<GameObject> GetAllRoots()
        {
            List<GameObject> roots = new List<GameObject>();
            return GetAllRoots(ref roots);
        }

        public static List<GameObject> FindAll(string path)
        {
            path = path.Replace("\\", "/");

            string[] dirs = path.Split('/');

            List<GameObject> roots = GetAllRoots();

            List<GameObject> searchObjs = new List<GameObject>(roots);

            for (int j = 0; j < dirs.Length; ++j)
            {
                string pathPart = dirs[j];

                List<GameObject> foundObjs = new List<GameObject>();
                for (int i = 0; i < searchObjs.Count; ++i)
                {
                    if (searchObjs[i].name == pathPart)
                    {
                        if(j == (dirs.Length-1))
                        {
                            foundObjs.Add(searchObjs[i]);
                        }
                        else
                        {
                            for (int k = 0; k < searchObjs[i].transform.childCount; ++k)
                            {
                                foundObjs.Add(searchObjs[i].transform.GetChild(k).gameObject);
                            }
                        }

                    }
                }

                searchObjs = foundObjs;
            }

            return searchObjs;
        }

        public static Dictionary<int, Light> LightsByLocalFileId()
        {
            Dictionary<int, Light> dict = new Dictionary<int, Light>();
            List<GameObject> roots = GetAllRoots();

            foreach(GameObject root in roots)
            {
                Light[] lights = root.GetComponentsInChildren<Light>();
                for(int i = 0; i < lights.Length; ++i)
                {
                    if(lights[i] == null)
                        continue;
                    
                    int id = lights[i].GetLocalIDinFile();
                    #if UNITY_EDITOR
                    if(id == 0)
                    {
                        // if the id is 0 the scene needs to be serialized in order to generate this id
                        EditorSceneManager.SaveScene(lights[i].gameObject.scene);
                        id = lights[i].GetLocalIDinFile();
                    }
                    #endif
                    dict.Add(id, lights[i]);
                }
            }

            return dict;
        }

    }

}
