using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginUIManager : MonoBehaviour
{
    public GameObject ConnectOptionsPanel;
    public GameObject ConnectWithNamePanel;

    [SerializeField]
    private TextMeshPro textoDebug;

    private int debugTextIndex = 0;
    [SerializeField]
    private int debugTextMaxLines = 10;
    private Queue<string> debugTextQueue;

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        textoDebug.text = "-Starting UI Manager-";
        ConnectOptionsPanel.SetActive(true);
        ConnectWithNamePanel.SetActive(false);
        debugTextQueue = new Queue<string>();
    }

    #endregion

    #region UIPanels
    public void enqueueDebugPanel(string newMessage)
    {
        Debug.Log("enqueueDebugText() - Encolando texto: " + newMessage);
        debugTextIndex++;

        // Add line
        debugTextQueue.Enqueue(debugTextIndex + ": " + newMessage);

        // Si tenemos el máximo de lineas, borramos la última.
        if (debugTextQueue.Count > debugTextMaxLines)
        {
            debugTextQueue.Dequeue(); // Quitamos el primer elemento de la cola (FIFO)
        }

        // Mandamos imprimir
        UpdateDebugPanel();
    }

    public void UpdateDebugPanel()
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
    }
    #endregion
}
