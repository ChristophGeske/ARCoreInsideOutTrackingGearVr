using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace daydreamrenderer
{
    using System;
    using UnityEditor.AnimatedValues;
    using UnityEngine.SceneManagement;
    public class MaterialConversionDialog : EditorWindow
    {

        public delegate void ResultCallback(Result result);
        public static bool m_active = false;
        public static MaterialConversionDialog m_instance = null;
        static ResultCallback m_resultCallback = null;


        static Vector2 m_scrollPosConverted;
        static Vector2 m_scrollPosSplit;

        const string kAssetPathBackup = BakeData.kDaydreamPath + "Backup";
        const string kBackupPrefix = "bak_";
        const string kStaticSuffix = "_static";
        const string kDynmcSuffix = "_dynmc";
        const int kListHeight = 600;

        static DaydreamRendererMaterialHistory s_materialHistory;
        static SerializedProperty s_list;
        static bool m_repaint = false;

        static List<Material> m_allMaterials = null;
        static Dictionary<string, List<MaterialInfo>> m_infoMap = new Dictionary<string, List<MaterialInfo>>();
        static int m_staticConvertableCount = 0;
        static int m_dynamicConvertableCount = 0;
        static int m_convertedCount = 0;
        static AnimBool m_UIFade;

        public enum Result
        {
            Remove,
            Ok,
            Cancel,
        }

        public class Styles
        {
            //public static GUIContent toggleLightingSystem = new GUIContent("Enable Daydream Lighting System", "Daydream replaces the lighting system");

            public const string kTitle = "Conversion Wizard";
            public const string kStaticConvertedMaterialListFrmt = "{0} Converted static lighting materials";
            public const string kDynamConvertedMaterialListFrmt = "{0} Converted dynamic lighting materials";
            public const string kDaydreamMaterialListFrmt = "{0} Daydream materials found";
            public const string kStaticSplitMaterialListFrmt = "{0} Split materials references. These materials were shared between a statically " +
                "lit and non-statically lit objects. The material has been copied a dynamic and static version and appended with '"+ kStaticSuffix+"/" + kDynmcSuffix+ "'.";
            public const string kMatierlConversionInfo = "From here you can convert all the materials in the project or just the current scene from 'Unity' to 'Daydream' materials. "
                + "\nThe wizard will:"
                    + "\n  *Convert Unity materials to Daydream materials (custom materials are ignored)."
                    + "\n  *Backup any material before replacing it."
                    + "\n  *Allow for reverting to backup."
                    + "\n  *Split into 'two' new materials if the material is used in static and dynamic lighting."
                    + "\n  *You can export your backups if you want an old material but don't want to revert it.";

            public static Texture2D staticMaterial = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/StaticMat.png");
            public static Texture2D dynamicMaterial = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/DynMat.png");
            public static Texture2D mixedMaterial = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/MixMat.png");
            public static Texture2D daydreamMaterial = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/DrMat.png");
            public static Texture2D notInScene = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/NotInScene.png");
            public static Texture2D notConvertible = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/NotConvertibleMat.png");

            public static GUIContent staticMatContent = new GUIContent("", "Material is used for static lit objects in the scene");
            public static GUIContent dynamicMatContent = new GUIContent("", "Material is used for dynamic lit objects in the scene");
            public static GUIContent mixedMatContent = new GUIContent("", "Material is used for dynamic and static lit objects in the scene");
            public static GUIContent daydreamMatContent = new GUIContent("", "Material is a Daydream material");
            public static GUIContent notInSceneContent = new GUIContent("", "Material is not used in the scene");
            public static GUIContent backupContent = new GUIContent("", "Material is a backup material");
            public static GUIContent notConvertibleContent = new GUIContent("", "Conversion not supported");

            public static GUIStyle staticMatIcon;
            public static GUIStyle dynmcMatIcon;
            public static GUIStyle mixedMatIcon;
            public static GUIStyle daydrmMatIcon;
            public static GUIStyle notInSceneIcon;
            public static GUIStyle notConvertibleIcon;

            public static GUIStyle[] matTypeIcons;
            public static GUIContent[] matTypeContent;

            static Styles()
            {
                const int fontSize = 8;
                staticMatIcon = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                staticMatIcon.normal.background = staticMaterial;
                staticMatIcon.normal.textColor = Color.white;
                staticMatIcon.fontSize = fontSize;

                dynmcMatIcon = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                dynmcMatIcon.normal.background = dynamicMaterial;
                dynmcMatIcon.normal.textColor = Color.white;
                dynmcMatIcon.fontSize = fontSize;

                mixedMatIcon = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                mixedMatIcon.normal.background = mixedMaterial;
                mixedMatIcon.normal.textColor = Color.white;
                mixedMatIcon.fontSize = fontSize;

                daydrmMatIcon = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                daydrmMatIcon.normal.background = daydreamMaterial;
                daydrmMatIcon.normal.textColor = Color.white;
                daydrmMatIcon.fontSize = fontSize;

                notInSceneIcon = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                notInSceneIcon.normal.background = notInScene;
                notInSceneIcon.normal.textColor = Color.white;
                notInSceneIcon.fontSize = fontSize;

                notConvertibleIcon = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                notConvertibleIcon.normal.background = notConvertible;
                notConvertibleIcon.normal.textColor = Color.white;
                notConvertibleIcon.fontSize = fontSize;

                matTypeIcons = new GUIStyle[]
                {
                    daydrmMatIcon,
                    staticMatIcon,
                    dynmcMatIcon,
                    mixedMatIcon,
                    notInSceneIcon,
                    notConvertibleIcon,
                };

                matTypeContent = new GUIContent[]
                {
                    daydreamMatContent,
                    staticMatContent,
                    dynamicMatContent,
                    mixedMatContent,
                    notInSceneContent,
                    notConvertibleContent,
                };
            }
        }

        static class StaticSceneState
        {
            public static uint kNotStatic = 1;
            public static uint kStatic = 2;
            public static uint kMixed = 3;
            public static uint kMask = 3;
        }

        public class MaterialInfo
        {
            public uint sceneType = 0;

            public MaterialInfo() { }

            public MaterialInfo(Material material)
            {
                m_material = material;
            }

            public HashSet<Renderer> GetTargets()
            {
                if (m_material == null || !m_targets.ContainsKey(m_material.name))
                {
                    return null;
                }

                return m_targets[m_material.name];
            }

            public void AddTarget(Renderer target)
            {
                if (!m_targets.ContainsKey(m_material.name))
                {
                    m_targets.Add(m_material.name, new HashSet<Renderer>());
                }

                m_targets[m_material.name].Add(target);
            }

            public Material m_material;

            // targets container is shared across material infos to capture one material used by multiple targets
            public static Dictionary<string, HashSet<Renderer>> m_targets = new Dictionary<string, HashSet<Renderer>>();
        }


        void OnEnable()
        {
            m_scrollPosConverted = Vector2.zero;
            EditorApplication.hierarchyWindowChanged += OnSceneChange;
            m_UIFade = new AnimBool(false);
            m_UIFade.valueChanged.AddListener(Repaint);
        }
        
        public static void OnSceneChange()
        {
            MaterialInfo.m_targets.Clear();
            Configure();
        }

        static void Configure()
        {
            s_materialHistory = TypeExtensions.FindOrCreateScriptableAsset<DaydreamRendererMaterialHistory>(kAssetPathBackup, "conversion_materialhistory");

            List<GameObject> roots = Utilities.GetAllRoots();
            GatherMaterials(roots.ToArray());
        }

        class MatSort : IComparer<Material>
        {
            public int Compare(Material x, Material y)
            {
                return string.Compare(x.name, y.name);
            }
        }

        private static void GatherMaterials(GameObject[] gos)
        {
            m_staticConvertableCount = 0;
            m_dynamicConvertableCount = 0;
            m_convertedCount = 0;

            m_allMaterials = new List<Material>();
            string[] allMats = AssetDatabase.FindAssets("t:material");
            foreach(string mat in allMats)
            {
                string path = AssetDatabase.GUIDToAssetPath(mat);
                Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if(m != null)
                {
                    m_allMaterials.Add(m);
                }
            }

            m_allMaterials.Sort(new MatSort());

            m_infoMap.Clear();
            GatherMaterialsForConversion(gos, ref m_infoMap);

            Dictionary<string, HashSet<Renderer>>.Enumerator dictIter = MaterialInfo.m_targets.GetEnumerator();
            while (dictIter.MoveNext())
            {
                HashSet<Renderer>.Enumerator iter = dictIter.Current.Value.GetEnumerator();
                while (iter.MoveNext())
                {
                    if (iter.Current == null) continue;

                    for (int i = 0, k = iter.Current.sharedMaterials.Length; i < k; ++i)
                    {
                        Material m = iter.Current.sharedMaterials[i];

                        if (m == null) continue;

                        if (!m.shader.name.ToLower().Contains("daydream"))
                        {
                            List<MaterialInfo> mis = null;
                            m_infoMap.TryGetValue(m.name, out mis);

                            if (IsStaticLit(iter.Current))
                            {
                                m_staticConvertableCount++;
                                if(mis != null)
                                {
                                    foreach (var mi in mis)
                                    {
                                        mi.sceneType |= StaticSceneState.kStatic;
                                    }
                                }
                            }
                            else
                            {
                                m_dynamicConvertableCount++;
                                if(mis != null)
                                {
                                    foreach (var mi in mis)
                                    {
                                        mi.sceneType |= StaticSceneState.kNotStatic;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // already a daydream material
                            List<MaterialInfo> mis = null;
                            m_infoMap.TryGetValue(m.name, out mis);

                            m_convertedCount++;

                            if (mis != null)
                            {
                                foreach (var mi in mis)
                                {
                                    mi.sceneType |= IsStaticLit(iter.Current) ? StaticSceneState.kStatic : StaticSceneState.kNotStatic;
                                }
                            }

                        }
                    }
                }
            }
        }

        public static MaterialConversionDialog ShowDialog(ResultCallback resultCallback)
        {
            if (m_instance != null)
            {
                m_instance.CloseDialog();
            }
            m_active = true;
            m_resultCallback = resultCallback;
            m_instance = EditorWindow.CreateInstance<MaterialConversionDialog>();

            Vector2 point = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            m_instance.position = new Rect(point.x, point.y, 250, 900);
            m_instance.minSize = new Vector2(250, 900);
            Configure();
            m_instance.ShowUtility();
            return m_instance;
        }

        void OnGUI()
        {

            if (s_materialHistory == null)
            {
                Configure();
            }

            //---------------------------------------------------------------------//
            // Daydream Material Wizard
            GUILayout.Space(5);
            EditorGUILayout.LabelField(Styles.kTitle, DaydreamRendererImportManager.Styles.sectionLabel, GUILayout.Height(25));
            DaydreamRendererImportManager.DrawSection(500, 1);
            GUILayout.Space(5);

            EditorGUILayout.HelpBox(Styles.kMatierlConversionInfo, MessageType.Info);

            m_UIFade.target = EditorGUILayout.Foldout(m_UIFade.target, "Legend");
            
            if (EditorGUILayout.BeginFadeGroup(m_UIFade.faded))
            {
                for (int i = 0; i < Styles.matTypeContent.Length; i += 2)
                {
                    EditorGUILayout.BeginHorizontal();

                    // icon
                    EditorGUILayout.LabelField("", Styles.matTypeIcons[i], GUILayout.Width(16));
                    // info
                    EditorGUILayout.LabelField(Styles.matTypeContent[i].tooltip);

                    if (i + 1 < Styles.matTypeContent.Length)
                    {
                        // icon
                        EditorGUILayout.LabelField("", Styles.matTypeIcons[i + 1], GUILayout.Width(16));
                        // info
                        EditorGUILayout.LabelField(Styles.matTypeContent[i + 1].tooltip);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFadeGroup();
            

            GUILayout.Space(10);


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Convert all materials used in the 'Scene'"))
            {
                List<Material> conversionList = new List<Material>();
                var iter = m_infoMap.GetEnumerator();
                while(iter.MoveNext())
                {
                    if(iter.Current.Value[0] != null && iter.Current.Value[0].sceneType > 0)
                    {
                        conversionList.Add(iter.Current.Value[0].m_material);
                    }
                }

                ConvertList(conversionList);

                if (EditorUtility.DisplayDialog(Styles.kTitle, "Would you like to bake vertex lighting now?", "Yes", "No"))
                {
                    DaydreamVertexLightingEditor.OpenWindow();
                }
                else
                {
                    // open up conversion dialog again
                    ShowDialog(null);
                }
            }

            if (GUILayout.Button("Convert all materials used in the 'Project'"))
            {
                List<Material> conversionList = new List<Material>();
                var iter = m_infoMap.GetEnumerator();
                while (iter.MoveNext())
                {
                    if (iter.Current.Value[0] != null)
                    {
                        conversionList.Add(iter.Current.Value[0].m_material);
                    }
                }

                ConvertList(conversionList);

                if (EditorUtility.DisplayDialog(Styles.kTitle, "Would you like to bake vertex lighting now?", "Yes", "No"))
                {
                    DaydreamVertexLightingEditor.OpenWindow();
                }
                else
                {
                    // open up conversion dialog again
                    ShowDialog(null);
                }
            }
            EditorGUILayout.EndHorizontal();

            DrawAllMaterials();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if(GUILayout.Button("Delete All Backup Data"))
            {
                if(EditorUtility.DisplayDialog(Styles.kTitle, "Delete all backup materials, are you sure?", "yes", "no"))
                {
                    for(int i = 0, c = s_materialHistory.m_backupMaterials.Count; i < c; ++i)
                    {
                        DestroyImmediate(s_materialHistory.m_backupMaterials[i], true);
                    }
                    s_materialHistory.m_backupMaterials.Clear();
                }
            }
            if(GUILayout.Button("Export Backups"))
            {
                List<string> assetPaths = new List<string>();
                if(!Directory.Exists(kAssetPathBackup + "/Export"))
                {
                    Directory.CreateDirectory(kAssetPathBackup + "/Export");
                }
                for (int i = 0, c = s_materialHistory.m_backupMaterials.Count; i < c; ++i)
                {
                    if(s_materialHistory.m_backupMaterials[i] != null)
                    {
                        Material copy = new Material(s_materialHistory.m_backupMaterials[i]);
                        copy.name = copy.name.Replace(kBackupPrefix, "");
                        AssetDatabase.CreateAsset(copy, kAssetPathBackup + "/Export/" + copy.name + ".mat");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        assetPaths.Add(kAssetPathBackup + "/" + copy.name + ".mat");
                    }
                }
                if(assetPaths.Count > 0)
                {
                    EditorUtility.DisplayDialog(Styles.kTitle, "Materials written to " + kAssetPathBackup+"/Export", "ok");
                }
                else
                {
                    EditorUtility.DisplayDialog(Styles.kTitle, "Nothing to export", "ok");
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (m_repaint)
            {
                List<GameObject> roots = Utilities.GetAllRoots();
                GatherMaterials(roots.ToArray());
                this.Repaint();
            }
        }

        static void DrawAllMaterials()
        {
            if (m_allMaterials == null || m_allMaterials.Count == 0) return;
            
            EditorGUILayout.Separator();
            //EditorGUILayout.LabelField(string.Format(Styles.kDynamConvertedMaterialListFrmt, m_allMaterials.Count));
            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 75f;

            m_scrollPosConverted = DrawMaterialList(m_scrollPosConverted, m_allMaterials, kListHeight);

            EditorGUILayout.EndVertical();
        }
        
        static void ConvertList(List<Material> list, bool revert = false)
        {
            EditorUtility.DisplayProgressBar(Styles.kTitle, "Converting Materials", 0f);
            try
            {
                for (int i = 0, c = list.Count; i < c; i++)
                {
                    Material material = list[i];

                    if (material == null) continue;

                    List<MaterialInfo> matInfos = null;
                    uint state = 0;

                    if (m_infoMap.TryGetValue(material.name, out matInfos) && matInfos.Count > 0)
                    {
                        state = matInfos[0].sceneType & StaticSceneState.kMask;

                    }

                    bool isBackup = false;
                    if (material.name.StartsWith(kBackupPrefix))
                    {
                        isBackup = true;
                    }

                    bool canConvert = !isBackup && !material.shader.name.ToLower().Contains("daydream");

                    if (canConvert)
                    {
                        DoConvertMaterial(state, material, matInfos[0]);
                    }
                    else if(revert)
                    {
                        DoRevertMaterial(material, matInfos[0]);
                    }

                    EditorUtility.DisplayProgressBar(Styles.kTitle, "Converting Materials", (float)i / (float)c);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

        }

        static void DoConvertMaterial(uint state, Material material, MaterialInfo matInfo)
        {
            // force repaint
            m_repaint = true;

            // --------------------------------------------//
            // Not in the scene
            if (state == 0)
            {
                MakeMaterialBackup(material);
                ConvertToDynamicLighting(material);
            }
            // --------------------------------------------//
            // Static lit
            else if (state == StaticSceneState.kStatic)
            {
                MakeMaterialBackup(material);
                ConvertToStaticLighting(material);
            }
            // --------------------------------------------//
            // Dynamic lit
            else if (state == StaticSceneState.kNotStatic)
            {

                MakeMaterialBackup(material);
                ConvertToDynamicLighting(material);
            }
            // --------------------------------------------//
            // Mixed between static and dynamic lit
            else if (state == StaticSceneState.kMixed)
            {
                // duplicate material and convert the duplicate
                Material staticLitMat = null;
                Material dynmcLitMat = null;
                MakeOrLoadSplitMaterial(material, out staticLitMat, out dynmcLitMat);

                staticLitMat = ConvertToStaticLighting(staticLitMat);
                dynmcLitMat = ConvertToDynamicLighting(dynmcLitMat);

                // update all static target sites
                HashSet<Renderer>.Enumerator iter = matInfo.GetTargets().GetEnumerator();

                while (iter.MoveNext())
                {

                    // find the split material source and replace it with the split material
                    Material[] mats = iter.Current.sharedMaterials;
                    Material[] newMats = new Material[mats.Length];
                    for (int m = 0, k = mats.Length; m < k; ++m)
                    {
                        if (mats[m].name == material.name)
                        {
                            if (IsStaticLit(iter.Current))
                            {
                                newMats[m] = staticLitMat;
                            }
                            else
                            {
                                newMats[m] = dynmcLitMat;
                            }
                        }
                        else
                        {
                            newMats[m] = mats[m];
                        }
                    }

                    iter.Current.sharedMaterials = newMats;

                    // dirty the mesh renderer
                    EditorUtility.SetDirty(iter.Current);
                    EditorUtility.SetDirty(material);
                }
            }
        }

        static Material DoRevertMaterial(Material material, MaterialInfo matInfo)
        {

            Material backupMaterial = null;
            bool splitMaterial = false;
            if (material.name.EndsWith(kStaticSuffix) || material.name.EndsWith(kDynmcSuffix))
            {
                string path = GetMaterialPath(material);
                string matName = material.name.Split(new string[] { kStaticSuffix, kDynmcSuffix}, System.StringSplitOptions.None)[0];
                backupMaterial = AssetDatabase.LoadAssetAtPath<Material>(path + "/" + matName + ".mat");
                splitMaterial = true;
            }
            else
            {
                backupMaterial = s_materialHistory.m_backupMaterials.Find(delegate (Material entry)
                {
                    return entry != null && (kBackupPrefix + material.name) == entry.name;
                });
            }

            // If a backup was found restore the material and cleanup the backup file
            if (backupMaterial != null)
            {
                if (splitMaterial)
                {
                    // update all static target sites
                    HashSet<Renderer>.Enumerator iter = matInfo.GetTargets().GetEnumerator();

                    while (iter.MoveNext())
                    {
                        // find the split material source and replace it with the split material
                        Material[] mats = iter.Current.sharedMaterials;
                        for (int m = 0, k = mats.Length; m < k; ++m)
                        {
                            if (mats[m].name == material.name)
                            {
                                mats[m] = backupMaterial;
                            }
                        }

                        iter.Current.sharedMaterials = mats;

                        // dirty the mesh renderer
                        EditorUtility.SetDirty(iter.Current);
                    }

                    // delete the old split asset
                    AssetDatabase.DeleteAsset(GetMaterialPath(material) + "/" + material.name + ".mat");
                    material = backupMaterial;
                }
                else
                {
                    material.shader = backupMaterial.shader;
                    material.CopyPropertiesFromMaterial(backupMaterial);
                    EditorUtility.SetDirty(material);
                    AssetDatabase.SaveAssets();

                    // delete backup
                    DestroyImmediate(backupMaterial, true);
                }
            }

            return material;
        }
        
        static Vector2 DrawMaterialList(Vector2 scrollPosition, List<Material> list, int listHeight)
        {
            // Split and converted materials
            int linesPerEntry = 2;
            float height = DaydreamRendererImportManager.Styles.s_defaultLabel.fixedHeight * linesPerEntry;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(listHeight));
            for (int i = 0, c = list.Count; i < c; i++)
            {
                // create a 'window' of items to actually fill in, otherwise rendering the list is too slow
                int topIndex = (int)(scrollPosition.y / height);
                int start = (int)(Mathf.Max(0f, topIndex - 4f));
                int end = (int)(Mathf.Min(list.Count - 1, topIndex + 40f));

                if (i < start || i > end)
                {
                    // fill this item with blank space to save time
                    GUILayout.Space(DaydreamRendererImportManager.Styles.s_defaultLabel.fixedHeight * linesPerEntry);
                    continue;
                }

                Material material = list[i];

                if (material == null) continue;

                EditorGUILayout.BeginHorizontal();

                // print index
                int numberWidth = (int)Mathf.Max(2f, Mathf.Log10(i)+1);
                GUILayout.Label("" + i, DaydreamRendererImportManager.Styles.s_defaultLabel, GUILayout.Width(numberWidth*10));

                EditorGUILayout.ObjectField(material, typeof(Material), true, GUILayout.Height(DaydreamRendererImportManager.Styles.s_defaultLabel.fixedHeight));

                GUIContent content = Styles.notInSceneContent;

                List<MaterialInfo> matInfos = null;
                GUIStyle elStyle = Styles.notInSceneIcon;
                uint state = 0;

                if (m_infoMap.TryGetValue(material.name, out matInfos) && matInfos.Count > 0)
                {
                    state = matInfos[0].sceneType & StaticSceneState.kMask;
                    if(state == StaticSceneState.kStatic)
                    {
                        content = Styles.staticMatContent;
                        elStyle = Styles.staticMatIcon;
                    }else if(state == StaticSceneState.kNotStatic)
                    {
                        content = Styles.dynamicMatContent;
                        elStyle = Styles.dynmcMatIcon;
                    }
                    else
                    {
                        content = Styles.mixedMatContent;
                        elStyle = Styles.mixedMatIcon;
                    }
                }

                bool enabled = true;
                if (material.name.StartsWith(kBackupPrefix))
                {
                    enabled = false;
                    content = Styles.backupContent;
                }

                bool nonDaydream = !material.shader.name.ToLower().Contains("daydream");
                
                elStyle = nonDaydream ? elStyle : Styles.daydrmMatIcon;
                content = nonDaydream ? content : Styles.daydreamMatContent;

                // if its not a daydream material check to see if it is convertible
                bool convertible = true;
                if (nonDaydream)
                {
                    convertible = DaydreamMenu.IsConvertible(material.shader);
                    elStyle = convertible ? elStyle : Styles.notConvertibleIcon;
                    content = convertible ? content : Styles.notConvertibleContent;
                }

                EditorGUILayout.LabelField(content, elStyle, GUILayout.Width(16));
                EditorGUI.BeginDisabledGroup(!enabled || !convertible);
                if (nonDaydream)
                {
                    // if it has a backup dont offer conversion
                    EditorGUI.BeginDisabledGroup(HasBackup(material));

                    if (GUILayout.Button("Convert", GUILayout.Width(80), GUILayout.Height(DaydreamRendererImportManager.Styles.s_defaultLabel.fixedHeight)))
                    {
                        // force repaint
                        m_repaint = true;

                        DoConvertMaterial(state, material, matInfos != null ? matInfos[0] : null);
                    }

                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    // if it has a backup dont offer conversion
                    EditorGUI.BeginDisabledGroup(!HasBackup(material));
                    if (GUILayout.Button("Revert", GUILayout.Width(80), GUILayout.Height(DaydreamRendererImportManager.Styles.s_defaultLabel.fixedHeight)))
                    {
                        // force repaint
                        m_repaint = true;

                        material = DoRevertMaterial(material, matInfos != null ? matInfos[0] : null);

                        // if last item is visible and we delete something Unity throws a repaint exception, trying to repaint a list item thats not there anymore
                        if (list.Count - 1 == end)
                        {
                            scrollPosition.y -= height;

                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(28);
                EditorGUILayout.LabelField("Path:", GUILayout.Width(32));
                EditorGUILayout.LabelField(GetMaterialPath(material), GUILayout.Height(DaydreamRendererImportManager.Styles.s_defaultLabel.fixedHeight));
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndScrollView();

            return scrollPosition;
        }

        static List<MaterialInfo> GatherMaterialsForConversion()
        {
            List<GameObject> roots = Utilities.GetAllRoots();
            return GatherMaterialsForConversion(roots.ToArray(), ref m_infoMap);
        }

        static List<MaterialInfo> GatherMaterialsForConversion(GameObject[] roots, ref Dictionary<string, List<MaterialInfo>> infoMap)
        {
            List<MaterialInfo> materials = new List<MaterialInfo>();
            foreach (GameObject go in roots)
            {
                Renderer[] meshes = go.GetComponentsInChildren<Renderer>();
                foreach (Renderer mr in meshes)
                {
                    foreach (Material m in mr.sharedMaterials)
                    {
                        if (m != null)// && m.shader.name != kDaydreamShader)
                        {
                            MaterialInfo matInfo = new MaterialInfo(m);
                            // record scene path
                            matInfo.AddTarget(mr);

                            materials.Add(matInfo);

                            if(!infoMap.ContainsKey(m.name))
                            {
                                infoMap.Add(m.name, new List<MaterialInfo>());
                            }
                            infoMap[m.name].Add(matInfo);
                        }
                    }

                }

            }

            return materials;
        }


        static uint GetSceneStaticState(HashSet<Renderer> renderers)
        {
            uint staticFlag = 0;
            HashSet<Renderer>.Enumerator iter = renderers.GetEnumerator();

            while (iter.MoveNext())
            {
                if (!IsStaticLit(iter.Current))
                {
                    staticFlag |= StaticSceneState.kNotStatic;
                }
                else
                {
                    staticFlag |= StaticSceneState.kStatic;
                }
            }

            return staticFlag;
        }

        static void AddVertexLightingComponent(HashSet<Renderer> renderers)
        {
            HashSet<Renderer>.Enumerator iter = renderers.GetEnumerator();

            while (iter.MoveNext())
            {
                if (IsStaticLit(iter.Current))
                {
                    if (iter.Current.gameObject.GetComponent<DaydreamVertexLighting>() == null)
                    {
                        iter.Current.gameObject.AddComponent<DaydreamVertexLighting>();
                    }
                }
            }
        }

        static bool IsStaticLit(Renderer renderer)
        {
            return ((int)StaticEditorFlags.LightmapStatic & (int)GameObjectUtility.GetStaticEditorFlags(renderer.gameObject)) == 1;
        }

        static bool IsDynamicLit(Renderer renderer)
        {
            return !renderer.sharedMaterial.shader.name.ToLower().Contains("unlit");
        }

        static bool HasBackup(Material material)
        {
            if(material != null)
            {
                // if its mixed look locally for backup
                if (material.name.EndsWith(kStaticSuffix) || material.name.EndsWith(kDynmcSuffix))
                {
                    string path = GetMaterialPath(material);
                    string mixedMatName = material.name.Split(new string[] { kStaticSuffix, kDynmcSuffix }, System.StringSplitOptions.None)[0];
                    Material backupMaterial = AssetDatabase.LoadAssetAtPath<Material>(path + "/" + mixedMatName + ".mat");
                    return backupMaterial != null;
                }

                // if its not mixed look in the backup cache
                string matName = material.name.Split(new string[] { kDynmcSuffix, kStaticSuffix }, System.StringSplitOptions.None)[0];
                return (s_materialHistory.m_backupMaterials.Exists(delegate (Material m)
                {
                    return m != null && m.name == (kBackupPrefix + matName);
                }));
            }
            return false;
        }

        static Material MakeMaterialBackup(Material material)
        {
            // the material is not already backed up
            if (!HasBackup(material))
            {
                // create asset for backup
                Material copy = new Material(material);
                copy.name = kBackupPrefix + copy.name;

                s_materialHistory.m_backupMaterials.Add(copy);
                AssetDatabase.AddObjectToAsset(copy, s_materialHistory);
                EditorUtility.SetDirty(s_materialHistory);
                AssetDatabase.SaveAssets();
            }
            
            return null;
        }

        static string GetMaterialPath(Material mat)
        {
            string assetPath = AssetDatabase.GetAssetPath(mat);
            return assetPath.Split(new string[] { "/" + mat.name + ".mat" }, System.StringSplitOptions.None)[0];
        }

        static void MakeOrLoadSplitMaterial(Material material, out Material staticMaterial, out Material dynamciMaterial)
        {
            // first backup original
            //MakeMaterialBackup(material);

            string assetPath = GetMaterialPath(material);

            // create new materials for dynamic and static versions
            if (!File.Exists(assetPath + "/" + material.name + kStaticSuffix + ".mat"))
            {
                // create asset for backup
                Material copy = new Material(material);
                AssetDatabase.CreateAsset(copy, assetPath + "/" + material.name + kStaticSuffix + ".mat");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (!File.Exists(assetPath + "/" + material.name + kDynmcSuffix + ".mat"))
            {
                // create asset for backup
                Material copy = new Material(material);
                AssetDatabase.CreateAsset(copy, assetPath + "/" + material.name + kDynmcSuffix + ".mat");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // load the split material
            staticMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath + "/" + material.name + kStaticSuffix+ ".mat");
            staticMaterial.CopyPropertiesFromMaterial(material);
            dynamciMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath + "/" + material.name + kDynmcSuffix + ".mat");
            dynamciMaterial.CopyPropertiesFromMaterial(material);
        }

        static Material ConvertToStaticLighting(Material material)
        {
            Shader destShader = null;

            Texture cubeMap = null;
            if(material.HasProperty("_Cube"))
            {
                cubeMap = material.GetTexture("_Cube");
            }
            // if there is a cube map use the reflection shader
            if (cubeMap != null)
            {
                destShader = Shader.Find("Daydream/DiffuseAndReflections");
            }
            else
            {
                destShader = Shader.Find("Daydream/Standard");
            }

            if (!DaydreamMenu.StandardToDaydreamSingleMaterial(material, material.shader, destShader))
            {
                Debug.LogWarning("No Conversion");
                material.shader = destShader;
            }

            // enable static vertex lighting
            material.EnableKeyword("STATIC_LIGHTING");
            material.DisableKeyword("LIGHTMAP");
            material.EnableKeyword("VERTEX_LIGHTING");

            EditorUtility.SetDirty(material);

            return material;
        }

        static Material ConvertToDynamicLighting(Material material)
        {
            // load main asset
            string path = AssetDatabase.GetAssetPath(material);
            Material mainMaterial = AssetDatabase.LoadMainAssetAtPath(path) as Material;

            if (mainMaterial == null)
            {
                return null;
            }

            Shader destShader = null;

            Texture cubeMap = material.GetTexture("_Cube");
            // if there is a cube map use the reflection shader
            if (cubeMap != null)
            {
                destShader = Shader.Find("Daydream/DiffuseAndReflections");
            }
            else
            {
                destShader = Shader.Find("Daydream/Standard");
            }

            if (!DaydreamMenu.StandardToDaydreamSingleMaterial(material, material.shader, destShader))
            {
                Debug.Log("No Conversion for " + material.shader.name);
            }

            EditorUtility.SetDirty(material);

            return material;
        }

        static List<GameObject> FindAllMaterialSites(string materialName)
        {
            List<GameObject> roots = Utilities.GetAllRoots();

            List<GameObject> foundObjs = new List<GameObject>();
            for (int i = 0; i < roots.Count; ++i)
            {
                Renderer[] mrs = roots[i].transform.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer mr in mrs)
                {
                    if (mr != null)
                    {
                        foreach (Material m in mr.sharedMaterials)
                        {
                            if (m != null && m.name == materialName)
                            {
                                foundObjs.Add(mr.gameObject);
                                break;
                            }
                        }
                    }
                }
            }

            return foundObjs;
        }

        public void CancelDialog()
        {
            if (m_resultCallback != null)
            {
                m_resultCallback(Result.Cancel);
            }
            CloseDialog();
        }

        void CloseDialog()
        {
            m_active = false;
            this.Close();
        }

        void OnLostFocus()
        {
            CancelDialog();
        }

    }

}
