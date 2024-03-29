using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LoginManager))]
public class LoginManagerEditorScript : Editor
{
    // Like update, it's called every frame, but for the editor in Unity
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("This script is responsible for connecting to Photon Servers.", MessageType.Info);
        //DrawDefaultInspector(); (Si activo, pintara dos veces la info del inspector)

        LoginManager loginManager = (LoginManager)target;
        
        // Add button to Inspector
        if (GUILayout.Button("Connect Anonymously"))
        {
            loginManager.ConnectAnonymously();
        }
    }
}
