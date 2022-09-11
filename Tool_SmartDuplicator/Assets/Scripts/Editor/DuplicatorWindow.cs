using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace sidz.tool.duplicator
{
    public class DuplicatorWindow : EditorWindow
    {
        public static EditorWindow instance = null;

        [MenuItem("DupicatorTool/My Duplicator Window", false, 1)]
        public static void ShowWindow()
        {
            instance = GetWindow(typeof(DuplicatorWindow),true,"Duplicator Window");
        }

        public void OnGUI()
        {
            // Layout the UI
        }

    }
}