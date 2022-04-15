using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPushClick : VR_Button_Template
{
    public GameObject OnOffObject;
    private bool activeState = true;

    public override void OnClick()
    {
        activeState = !activeState;

        // Disable mirror.
        OnOffObject.SetActive(activeState);
    }
}
