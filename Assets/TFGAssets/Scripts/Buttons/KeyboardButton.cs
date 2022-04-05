using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum KeyCommand 
{
    Q,
    W,
    E,
    R,
    T,
    Y,
    U,
    I,
    O,
    P,
    A,
    S,
    D,
    F,
    G,
    H,
    J,
    K,
    L,
    Ñ,
    Z,
    X,
    C,
    V,
    B,
    N,
    M,
    ENTER,
    BACKSPACE,
    BACK
}

public class KeyboardButton : MonoBehaviour
{
    // Behavior Selector
    [SerializeField]
    private orientation orientation;

    [SerializeField]
    private LoginManager loginManager;

    // Posiciones mínimas y máximas del boton
    [SerializeField]
    private float MinLocalX = 0.105f;
    [SerializeField]
    private float MaxLocalX = 0.2f;
    [SerializeField]
    private float MinLocalY = 0.38f;
    [SerializeField]
    private float MaxLocalY = 0.40f;

    [SerializeField]
    private TMP_InputField outputField;

    [SerializeField]
    private KeyCommand keyboardKey;

    private float minPos;

    // Posicion
    Vector3 buttonPressedPos;
    Vector3 buttonUnpressedPos;

    // Booleans
    public bool isBeingTouched = false;
    public bool isClicked = false;
    public bool isRunningLogic = false;

    // Color del boton
    public Material greenMat;
    public Material redMat;

    // Movimiento Recuperacion boton
    public float smooth = 0f;

    // Timers
    public float timeAcu = 0.0f;
    public float waitTime = 5.0f;

    void Start()
    {
        // Inicializamos la posición
        if (orientation == orientation.VERTICAL)
            transform.localPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);
        else if (orientation == orientation.HORIZONTAL)
            transform.localPosition = new Vector3(MaxLocalX, transform.localPosition.y, transform.localPosition.z);

        // Inicializar vectores
        buttonPressedPos = new Vector3(0f, 0f, 0f);
        buttonUnpressedPos = new Vector3(0f, 0f, 0f);
    }

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
            //Debug.Log("MinLocalX: " + MinLocalX);
            //Debug.Log("MaxLocalY: " + MaxLocalX);

            buttonPressedPos.Set(MinLocalX, transform.localPosition.y, transform.localPosition.z);
            buttonUnpressedPos.Set(MaxLocalX, transform.localPosition.y, transform.localPosition.z);

            //Debug.Log("buttonPressedPos (" + buttonPressedPos + ")");
            //Debug.Log("buttonUnpressedPos (" + buttonUnpressedPos + ")");

            // Getting it back into normal position
            if (!isBeingTouched && (transform.localPosition.x < MaxLocalX))
            {
                //Debug.Log("Lerping to position");
                transform.localPosition = Vector3.Lerp(transform.localPosition, buttonUnpressedPos, Time.deltaTime); // * Smooth)
            }
            else if (!isBeingTouched && (transform.localPosition.x > MaxLocalX))
            {
                //Debug.Log("Setting position to max");
                transform.localPosition.Set(MaxLocalX, transform.localPosition.y, transform.localPosition.z);
            }

            // Si no esta clicado
            if (!isClicked)
            {
                // Y su posición es menor a la mínima local
                if (transform.localPosition.x < MinLocalX)
                {
                    //Debug.Log("Is not clicked and pos (" + transform.localPosition.x + ") < MinLocalX: " + MinLocalX);
                    isClicked = true;
                    transform.localPosition = buttonPressedPos;
                    OnButtonDown();
                }
            }
            // Si esta clicado
            else
            {
                // Y su posición es MAYOR al máximo local
                if (transform.localPosition.x > MaxLocalX)// - 0.02f)
                {
                    //Debug.Log("Is clicked and pos > MinLocalX");
                    isClicked = false;
                    transform.localPosition = buttonUnpressedPos;
                    OnButtonUp();
                }
            }
        }

        if (isRunningLogic)
        {
            // CREATE AND CALL A LOGIC METHOD
            // Method that should be called once.
            appendKeyToOutputField();
            isRunningLogic = false;
        }
    }

    void appendKeyToOutputField()
    { 
        string key = "";
        switch (keyboardKey)
        {
            case KeyCommand.A:
                key = "A";
                break;
            case KeyCommand.B:
                key = "B";
                break;
            case KeyCommand.C:
                key = "C";
                break;
            case KeyCommand.D:
                key = "D";
                break;
            case KeyCommand.E:
                key = "E";
                break;
            case KeyCommand.F:
                key = "F";
                break;
            case KeyCommand.G:
                key = "G";
                break;
            case KeyCommand.H:
                key = "H";
                break;
            case KeyCommand.I:
                key = "I";
                break;
            case KeyCommand.J:
                key = "J";
                break;
            case KeyCommand.K:
                key = "K";
                break;
            case KeyCommand.L:
                key = "L";
                break;
            case KeyCommand.M:
                key = "M";
                break;
            case KeyCommand.N:
                key = "N";
                break;
            case KeyCommand.Ñ:
                key = "Ñ";
                break;
            case KeyCommand.O:
                key = "O";
                break;
            case KeyCommand.P:
                key = "P";
                break;
            case KeyCommand.Q:
                key = "Q";
                break;
            case KeyCommand.R:
                key = "R";
                break;
            case KeyCommand.S:
                key = "S";
                break;
            case KeyCommand.T:
                key = "T";
                break;
            case KeyCommand.U:
                key = "U";
                break;
            case KeyCommand.V:
                key = "V";
                break;
            case KeyCommand.W:
                key = "W";
                break;
            case KeyCommand.X:
                key = "X";
                break;
            case KeyCommand.Y:
                key = "Y";
                break;
            case KeyCommand.Z:
                key = "Z";
                break;
            case KeyCommand.ENTER:
                loginManager.ConnectToPhotonServer();
                break;
            case KeyCommand.BACKSPACE:
                string textEdited = "";
                // Eliminamos el último caracter
                if (outputField.text.Length > 1)
                {
                    textEdited = outputField.text.Remove(outputField.text.Length - 1);
                }
                else
                {
                    textEdited = "";
                }
                outputField.text = textEdited;
                return;
            case KeyCommand.BACK:
                loginManager.backToSelection();
                return;
            default:
                key = "invalid";
                break;
        }

        outputField.text = outputField + key;
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
