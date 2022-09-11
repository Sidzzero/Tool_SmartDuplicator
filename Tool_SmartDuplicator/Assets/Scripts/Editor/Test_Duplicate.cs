using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.Windows;
using UnityEngine.UIElements;
namespace sidz.tool.duplicator
{
    public static class CONST_DUPLICATOR
    {
        public readonly static string[] c_skipType = new string[] { ".fbx" };
    }
    public class Test_Duplicate : MonoBehaviour
    {
        // Define this function somewhere in your editor class to make a shortcut to said hidden function
        public static bool TryGetActiveFolderPath(out string path)
        {
            //    //Soruce https://stackoverflow.com/questions/32318320/getting-path-of-right-click-in-unity-3d-editor
            var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            object[] args = new object[] { null };
            bool found = (bool)_tryGetActiveFolderPath.Invoke(null, args);
            path = (string)args[0];

            return found;
        }

        // [MenuItem("Assets/Create/SelectedDepencies")]
        public static void Editor_SelectedDepencies()
        {
            Object[] roots = new Object[] { Selection.activeGameObject };
            var selectedDepencies = EditorUtility.CollectDependencies(roots);
            foreach (var objDound in selectedDepencies)
            {
                Debug.Log(objDound.name);
            }

        }

        [MenuItem("Assets/Create/DuplicateFolder")]
        public static void Editor_SelectedStuff()
        {
            string path = "None Selected";
            if (TryGetActiveFolderPath(out path))
            {
                Debug.Log("Editor Selected Root Folder:" + path);

              //  GetAllAsset(path);
                DuplicatorWindow.ShowWindow();

                //  AssetDatabase.CreateFolder("Assets","Assets/Duplicated_Test");
                //   AssetDatabase.CopyAsset(path, "Assets/Duplicated_Test");
                //   AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log("none selected");
            }

        }

        private static void GetAllAsset(string a_strAssetRootLocation)
        {
            var assetsAtLocation = AssetDatabase.FindAssets("", new string[] { a_strAssetRootLocation });
            Dictionary<string, string> dicAllDepencies = new Dictionary<string, string>();
            Dictionary<string, List<string>> dicMainAsset = new Dictionary<string, List<string>>();
            if (assetsAtLocation != null && assetsAtLocation.Length > 0)
            {
                foreach (var assetPAthFromGuid in assetsAtLocation)
                {
                    var actualAssetPath = AssetDatabase.GUIDToAssetPath(assetPAthFromGuid);
                    var strType = AssetDatabase.GetMainAssetTypeAtPath(actualAssetPath);


                    Debug.Log("Current Asset Path:--->" + actualAssetPath + "," + strType);

                    if (strType.ToString() == "UnityEditor.DefaultAsset")
                    {
                        Debug.Log("Skipping as it;s a folder...");
                        continue;

                    }

                    var assetImporterType = AssetImporter.GetAtPath(actualAssetPath);
                    if (assetImporterType != null)
                    {

                        Debug.Log("AssetImporterType:" + assetImporterType.GetType());
                        var externalMap = assetImporterType.GetExternalObjectMap();
                        if (externalMap != null)
                        {
                            foreach (KeyValuePair<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> kv in externalMap)
                            {
                                Debug.Log(kv.Key + "," + kv.Value);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("NO AssetImporterType:" + assetImporterType.GetType());
                    }

                    //TODO: what is fileid type of class id where it is 
                    var depencies = AssetDatabase.GetDependencies(actualAssetPath, false);



                    if (depencies != null && depencies.Length > 0)
                    {
                        if (dicMainAsset.ContainsKey(actualAssetPath) == false)
                        {
                            dicMainAsset.Add(actualAssetPath, new List<string>());
                        }

                        foreach (var depPath in depencies)
                        {

                            if (Editor_CheckInRoot(a_strAssetRootLocation, depPath) == true)
                            {
                                Debug.Log("Present in Root Depencies:->" + depPath);
                                var tempGuid = AssetDatabase.GUIDFromAssetPath(depPath).ToString();
                                if (dicAllDepencies.ContainsKey(tempGuid) == false)
                                {
                                    dicAllDepencies.Add(tempGuid, depPath);
                                }
                                dicMainAsset[actualAssetPath].Add(tempGuid);


                            }
                            else
                            {
                                Debug.Log("Not Present in root Depencies:->" + depPath);
                                return;
                            }

                        }
                    }
                    else
                    {
                        Debug.Log("No Depencies");
                    }
                }
            }
            else
            {
                Debug.LogError("No asset found in sub folder:" + a_strAssetRootLocation);
            }

            foreach (KeyValuePair<string, string> kv in dicAllDepencies)
            {
                Debug.Log(kv.Key + "," + kv.Value);
            }
            foreach (KeyValuePair<string, List<string>> kv in dicMainAsset)
            {
                Debug.LogFormat("Main Asset:{0}", kv.Key);
                foreach (var data in kv.Value)
                {
                    Debug.LogFormat("---->Dep Asset:{0}", data);
                }
            }
            // Editor_ReplaceMetaFile("", "", "");
            string strNewLocation = a_strAssetRootLocation + "_Test_01";
            try
            {
                Editor_CreateDuplicateFolder(a_strAssetRootLocation, strNewLocation);
                Editor_ReReferenceAsset(a_strAssetRootLocation, strNewLocation, dicAllDepencies, dicMainAsset);
            }
            catch (System.Exception exp)
            {
                Debug.LogError("[Error]GetAllAsset->" + exp.Message);
            }
            //TODO logic
            //skip dep in other folders than assets !
            //Create folder
            //Track guid vs depencies list
            //after folder duplicate
            //serach same guid usage in new folder
            //search asset dependent files in new folder and get path(assume same root)
            //get new guid map old to new
            //run same open .meta file and update guid
            //trying to see the FBX importing 
        }


        private static void Editor_ReplaceMetaFile(string str_OldGuid, string str_NewGuid, string str_AssetPath)
        {
            // string text = System.IO.File.ReadAllText("D:\\Projects\\Unity\\Tool_SmartDuplicator\\Tool_SmartDuplicator\\Tool_SmartDuplicator\\Assets\\Character\\Materials\\Mat_Test.mat");
            string assetTextFile = System.IO.File.ReadAllText(str_AssetPath);
            Debug.LogFormat("Replace {0} with {1} in file {2}: \n {3}:", str_OldGuid, str_NewGuid, str_AssetPath, assetTextFile);
            try
            {


                //  assetTextFile = assetTextFile.Replace(str_OldGuid, str_NewGuid);
                //  Debug.Log(assetTextFile);
                //  System.IO.File.WriteAllText(str_AssetPath, assetTextFile);



            }
            catch (System.Exception exp)
            {
                AssetDatabase.StopAssetEditing();
                Debug.LogError("[Error]Editor_ReplaceMetaFile->" + exp.Message);
            }

            //save as depen asset and its depenices and path
        }
        private static void Editor_CreateDuplicateFolder(string a_strRootPath, string a_strNewLocation)
        {
            try
            {
                AssetDatabase.CopyAsset(a_strRootPath, a_strNewLocation);
                AssetDatabase.Refresh();
            }
            catch (System.Exception exp)
            {

                Debug.LogError("[Error]Editor_CreateDuplicateFolder->" + exp.Message);
            }
        }


        private static void Editor_ReReferenceAsset(
              string _a_strOldLocation,
              string a_strNewLocationRoot,
              Dictionary<string, string> a_AllDep,
              Dictionary<string, List<string>> a_MainAsset)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (KeyValuePair<string, List<string>> kv in a_MainAsset)
                {
                    foreach (var depGuid in kv.Value)
                    {
                        if (a_AllDep.ContainsKey(depGuid) == false)
                        {
                            throw new System.Exception("Missing this GUID in all dep dic:" + depGuid);
                        }
                        var depNewLocation = a_AllDep[depGuid].Replace(_a_strOldLocation, a_strNewLocationRoot);
                        var depNewGUID = AssetDatabase.AssetPathToGUID(depNewLocation);
                        if (string.IsNullOrEmpty(depNewGUID))
                        {
                            throw new System.Exception("NO valid guid found for this DEP asset at path:" + depNewLocation);
                        }
                        var mainAssetPath = kv.Key;
                        var mainAssetNewPath = mainAssetPath.Replace(_a_strOldLocation, a_strNewLocationRoot);
                        var mainNewGUId = AssetDatabase.AssetPathToGUID(mainAssetNewPath);
                        if (string.IsNullOrEmpty(mainNewGUId))
                        {
                            throw new System.Exception("NO valid guid found for this MAIN asset at path:" + mainAssetNewPath);
                        }

                        Editor_ReplaceMetaFile(depGuid, depNewGUID, mainAssetNewPath);
                    }
                }
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (System.Exception exp)
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.LogError("Editor_ReReferenceAsset:" + exp);
            }
        }
        private static bool Editor_CheckInRoot(string a_strRootDir, string a_StrFilePath)
        {
            return a_StrFilePath.Contains(a_strRootDir);


        }
    }
}