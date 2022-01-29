using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum handRecord
{
    LEFT_HAND,
    RIGHT_HAND,
    BOTH_HANDS
};

public class RecordButton : MonoBehaviour
{
    // Posiciones mínimas y máximas del boton
    public float MinLocalY = 0.38f;
    public float MaxLocalY = 0.55f;

    // Gesture Recognizer GO
    [SerializeField]
    private GestureRecognizer GR;
    [SerializeField]
    private TextMeshPro guideText;

    // Booleans
    public handRecord handSelector;
    public bool isBeingTouched = false;
    public bool isClicked = false;
    public bool isRunningLogic = false;
    public bool gestureRecorded = false;

    // Color del boton
    public Material greenMat;
    public Material redMat;

    // Movimiento Recuperacion boton
    public float smooth = 0.1f;

    // Timers
    public float timeAcu = 0.0f;
    public float waitTime = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 buttonDownPosition = new Vector3(transform.localPosition.x, MinLocalY, transform.localPosition.z);
        Vector3 buttonUpPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);

        // Getting it back into normal position
        if (!isBeingTouched && (transform.localPosition.y > MaxLocalY || transform.localPosition.y < MaxLocalY))
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, buttonUpPosition, Time.deltaTime * smooth);
        }

        if (!isClicked)
        {
            if (transform.localPosition.y < MinLocalY)
            {
                isClicked = true;
                transform.localPosition = buttonDownPosition;
                OnButtonDown(); 
            }
        }
        else
        {
            if (transform.localPosition.y > MaxLocalY - 0.01f)
            {
                isClicked = false;
                transform.localPosition = buttonUpPosition;
                OnButtonUp();
            }
        }

        if (isRunningLogic)
        {
            // It's Clicked, so run logic.
            runLogicOnButtonPush();
        }
    }

    // Que pasa al pulsar el boton
    void OnButtonDown()
    {
        GetComponent<MeshRenderer>().material = greenMat;
        GetComponent<Collider>().isTrigger = true;
        if (!isRunningLogic)
        {
            timeAcu = 0.0f;
            isRunningLogic = true;
        }
    }

    /// <summary>
    /// Que pasa al volver al estado inicial
    /// </summary>
    void OnButtonUp()
    {
        GetComponent<MeshRenderer>().material = redMat;
        GetComponent<Collider>().isTrigger = false;
    }

    void runLogicOnButtonPush()
    {
        // Acumulamos tiempo
        timeAcu += Time.deltaTime;

        if (timeAcu < 1.0f)
            guideText.text = "Capture in 5 seconds.";
        else if (timeAcu > 1.0f && timeAcu < 2.0f)
            guideText.text = "Capture in 4 seconds.";
        else if (timeAcu > 2.0f && timeAcu < 3.0f)
            guideText.text = "Capture in 3 seconds.";
        else if (timeAcu > 3.0f && timeAcu < 4.0f)
            guideText.text = "Capture in 2 seconds.";
        else if (timeAcu > 4.0f && timeAcu < 5.0f)
            guideText.text = "Capture in 1 seconds.";
        else if (timeAcu > 5.0f && timeAcu < 7.0f)
        {
            guideText.text = "Capturing gesture!!!.";
         
            if (!gestureRecorded)
            {
                if (handSelector == handRecord.LEFT_HAND)
                    GR.SaveLeftHandGesture();
                else if (handSelector == handRecord.RIGHT_HAND)
                    GR.SaveRightHandGesture();
                else if (handSelector == handRecord.BOTH_HANDS)
                    GR.SaveFullGesture();
                else
                    Debug.Log("RecordButton: Error! Could not decide which hand to record.");

                gestureRecorded = true;
            }
        }
        else if (timeAcu > 7.0f)
        {
            guideText.text = "Gesture Captured.";
            isRunningLogic = false;
            timeAcu = 0.0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isClicked)
        {
            // do something...
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.tag != "BackButton")
        {
            isBeingTouched = true;
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.tag != "BackButton")
        {
            isBeingTouched = false;
        }
    }
}
