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
using System.IO;
using System;

public class IconCache {

    [System.Serializable]
    public class IconEntry
    {
        public bool m_rebuild = false;
        public bool m_dirty = true;
        public string m_guid;
        public Texture2D m_icon;

        public void Remove()
        {
            FileInfo fi = new FileInfo(kPath + "/" + m_guid);
            if (fi.Exists)
            {
                fi.Delete();
            }
        }

        public void DeSerialize(FileInfo fi)
        {
            using (BinaryReader br = new BinaryReader(fi.OpenRead()))
            {
                m_guid = br.ReadString();

                int count = br.ReadInt32();

                byte[] buffer = br.ReadBytes(count);

                m_icon = new Texture2D(128, 128);
                m_icon.LoadImage(buffer);
                m_icon.Apply();
            }
        }

        public void Serialize(string path)
        {
            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(path + "/" + m_guid, FileMode.Create)))
                {
                    bw.Write(m_guid);

                    if (m_icon != null)
                    {
                        byte[] buffer = m_icon.EncodeToPNG();
                        bw.Write(buffer.Length);
                        bw.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        bw.Write(0);
                    }

                }
            } catch(Exception e)
            {
                Debug.LogError(e.StackTrace);
            }

        }
    }

    public List<IconEntry> m_iconList = new List<IconEntry>();

    const string kPath = "Library/DaydreamRenderer/Icons";

    public IconEntry Find(string guid)
    {
        for (int i = 0; i < m_iconList.Count; ++i)
        {
            if(m_iconList[i].m_guid == guid)
            {
                return m_iconList[i];
            }
        }

        return null;
    }

    public void DeSerialize()
    {
        DirectoryInfo di = new DirectoryInfo(kPath);

        if(di.Exists)
        {
            FileInfo[] fis = di.GetFiles();

            foreach(FileInfo fi in fis)
            {
                IconEntry entry = new IconEntry();
                try
                {
                    entry.m_dirty = false;
                    entry.DeSerialize(fi);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
                finally
                {
                    m_iconList.Add(entry);
                }
            }
        }
    }

    public void Serialize()
    {
        DirectoryInfo di = new DirectoryInfo(kPath);

        if (!di.Exists)
        {
            di.Create();
        }

        foreach(IconEntry entry in m_iconList)
        {
            if (entry.m_dirty)
            {
                entry.Serialize(di.FullName);
                entry.m_dirty = false;
            }
        }
    }

}
