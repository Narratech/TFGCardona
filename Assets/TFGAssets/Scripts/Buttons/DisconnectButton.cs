using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectButton : VR_Button_Template
{
    public override void OnClick()
    {
        //base.OnClick();
        if (MultiplayerManager.Instance != null) MultiplayerManager.Instance.LeaveRoomAndLoadHomeScene();
    }
}
