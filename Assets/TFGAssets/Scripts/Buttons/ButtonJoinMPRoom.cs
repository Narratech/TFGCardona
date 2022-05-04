using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ButtonJoinMPRoom : VR_Button_Template
{
    [SerializeField]
    private MultiplayerRoomManager mpManager;

    [SerializeField]
    private eMapType mapType;

    public override void OnClick()
    {
        switch (mapType)
        {
            case eMapType.Aula:
                mpManager.OnEnterButtonClicked_Classroom();
                break;
            case eMapType.Exteriores:
                mpManager.OnEnterButtonClicked_Outdoor();
                break;
            default:
                mpManager.JoinRandomRoom();
                break;
        }
    }
}
