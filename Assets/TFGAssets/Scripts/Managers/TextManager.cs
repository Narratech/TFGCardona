using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public enum eMessageSource
{
    DEBUG,
    VOICE,
    HAND_SIGN    
}

public class TextManager : MonoBehaviour
{
    // Referencias Externas
    public Persistence persistenceManager;

    // Esqueletos de ambas manos
    [SerializeField]
    private OVRSkeleton RHskeleton;
    [SerializeField]
    private OVRSkeleton LHskeleton;

    // Texto 3D donde se muestra la info
    [SerializeField]
    private TextMeshPro RHDebugText;
    [SerializeField]
    private TextMeshPro LHDebugText;

    // Huesos de las manos
    private List<OVRBone> RHfingerBones;
    private List<OVRBone> LHfingerBones;

    // Textos que se actualizan por ocmpleto
    [SerializeField]
    private TextMeshPro textoRecog;
    [SerializeField]
    private TextMeshPro textoGuardarGesto; // Record Panel
    
    // Textos con colas de registro
    [SerializeField]
    private TextMeshPro textoDebug;
    [SerializeField]
    private TextMeshPro textoChat;
    [SerializeField]
    private TextMeshPro textoPersistencia;

    // TEXTO QUE VE EL JUGADOR
    [SerializeField]
    private TextMeshPro textoChatGUI;
    [SerializeField]
    private TextMeshPro textoRecogGUI;
    [SerializeField]
    private TextMeshPro chatBufferGUI;

    // Colas de texto
    private Queue<string> debugTextQueue;
    private List<string> debugLog;
    private Queue<string> chatTextQueue;
    private string chatBuffer;
    private List<string> chatLog;
    private Queue<string> persistTextQueue;
    private List<string> persistLog;

    // Activadores de debugs
    // Debug General
    public bool updateDebug  = true;
    public bool saveDebug = false;
    // Reconocimiento de gestos de la DB 
    public bool updateRecog  = true;
    public bool saveRecog = false;
    // Captura de nuevos gestos
    public bool updateCapture = true;
    public bool saveCapture = false;
    // Chat
    public bool updateChat   = true;
    public bool saveChat = false;
    // Debug Huesos de la mano
    public bool updateBones  = false;
    
    // Parametros de paneles
    public int debugTextMaxLines = 35;
    public int chatTextMaxLines = 5;
    public int persistTextMaxLines = 10;
    private int debugTextIndex = 0;
    private int chatTextIndex = 0;
    private int persistTextIndex = 0;
    private string sessionStart;

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
        RHfingerBones = new List<OVRBone>(RHskeleton.Bones);
        LHfingerBones = new List<OVRBone>(LHskeleton.Bones);

        // First message.
        string nowTime = DateTime.Now.ToString();
        sessionStart = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        EnqueueDebugText("Session: " + nowTime);
        EnqueueChatText("Session: " + nowTime, eMessageSource.DEBUG);

        // Chat Buffer
        chatBuffer = "";
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
        Debug.Log("enqueuePersistenceText() - Encolando texto: " + line);
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
        Debug.Log("Actualizando panel Persistencia.");
        int index = 0;
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
        if (updateChat)
        {
            Debug.Log("enqueueChatText() - Encolando texto: " + line);
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

            UpdateChatPanel();
        }
        else
            Debug.Log("Update Chat Panel is deactivated.");
    }

    /// <summary>
    /// Actualiza el texto del panel de chat en la escena.
    /// </summary>
    public void UpdateChatPanel()
    {
        Debug.Log("Actualizando panel Chat.");
        int index = 0;
        textoChat.text = "";
        textoChatGUI.text = "";

        foreach (string line in chatTextQueue)
        {
            if (index < chatTextQueue.Count - 1)
            { 
                textoChat.text += line + "\n";
                textoChatGUI.text += line + "\n";
            }
            else
            { 
                textoChat.text += line;
                textoChatGUI.text += line;
            }
            index++;
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
        if (updateDebug)
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
        if (updateDebug)
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
        if (updateBones)
        {
            string RHtext = "Bone Debugger\n------------------------\nRIGHT HAND\n";
            string LHtext = "Bone Debugger\n------------------------\nLEFT HAND\n";

            //RH Information
            if (RHfingerBones != null)
            {
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
        else 
        {
            Debug.Log("Update Bone Panels is deactivated.");
        }
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

        Debug.Log("Original text: (" + chatBuffer + ")");
        Debug.Log("Text edited: (" + textEdited + ")");
        EnqueueDebugText("BACKSPACE - Original text: (" + chatBuffer + ")");
        EnqueueDebugText("BACKSPACE - Edited text: (" + textEdited + ")");

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

        // Vaciamos el buffer
        chatBuffer = "";
        // Actualizamos el bufferGUI
        chatBufferGUI.text = chatBuffer;
    }
}
