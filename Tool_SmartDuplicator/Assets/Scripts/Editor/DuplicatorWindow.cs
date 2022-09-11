using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace sidz.tool.duplicator
{
    public enum eWindowState
    {
        Unintitalized,
        InitWithCorrect,
        InitWithWrong,
    }
    public class DuplicatorWindow : EditorWindow
    {
        public static EditorWindow instance = null;

        public static WindowStateManager<IWindowState> stateManager;
        public static eWindowState eWindowState = eWindowState.Unintitalized;

        private static string selectedPath = "";
        [MenuItem("DupicatorTool/My Duplicator Window", false, 1)]
        public static void ShowWindow()
        {
            selectedPath = "";
            stateManager = new WindowStateManager<IWindowState>();
        
            eWindowState = eWindowState.Unintitalized;
            instance = GetWindow(typeof(DuplicatorWindow),true,"Duplicator Window");

            
            if (TryGetActiveFolderPath(out selectedPath) == false)
            {
                eWindowState = eWindowState.InitWithWrong;
                
            }
            else
            {
                eWindowState = eWindowState.InitWithCorrect;
            }
        }


        public void OnGUI()
        {
            switch (eWindowState)
            {
                case eWindowState.Unintitalized:
                    EditorGUILayout.LabelField("Not  Initialized");
                    break;
                case eWindowState.InitWithWrong :
                    EditorGUILayout.LabelField("Wrongly Initialized");

                    break;
                case eWindowState.InitWithCorrect:
                    EditorGUILayout.LabelField("Correctly Initialized:"+selectedPath);
                    UpdateOnCorrectPath();
                    break;
                default:
                    break;
            }
        }


        private void UpdateOnCorrectPath()
        { 
        
        }
        #region HELPER
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
        #endregion

    }

    public class WindowStateManager<T> where T: IWindowState
    {
        public T currentState;
        public T PrevState;
        public void Init(List<T> a_lstStates)
        { 
        
        }
        public void ChangeState(T a_NewState)
        {
            PrevState = currentState;
            currentState = a_NewState;

            PrevState.Exit();
            currentState.Enter();
        }
    }
    public interface IWindowState
    {
        void Enter();
        void Update();
        void Exit();
    }
  
    public class CorrectWindowState : IWindowState
    {
        public void Exit()
        {
       
        }

        public void Enter()
        {
      
        }

        public void Update()
        {
            EditorGUILayout.LabelField("Proper Folder Selected in Project Heirachy");
        }

       
    }

    public class WrongWindowState : IWindowState
    {
        public void Exit()
        {
           
        }

        public void Enter()
        {
          
        }

        public void Update()
        {
            EditorGUILayout.LabelField("No Proper Folder Selected in Project Heirachy");
        }


    }
}