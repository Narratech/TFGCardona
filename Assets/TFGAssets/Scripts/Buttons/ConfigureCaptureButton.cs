using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConfigureCaptureButton : VR_Button_Template
{
    /// <summary>
    /// Esta clase debe usarse en una escena que contenga un objeto con el script
    /// asociado GestureCaptureManager.
    /// </summary>

    // Gesture Recognizer GO
    [SerializeField]
    private TextMeshPro guideText;

    [SerializeField]
    private eButtonMode onClickBehavior;
    
    //Booleans
    public bool gestureRecorded = false;
    private bool runLogic = false;

    // timers
    private float timeAcu = 0.0f;

    // Que pasa al pulsar el boton
    public override void OnClick()
    {
        runLogic = true;
    }

    public override void Update()
    {
        base.Update();

        if (runLogic)
        {
            if (onClickBehavior == eButtonMode.GESTURE_CAPTURE)
            {
                // It's Clicked, so run logic.
                RunCaptureLogic();
            }
            else if (onClickBehavior == eButtonMode.SEND_COMMAND)
            {
                RunSendCommandLogic();
            }
        }
    }

    void RunCaptureLogic()
    {
        // Acumulamos tiempo
        timeAcu += Time.deltaTime;

        if (timeAcu < 1.0f)
            guideText.text = "Captura en 5 segundo.";
        else if (timeAcu > 1.0f && timeAcu < 2.0f)
            guideText.text = "Captura en 4 segundo.";
        else if (timeAcu > 2.0f && timeAcu < 3.0f)
            guideText.text = "Captura en 3 segundo.";
        else if (timeAcu > 3.0f && timeAcu < 4.0f)
            guideText.text = "Captura en 2 segundo.";
        else if (timeAcu > 4.0f && timeAcu < 5.0f)
            guideText.text = "Captura en 1 segundo.";
        else if (timeAcu > 5.0f && timeAcu < 6.0f)
        {
            guideText.text = "Capturando Gesto!";

            if (!gestureRecorded)
            {
                TextManager.Instance.EnqueuePersistenceText("RecordButton::runLogicOnButtonPush() - Llamando a GR.SaveGesture()");
                
                GestureRecognizer.Instance.SaveGesture(
                    GestureCaptureManager.Instance.getHand(),
                    GestureCaptureManager.Instance.getPhase(),
                    GestureCaptureManager.Instance.getCategory(),
                    GestureCaptureManager.Instance.getSimpleTranscription(),
                    GestureCaptureManager.Instance.getComplexTranscription(),
                    GestureCaptureManager.Instance.getGestureName());
                
                gestureRecorded = true;
            }
        }
        else if (timeAcu > 6.0f)
        {
            if (guideText != null)
                guideText.text = "Gesture Captured.";

            gestureRecorded = false;
            timeAcu = 0.0f;
            runLogic = false;
        }
    }

    void RunSendCommandLogic()
    {
        switch (GestureCaptureManager.Instance.getGestureName())
        {
            case "SEND":
                TextManager.Instance.OnSendCommand();
                break;
            case "CLEAR":
                TextManager.Instance.ClearChatBuffer();
                break;
            case "BACKSPACE":
                TextManager.Instance.BackspaceOnBuffer();
                break;
            default:
                Debug.Log("RunSendCommandLogic(): Gesture name not recognized");
                break;
        }
        runLogic = false;
    }
}