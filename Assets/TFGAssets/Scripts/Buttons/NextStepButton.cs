using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextStepButton : VR_Button_Template
{
    public string sentido;

    public override void OnClick()
    {
        if (sentido == "siguiente") TutorialManager.Instance.OnNextStep();
        else TutorialManager.Instance.OnPreviousStep();
    }
}
