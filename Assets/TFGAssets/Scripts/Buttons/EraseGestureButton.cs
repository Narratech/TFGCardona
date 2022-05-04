using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseGestureButton : VR_Button_Template
{
    public override void OnClick()
    {
        //base.OnClick();
        if (DBManager.Instance != null) DBManager.Instance.removeLastGestureInDB();
    }
}
