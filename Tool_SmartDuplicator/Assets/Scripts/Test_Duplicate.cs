using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class Test_Duplicate : MonoBehaviour
{
  

    // Define this function somewhere in your editor class to make a shortcut to said hidden function
    public static bool TryGetActiveFolderPath( out string path )
    {
        //    //Soruce https://stackoverflow.com/questions/32318320/getting-path-of-right-click-in-unity-3d-editor
        var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

        object[] args = new object[] { null };
        bool found = (bool)_tryGetActiveFolderPath.Invoke( null, args );
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

            GetAllAsset(path);
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
        var assetsAtLocation = AssetDatabase.FindAssets("",new string[] { a_strAssetRootLocation });
        Dictionary<string, string> dicMap = new Dictionary<string, string>();
        if (assetsAtLocation != null && assetsAtLocation.Length > 0)
        {
            foreach (var assetPAthFromGuid in assetsAtLocation)
            {
                var actualAssetPath = AssetDatabase.GUIDToAssetPath(assetPAthFromGuid);
                var strType = AssetDatabase.GetMainAssetTypeAtPath(actualAssetPath);
                
                Debug.Log("Current Asset Path:--->"+ actualAssetPath + "," + strType);

                if (strType.ToString() == "UnityEditor.DefaultAsset")
                {
                    Debug.Log("Skipping as it;s a folder...");
                    continue;
                    
                }
                //TODO: what is fileid type of class id where it is 
                 var depencies = AssetDatabase.GetDependencies(actualAssetPath,false);

             
            
                if (depencies != null && depencies.Length > 0)
                {
                    foreach (var depPath in depencies)
                    {
                     
                        if (Editor_CheckInRoot(a_strAssetRootLocation, depPath) == true)
                        {
                            Debug.Log("Present in Root Depencies:->" + depPath);
                            var tempGuid = AssetDatabase.GUIDFromAssetPath(depPath).ToString();
                            if ( dicMap.ContainsKey(tempGuid) == false )
                            {
                                dicMap.Add(tempGuid, depPath);
                            }

                        
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
            Debug.LogError("No asset found in sub folder:"+ a_strAssetRootLocation);
        }

        foreach (KeyValuePair<string , string> kv in dicMap)
        {
            Debug.Log(kv.Key+","+kv.Value);
        }

       //skip dep in other folders than assets !
        //Create folder
        //Track guid vs depencies list
        //after folder duplicate
        //serach same guid usage in new folder
        //search asset dependent files in new folder and get path(assume same root)
        //get new guid map old to new
        //run same open .meta file and update guid
    }

    private static bool Editor_CheckInRoot(string a_strRootDir , string a_StrFilePath)
    {
       return a_StrFilePath.Contains(a_strRootDir);


    }
}
