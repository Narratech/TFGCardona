using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum configuration
{ 
    HAND_USE,
    GEST_CAT,
    GEST_PHASE
}

public class HandConfiguratorButton : VR_Button_Template
{
    [SerializeField]
    private configuration buttonConfig;
    [SerializeField]
    private eHandUsage usedHand;
    [SerializeField]
    private eGesturePhase phase;
    [SerializeField]
    private eGestureCategory category;

    public override void OnClick()
    {
        switch (buttonConfig)
        {
            case configuration.HAND_USE:
                GestureCaptureManager.Instance.setHand(usedHand);
                break;
            case configuration.GEST_CAT:
                GestureCaptureManager.Instance.setCategory(category);
                break;
            case configuration.GEST_PHASE:
                GestureCaptureManager.Instance.setPhase(phase);
                break;
        }
    }
}
