using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TextManager : MonoBehaviour
{
    //--------- VARIABLES EN INSPECTOR ----------------

    [Header("Other Managers")]
    // Referencias Externas
    public Persistence persistenceManager;
    public GestureRecognizer gestureRecognizer;

    // TEXTO QUE VE EL JUGADOR
    [Header("OVR RIG Player UI Panels")]
    [SerializeField]
    private TextMeshPro textoChatGUI;
    [SerializeField]
    private TextMeshPro textoRecogGUI;
    [SerializeField]
    private TextMeshPro chatBufferGUI;
    [SerializeField]
    private TextMeshPro topHeadText;
    [SerializeField]
    private GameObject topHeadTextBackground;

    [Header("Top Head Text Timer")]
    private float topHeadTextTimer = 0.0f;
    [SerializeField]
    public float topHeadTextDuration = 5.0f;
    public bool topHeadTextIsVisible = false;

    [Header("LOGS")]
    public bool saveDebug = false;
    public bool saveRecog = false;
    public bool saveCapture = false;
    public bool saveChat = false;

    [Header("OPTIONAL PANELS")]
    // OPCIONAL en Escena: Texto 3D donde se muestra la info de los huesos
    [SerializeField]
    private TextMeshPro RHDebugText;
    [SerializeField]
    private TextMeshPro LHDebugText;
    // OPCIONAL en Escena: Textos que se actualizan por completo
    [SerializeField]
    private TextMeshPro textoRecog;
    [SerializeField]
    private TextMeshPro textoGuardarGesto; // Record Panel
    
    // OPCIONAL en Escena: Paneles de Texto con colas de registro
    [SerializeField]
    private TextMeshPro textoDebug;
    [SerializeField]
    private TextMeshPro textoChat;
    [SerializeField]
    private TextMeshPro textoPersistencia;

    [Header("OPTIONAL PANEL DATA")]
    // Esqueletos de ambas manos
    [SerializeField]
    private OVRSkeleton RHskeleton;
    [SerializeField]
    private OVRSkeleton LHskeleton;
    [SerializeField]
    private Transform RHAnchor;
    [SerializeField]
    private Transform LHAnchor;
    // Parametros de paneles
    public int debugTextMaxLines = 35;
    public int chatTextMaxLines = 5;
    public int persistTextMaxLines = 10;

    [Header("Optional Panels Update")]
    // Activadores de debugs
    // Debug General
    public bool updateDebugPanel = false;
    // Reconocimiento de gestos de la DB 
    public bool updateRecogPanel = false;
    // Captura de nuevos gestos
    public bool updateCapturePanel = false;
    // Chat
    public bool updateChatPanel = false;
    // Persistencia
    public bool updatePersistencyPanel = false;
    // Debug Huesos de la mano
    public bool updateBonesPanel = false;

    //--------- VARIABLES INTERNAS ----------------

    // Huesos de las manos
    private List<OVRBone> RHfingerBones;
    private List<OVRBone> LHfingerBones;

    // Colas de texto
    private Queue<string> debugTextQueue;
    private List<string> debugLog;
    private Queue<string> chatTextQueue;
    private string chatBuffer;
    private List<string> chatLog;
    private Queue<string> persistTextQueue;
    private List<string> persistLog;

    // Indices
    private int debugTextIndex = 0;
    private int chatTextIndex = 0;
    private int persistTextIndex = 0;

    // Strings
    private string sessionStart;

    // CLASE COMO SINGLETON
    public static TextManager Instance;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        
    }

    public void Init()
    {
        // Colas
        debugTextQueue = new Queue<string>();
        chatTextQueue = new Queue<string>();
        persistTextQueue = new Queue<string>();
        debugLog = new List<string>();
        chatLog = new List<string>();
        persistLog = new List<string>();

        // Huesos manos
        if (RHskeleton != null && LHskeleton != null)
        {
            RHfingerBones = new List<OVRBone>(RHskeleton.Bones);
            LHfingerBones = new List<OVRBone>(LHskeleton.Bones);
        }

        // First message.
        string nowTime = DateTime.Now.ToString();
        sessionStart = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        EnqueueDebugText("Session: " + nowTime);
        EnqueueChatText("Session: " + nowTime, eMessageSource.DEBUG);

        // Chat Buffer
        chatBuffer = "";
    }

    private void Update()
    {
        // Top head text management
        if (topHeadTextIsVisible)
        {
            topHeadTextTimer += Time.deltaTime;

            if (topHeadTextTimer > topHeadTextDuration)
            {
                clearTopHeadText();
                topHeadTextTimer = 0.0f;
                topHeadTextIsVisible = false;
            }

        }
    }

    /// <summary>
    /// Metodo que llama a la persistencia para guardar los archivos de log con los datos almacenados en el debug y chat.
    /// </summary>
    private void SaveLogs()
    {
        if (saveDebug)
        { 
            Debug.Log("saveLogs() - Guardando archivos de Debug.");
            persistenceManager.saveTextLog(Application.persistentDataPath + "/" + sessionStart + "-debugLog.xml", debugLog);
        }

        if (saveChat)
        { 
            Debug.Log("saveLogs() - Guardando archivos de Chat.");
            persistenceManager.saveTextLog(Application.persistentDataPath + "/" + sessionStart + "-chatLog.xml", chatLog);
        }

        
        // No guardamos por que sino estaríamos haciendo un bucle ciclico que guarda lo guardado y intenta guardar los nuevos mensajes de guardado ad infinitum.
        //if (save
        //Debug.Log("saveLogs() - Guardando archivos de Persistencia.");
        //persistenceManager.saveTextLog(Application.persistentDataPath + "/" + sessionStart + "-persistLog.xml", persistLog);
    }

    /////////////////////////////////////////
    ///  MENSAJES DE PERSITENCIA          ///
    /////////////////////////////////////////

    /// <summary>
    /// Encola un nuevo mensaje en la cola manteniendo un limite de líneas y manda imprimir
    /// el texto en el panel correspondiente de la escena.
    /// </summary>
    /// <param name="line"></param>
    public void EnqueuePersistenceText(string line)
    {
        Debug.Log("enqueuePersistenceText() - Encolando texto: " + line + "\n");
        persistTextIndex++;

        // Add line
        persistTextQueue.Enqueue(line);
        persistLog.Add(persistTextIndex + ": " + line);

        if (persistTextQueue.Count > persistTextMaxLines)
        {
            persistTextQueue.Dequeue(); // Quitamos el primer elemento de la cola (FIFO)
        }

        UpdatePersistPanel();
    }

    /// <summary>
    /// Actualiza el texto del panel de persistencia en la escena.
    /// </summary>
    public void UpdatePersistPanel()
    {
        if (updatePersistencyPanel)
        { 
            Debug.Log("Actualizando panel Persistencia.");
            int index = 0;
            if (textoPersistencia != null)
            { 
                textoPersistencia.text = "";

                foreach (string line in persistTextQueue)
                {
                    if (index < persistTextQueue.Count - 1)
                        textoPersistencia.text += line + "\n";
                    else
                        textoPersistencia.text += line;
                    index++;
                }
            }
        }
    }

    /////////////////////////////////////////
    ///  MENSAJES DEL CHAT                ///
    /////////////////////////////////////////

    /// <summary>
    /// Encola un nuevo mensaje en la cola manteniendo un limite de líneas y manda imprimir
    /// el texto en el panel correspondiente de la escena.
    /// </summary>
    /// <param name="line"></param>
    public void EnqueueChatText(string line, eMessageSource eTextSource)
    {
        Debug.Log("enqueueChatText() - Encolando texto: " + line + "\n");
        chatTextIndex++;

        // Añadimos iconos
        string textIntoChat;
        if (eTextSource == eMessageSource.VOICE)
        {
            textIntoChat = "<sprite=\"IconosChat\" name=\"IconosChat_0\">" + line;
        }
        else if (eTextSource == eMessageSource.HAND_SIGN)
        {
            textIntoChat = "<sprite=\"IconosChat\" index=1> " + line;
        }
        else
        {
            textIntoChat = line;
        }

        // Add line
        chatTextQueue.Enqueue(textIntoChat);
        chatLog.Add(chatTextIndex + ": " + line);

        if (chatTextQueue.Count > chatTextMaxLines)
        {
            chatTextQueue.Dequeue(); // Quitamos el primer elemento de la cola (FIFO)
        }

        UpdateChat();   
    }

    /// <summary>
    /// Actualiza el texto del panel de chat en la escena.
    /// </summary>
    public void UpdateChat()
    {
        Debug.Log("Actualizando panel Chat.");
        //int index = 0;
        if (updateChatPanel) textoChat.text = "";
        // El GUI del jugador siempre se actualiza
        textoChatGUI.text = "";
        string auxText = "";

        foreach (string line in chatTextQueue)
        {
            // Texto más reciente se coloca arriba
            // Nos interesa para que se corte el texto que sobresale de
            // la ventana de chat.
            auxText = line + "\n" + textoChatGUI.text;

            textoChatGUI.text = auxText;
            if (updateChatPanel) textoChat.text = auxText;

            /*
             * Texto más reciente se coloca abajo.
            if (index < chatTextQueue.Count - 1)
            {
                if (updateChatPanel) textoChat.text += line + "\n";
                textoChatGUI.text += line + "\n";
            }
            else
            {
                if (updateChatPanel) textoChat.text += line;
                textoChatGUI.text += line;
            }
            index++;
            */
        }

        if (saveChat) SaveLogs();
    }

    /////////////////////////////////////
    ///  MENSAJES DEBUG               ///
    /////////////////////////////////////

    /// <summary>
    /// Encola un nuevo mensaje en la cola manteniendo un limite de líneas y manda imprimir
    /// el texto en el panel correspondiente de la escena.
    /// </summary>
    /// <param name="line"></param>
    public void EnqueueDebugText(string line)
    {
        if (updateDebugPanel)
        {
            Debug.Log("enqueueDebugText() - Encolando texto: " + line);
            debugTextIndex++;

            // Add line
            debugTextQueue.Enqueue(debugTextIndex + ": " + line);
            debugLog.Add(debugTextIndex + ": " + line);

            // Si tenemos el máximo de lineas, borramos la última.
            if (debugTextQueue.Count > debugTextMaxLines)
            {
                debugTextQueue.Dequeue(); // Quitamos el primer elemento de la cola (FIFO)
            }

            // Mandamos imprimir
            UpdateDebugPanel();
        }
        else
        {
            Debug.Log("Update Debug Panel is deactivated.");
        }
    }

    /// <summary>
    /// Actualiza el texto del panel de debug en la escena.
    /// </summary>
    public void UpdateDebugPanel()
    {
        if (updateDebugPanel)
        {
            Debug.Log("Actualizando panel Debug.");
            int index = 0;
            string newText = "";
            textoDebug.text = "";


            foreach (string line in debugTextQueue)
            {
                if (index < debugTextQueue.Count - 1)
                    newText += line + "\n";
                else
                    newText += line;
                index++;
            }

            textoDebug.text = newText;
            if (saveDebug) SaveLogs();
        }
        else
        {
            Debug.Log("Update Debug Panel is deactivated.");
        }
    }

    /////////////////////////////////////
    ///  MENSAJES CAPTURA GESTOS      ///
    /////////////////////////////////////

    /// <summary>
    /// Panel de reconocimiento
    /// </summary>
    /// <param name="newText"></param>
    public void SetCapturePanel(string newText)
    {
        textoGuardarGesto.text = newText;
    }

    public void AppendCapturePanel(string nextText)
    {
        textoGuardarGesto.text = textoGuardarGesto.text + "\n" + nextText;
    }

    /////////////////////////////////////
    ///  MENSAJES RECONOCIMIENTO      ///
    /////////////////////////////////////

    /// <summary>
    /// Panel De reconocimiento
    /// </summary>
    /// <param name="newText"></param>
    public void SetRecogText(string newText)
    {
        textoRecog.text = newText;
    }

    /////////////////////////////////////////
    ///  MENSAJES DE LOS HUESOS           ///
    /////////////////////////////////////////
    public void UpdateBonePanels()
    {
        if (updateBonesPanel)
        {
            string RHtext = "Bone Debugger\n------------------------\nRIGHT HAND\n";
            string LHtext = "Bone Debugger\n------------------------\nLEFT HAND\n";

            //RH Information
            if (RHfingerBones != null)
            {
                RHtext = RHtext + "RHAnchor: POS(" + RHAnchor.position.x + ", " + RHAnchor.position.y + ", " + RHAnchor.position.z + ")\n";
                RHtext = RHtext + "          ROT(" + RHAnchor.rotation.x + ", " + RHAnchor.rotation.y + ", " + RHAnchor.rotation.z + ", " + RHAnchor.rotation.w + ")\n";
                foreach (OVRBone rhbone in RHfingerBones)
                {
                    RHtext = RHtext + rhbone.Id + " P: ";
                    RHtext = RHtext + RHskeleton.transform.InverseTransformPoint(rhbone.Transform.position) + " - R: " + rhbone.Transform.rotation;
                    RHtext = RHtext + "\n";
                }
            }
            //LH Information
            if (LHfingerBones != null)
            {
                LHtext = LHtext + "LHAnchor: POS(" + LHAnchor.position.x + ", " + LHAnchor.position.y + ", " + LHAnchor.position.z + ")\n";
                LHtext = LHtext + "          ROT(" + LHAnchor.rotation.x + ", " + LHAnchor.rotation.y + ", " + LHAnchor.rotation.z + ", " + LHAnchor.rotation.w + ")\n";
                foreach (OVRBone lhbone in LHfingerBones)
                {
                    LHtext = LHtext + lhbone.Id + " P: ";
                    LHtext = LHtext + LHskeleton.transform.InverseTransformPoint(lhbone.Transform.position) + " - R: " + lhbone.Transform.rotation;
                    LHtext = LHtext + "\n";
                }
            }

            RHDebugText.text = RHtext;
            LHDebugText.text = LHtext;
        }
        /*
        else 
        {
            Debug.Log("Update Bone Panels is deactivated.");
        }
        */
    }

    /////////////////////////////////////////
    ///  GUI RECONOCIMIENTO JUGADOR       ///
    /////////////////////////////////////////
    /// <summary>
    /// Panel De reconocimiento
    /// </summary>
    /// <param name="newText"></param>
    public void SetRecogGUIText(string newText)
    {
        textoRecogGUI.text = newText;
    }

    /////////////////////////////////////////
    ///  GUI RECONOCIMIENTO JUGADOR       ///
    /////////////////////////////////////////

    /// <summary>
    /// Agrega un texto al buffer de chat.
    /// El parametro addSpace permite añadir un espacio entre el texto existente
    /// en el buffer y el que se quiere agregar.
    /// </summary>
    /// <param name="txtToAppend"></param>
    /// <param name="addSpace"></param>
    public void AppendChatBuffer(string txtToAppend, bool addSpace = false)
    {
        // Concatenamos el nuevo texto al buffer
        if (addSpace)
        {
            chatBuffer = String.Concat(chatBuffer, txtToAppend);
            chatBuffer = chatBuffer + "_";
        }
        else
        {
            chatBuffer = String.Concat(chatBuffer, txtToAppend);
        }
        
        // Actualizamos el texto del GUI
        chatBufferGUI.text = chatBuffer;
    }

    public void ClearChatBuffer()
    {
        // Vaciamos el Buffer
        chatBuffer = "";
        // Actualizamos el texto del GUI
        chatBufferGUI.text = chatBuffer;
    }

    /// <summary>
    /// Elimina el último caracter de la cadena de texto.
    /// 
    /// ESTO VA CARACTER A CARACTER
    /// CONSIDERAR HACER UNA LISTA DE STRINGS COMO CHAT BUFFER
    /// PARA PODER ELIMINAR PALABRAS COMPLETAS CON EL BACKSPACE
    /// FINALMENTE UNIFICAR LA LISTA EN UN STRING AL RECIBIR
    /// COMMANDO SEND
    /// 
    /// </summary>
    public void BackspaceOnBuffer()
    {
        string textEdited = "";

        // Eliminamos el último caracter
        if (chatBuffer.Length > 1)
        {
            textEdited = chatBuffer.Remove(chatBuffer.Length - 1);
        }
        else
        { 
            textEdited = "";
        }

        //Debug.Log("Original text: (" + chatBuffer + ")");
        //Debug.Log("Text edited: (" + textEdited + ")");
        //EnqueueDebugText("BACKSPACE - Original text: (" + chatBuffer + ")");
        //EnqueueDebugText("BACKSPACE - Edited text: (" + textEdited + ")");

        chatBuffer = textEdited;

        // Actualizamos el texto del GUI
        chatBufferGUI.text = chatBuffer;
    }


    /////////////////////////////////////////
    ///          COMANDOS DE CHAT         ///
    /////////////////////////////////////////
    public void OnSendCommand()
    {
        // Sustituimos los "_" por espacios en blanco
        string processedText = chatBuffer.Replace("_", " ");
        EnqueueChatText(processedText, eMessageSource.HAND_SIGN);

        // Mandamos el texto del buffer al chat
        // chatTextQueue.Enqueue(processedText);
        // Actualizamos la ventana de chat
        // UpdateChatPanel();

        // Actualizamos el top head text
        if (topHeadText != null && topHeadTextBackground != null)
        {
            startTopHeadTextTimer();
            topHeadTextBackground.SetActive(true);
            topHeadText.gameObject.SetActive(true);
            topHeadText.text = chatBuffer;
        }

        // Vaciamos el buffer
        chatBuffer = "";
        // Actualizamos el bufferGUI
        chatBufferGUI.text = chatBuffer;
        
    }


    ////////////////////////////////////////////////////////////////////
    //////////  METODOS DE GESTION DE TEXTO SOBRE CABEZA ///////////////
    ////////////////////////////////////////////////////////////////////
    // esto esta aquí para evitar tener varios updates en los objetos //
    // el update del GestureRecognizer se ocupa de contar el tiempo   //
    // que el texto sobre la cabeza será mostrado.                    //
    ////////////////////////////////////////////////////////////////////
    ///
    public void clearTopHeadText()
    {
        if (topHeadText != null && topHeadTextBackground != null)
        {
            topHeadText.text = "";
            topHeadText.gameObject.SetActive(false);
            topHeadTextBackground.SetActive(false);
        }
    }

    public void startTopHeadTextTimer()
    {
        topHeadTextIsVisible = true;
    }

    public bool isTopHeadTimerActive()
    {
        return topHeadTextIsVisible;
    }

    public void resetTopHeadTimer()
    {
        topHeadTextTimer = 0.0f;
    }
}
