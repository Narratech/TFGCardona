using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaveRoomButton : VR_Button_Template
{
    public override void OnClick()
    {
        MultiplayerManager.Instance.LeaveRoomAndLoadHomeScene();
    }
}
