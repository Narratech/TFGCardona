using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

enum orientation
{ 
    HORIZONTAL,
    VERTICAL
}

public class RecordButton : MonoBehaviour
{
    // Behavior Selector
    [SerializeField]
    private orientation orientation;

    // Posiciones m�nimas y m�ximas del boton
    [SerializeField]
    private float MinLocalX = 0.105f;
    [SerializeField]
    private float MaxLocalX = 0.2f;
    [SerializeField]
    private float MinLocalY = 0.38f;
    [SerializeField]
    private float MaxLocalY = 0.40f;

    private float minPos;

    // Gesture Recognizer GO
    [SerializeField]
    private GestureRecognizer GR;
    [SerializeField]
    private DebugManager _debugManager;
    [SerializeField]
    private TextMeshPro guideText;

    // Configuraci�n del boton
    public string gestureName;
    public handUsage handSelector;
    public gesturePhase phase;    //
    public gestureCategory category;
    public string simpleTranscription;
    public List<string> composedTranscription;

    // Posicion
    Vector3 buttonPressedPos;
    Vector3 buttonUnpressedPos;

    // Booleans
    public bool isBeingTouched = false;
    public bool isClicked = false;
    public bool isRunningLogic = false;
    public bool gestureRecorded = false;

    // Color del boton
    public Material greenMat;
    public Material redMat;

    // Movimiento Recuperacion boton
    public float smooth = 0f;

    // Timers
    public float timeAcu = 0.0f;
    public float waitTime = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (orientation == orientation.VERTICAL)
            transform.localPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);
        else if (orientation == orientation.HORIZONTAL)
            transform.localPosition = new Vector3(MaxLocalX, transform.localPosition.y, transform.localPosition.z);
        
        // Inicializar vectores
        buttonPressedPos = new Vector3(0f, 0f, 0f);
        buttonUnpressedPos = new Vector3(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {

        if (orientation == orientation.VERTICAL)
        {
            buttonPressedPos.Set(transform.localPosition.x, MinLocalY, transform.localPosition.z);
            buttonUnpressedPos.Set(transform.localPosition.x, MaxLocalY, transform.localPosition.z);

            // Getting it back into normal position
            if (!isBeingTouched && (transform.localPosition.y > MaxLocalY || transform.localPosition.y < MaxLocalY))
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, buttonUnpressedPos, Time.deltaTime); // * Smooth)
            }

            if (!isClicked)
            {
                if (transform.localPosition.y < MinLocalY)
                {
                    isClicked = true;
                    transform.localPosition = buttonPressedPos;
                    OnButtonDown();
                }
            }
            else
            {
                if (transform.localPosition.y > MaxLocalY - 0.02f)
                {
                    isClicked = false;
                    transform.localPosition = buttonUnpressedPos;
                    OnButtonUp();
                }
            }
        }
        else if (orientation == orientation.HORIZONTAL)
        {
            buttonPressedPos.Set(MinLocalX, transform.localPosition.y, transform.localPosition.z);
            buttonUnpressedPos.Set(MaxLocalX, transform.localPosition.y, transform.localPosition.z);

            // Getting it back into normal position
            if (!isBeingTouched && (transform.localPosition.x > MaxLocalX || transform.localPosition.x < MaxLocalX))
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, buttonUnpressedPos, Time.deltaTime); // * Smooth)
            }

            if (!isClicked)
            {
                if (transform.localPosition.x < MinLocalX)
                {
                    isClicked = true;
                    transform.localPosition = buttonPressedPos;
                    OnButtonDown();
                }
            }
            else
            {
                if (transform.localPosition.x > MaxLocalX - 0.02f)
                {
                    isClicked = false;
                    transform.localPosition = buttonUnpressedPos;
                    OnButtonUp();
                }
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
        else if (timeAcu > 5.0f && timeAcu < 6.0f)
        {
            guideText.text = "Capturing gesture!!!.";
         
            if (!gestureRecorded)
            {
                _debugManager.enqueuePersistenceText("RecordButton::runLogicOnButtonPush() - Llamando a GR.SaveGesture()");
                GR.SaveGesture(handSelector, phase, category, simpleTranscription, composedTranscription, gestureName);
                gestureRecorded = true;
            }
        }
        else if (timeAcu > 6.0f)
        {
            if (guideText != null) 
                guideText.text = "Gesture Captured.";

            gestureRecorded = false;
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
