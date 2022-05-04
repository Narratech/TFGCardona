using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ChangeSceneButton : VR_Button_Template
{
    [SerializeField]
    private eScenes targetScene;
    public override void OnClick()
    {
        Debug.Log("Click on change scene: " + targetScene);

        switch (targetScene)
        {
            case eScenes.DEVELOPMENT:
                SceneManager.LoadSceneAsync("Dev_Scene");
                break;
            case eScenes.CAPTURE:
                SceneManager.LoadSceneAsync("Capture_Scene");
                break;
            case eScenes.LOGIN:
                SceneManager.LoadSceneAsync("Login_Scene");
                break;
            case eScenes.ROOM_SELECTION:
                SceneManager.LoadSceneAsync("Room_Selection_Scene");
                break;
            case eScenes.OUTDOORS:
                SceneManager.LoadSceneAsync("HR_Outdoors");
                break;
            case eScenes.CLASSROOM:
                SceneManager.LoadSceneAsync("HR_Classroom");
                break;
            case eScenes.TUTORIAL:
                SceneManager.LoadSceneAsync("Tutorial_Scene");
                break;
        }
    }
}
