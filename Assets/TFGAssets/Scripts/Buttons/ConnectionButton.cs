using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum connectionType
{ 
    ANONYMOUS,
    WITHNAME,
    CONNECT
}

public class ConnectionButton : MonoBehaviour
{
    // Behavior Selector
    [SerializeField]
    private orientation orientation;
    [SerializeField]
    private connectionType tipoConexion;

    // Panels
    public GameObject ConnectOptionPanel;
    public GameObject ConnectWithNamePanel;

    // Posiciones mínimas y máximas del boton
    [SerializeField]
    private float MinLocalX = 0.105f;
    [SerializeField]
    private float MaxLocalX = 0.2f;
    [SerializeField]
    private float MinLocalY = 0.38f;
    [SerializeField]
    private float MaxLocalY = 0.40f;

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

    // LoginManager
    public LoginManager loginManager;

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
            if (tipoConexion == connectionType.ANONYMOUS)
            {
                loginManager.ConnectAnonymously();
                Debug.Log("TipoConexion: Anonima.");
            }
            else if (tipoConexion == connectionType.WITHNAME)
            { 
                // to do
                Debug.Log("TipoConexion: Con Nombre.");
                ConnectOptionPanel.SetActive(false);
                ConnectWithNamePanel.SetActive(true);
            }
            else
                Debug.Log("TipoConexion: Invalida.");
            
            isRunningLogic = false;
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
