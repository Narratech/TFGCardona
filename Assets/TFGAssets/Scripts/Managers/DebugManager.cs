using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DebugManager : MonoBehaviour
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
    private TextMeshPro textoChatGUI;
    [SerializeField]
    private TextMeshPro textoPersistencia;

    // Colas de texto
    private Queue<string> debugTextQueue;
    private List<string> debugLog;
    private Queue<string> chatTextQueue;
    private List<string> chatLog;
    private Queue<string> persistTextQueue;
    private List<string> persistLog;

    // Activadores de debugs
    public bool updateDebug  = true;
    public bool updateRecog  = true;
    public bool updateRecord = true;
    public bool updateChat   = true;
    public bool updateBones  = false;
    
    // Parametros de paneles
    public int debugTextMaxLines = 40;
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
        enqueueDebugText("Session: " + nowTime);
        enqueueChatText("Session: " + nowTime);
    }

    /////////////////////////////////////////
    ///  GESTION DEL PERSITENCIA          ///
    /////////////////////////////////////////
    
    /// <summary>
    /// Metodo que llama a la persistencia para guardar los archivos de log con los datos almacenados en el debug y chat.
    /// </summary>
    private void saveLogs()
    {
        Debug.Log("saveLogs() - Guardando archivos de Debug.");
        persistenceManager.saveTextLog(Application.persistentDataPath + "/" + sessionStart + "-debugLog.xml", debugLog);
        Debug.Log("saveLogs() - Guardando archivos de Chat.");
        persistenceManager.saveTextLog(Application.persistentDataPath + "/" + sessionStart + "-chatLog.xml", chatLog);
        // No guardamos por que sino estaríamos haciendo un bucle ciclico que guarda lo guardado y intenta guardar los nuevos mensajes de guardado ad infinitum.
        //Debug.Log("saveLogs() - Guardando archivos de Persistencia.");
        //persistenceManager.saveTextLog(Application.persistentDataPath + "/" + sessionStart + "-persistLog.xml", persistLog);
    }

    /// <summary>
    /// Encola un nuevo mensaje en la cola manteniendo un limite de líneas y manda imprimir
    /// el texto en el panel correspondiente de la escena.
    /// </summary>
    /// <param name="line"></param>
    public void enqueuePersistenceText(string line)
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

        updatePersistPanel();
    }

    /// <summary>
    /// Actualiza el texto del panel de persistencia en la escena.
    /// </summary>
    public void updatePersistPanel()
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
    ///  GESTION DEL CHAT                 ///
    /////////////////////////////////////////

    /// <summary>
    /// Encola un nuevo mensaje en la cola manteniendo un limite de líneas y manda imprimir
    /// el texto en el panel correspondiente de la escena.
    /// </summary>
    /// <param name="line"></param>
    public void enqueueChatText(string line)
    {
        if (updateChat)
        {
            Debug.Log("enqueueChatText() - Encolando texto: " + line);
            chatTextIndex++;

            // Add line
            chatTextQueue.Enqueue(line);
            chatLog.Add(chatTextIndex + ": " + line);

            if (chatTextQueue.Count > chatTextMaxLines)
            {
                chatTextQueue.Dequeue(); // Quitamos el primer elemento de la cola (FIFO)
            }

            updateChatPanel();
        }
        else
            Debug.Log("Update Chat Panel is deactivated.");
    }

    /// <summary>
    /// Actualiza el texto del panel de chat en la escena.
    /// </summary>
    public void updateChatPanel()
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
        saveLogs();
    }

    /////////////////////////////////////////
    ///  GESTION DEL DEBUG                ///
    /////////////////////////////////////////

    /// <summary>
    /// Encola un nuevo mensaje en la cola manteniendo un limite de líneas y manda imprimir
    /// el texto en el panel correspondiente de la escena.
    /// </summary>
    /// <param name="line"></param>
    public void enqueueDebugText(string line)
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
            updateDebugPanel();
        }
        else
        {
            Debug.Log("Update Debug Panel is deactivated.");
        }
    }

    /// <summary>
    /// Actualiza el texto del panel de debug en la escena.
    /// </summary>
    public void updateDebugPanel()
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
            saveLogs();
        }
        else
        {
            Debug.Log("Update Debug Panel is deactivated.");
        }
    }

    /// <summary>
    /// Panel de reconocimiento
    /// </summary>
    /// <param name="newText"></param>
    public void setRecordPanel(string newText)
    {
        textoGuardarGesto.text = newText;
    }
    public void appendRecordPanel(string nextText)
    {
        textoGuardarGesto.text = textoGuardarGesto.text + "\n" + nextText;
    }

    /// <summary>
    /// Panel De reconocimiento
    /// </summary>
    /// <param name="newText"></param>
    public void setRecogText(string newText)
    {
        textoRecog.text = newText;
    }

    /////////////////////////////////////////
    ///  GESTION DE LOS HUESOS            ///
    /////////////////////////////////////////
    public void updateBonePanels()
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

}
