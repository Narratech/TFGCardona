using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MultiplayerRoomManager))]
public class RoomManagerEditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("This script is responsible for creating and joining rooms", MessageType.Info);
        base.OnInspectorGUI();

        MultiplayerRoomManager roomManager = (MultiplayerRoomManager)target;

        if (GUILayout.Button("Join Random Room"))
        {
            roomManager.JoinRandomRoom();
        }

        if (GUILayout.Button("Join Classroom Room"))
        {
            roomManager.OnEnterButtonClicked_Classroom();
        }

        if (GUILayout.Button("Join Outdoor Room"))
        {
            roomManager.OnEnterButtonClicked_Outdoor();
        }
    }
}
