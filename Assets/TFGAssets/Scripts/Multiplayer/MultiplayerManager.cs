using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    // CLASE COMO SINGLETON
    public static MultiplayerManager Instance;

    //[SerializeField]
    //public TextMeshPro mpDebug;
    //private Queue<string> debugTextQueue;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }
    
    /*
    private void Start()
    {
        Init();
    }

    void Init()
    {
        //debugTextQueue = new Queue<string>();
    }
    */

    public void LeaveRoomAndLoadHomeScene()
    {
        PhotonNetwork.LeaveRoom();
    }

    #region Photon Callback Methods    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("The new Player: " + newPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name + " Player count " + PhotonNetwork.CurrentRoom.PlayerCount);
        //EnqueueDebugText("The new Player: " + newPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name + " Player count " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public override void OnErrorInfo(ErrorInfo errorInfo)
    {
        base.OnErrorInfo(errorInfo);
        //EnqueueDebugText("OnErrorInfo");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("MultiplayerManager::OnLeftRoom()");
        //EnqueueDebugText("OnLeftRoom().");
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //base.OnDisconnected(cause);
        Debug.Log("MultiplayerManager::OnDisconnected() Loading Room Selection Scene.");
        //EnqueueDebugText("OnDisconnected() Loading Room Selection Scene.");
        PhotonNetwork.LoadLevel("Room_Selection_Scene");//Login_Scene");
    }
    #endregion

    #region debug
    /*
    public void EnqueueDebugText(string line)
    {
        Debug.Log("enqueueDebugText() - Encolando texto: " + line);

        if (debugTextQueue != null)
        {
            // Add line
            debugTextQueue.Enqueue(line);

            // Mandamos imprimir
            UpdateDebugPanel();
        }
        else
        {
            Debug.Log("Calling MultiplayerManager::EnqueueDebugText() too early. Ignoring petition.");
        }
    }

    public void UpdateDebugPanel()
    {
        Debug.Log("Actualizando panel Debug.");
        int index = 0;
        string newText = "";

        if (mpDebug != null)
        {
            mpDebug.text = "";

            foreach (string line in debugTextQueue)
            {
                if (index < debugTextQueue.Count - 1)
                    newText += line + "\n";
                else
                    newText += line;
                index++;
            }

            mpDebug.text = newText;
        }
        else
        {
            Debug.Log("Calling MultiplayerManager::UpdateDebugPanel() too early. Ignoring petition.");
        }
    }
    */
    #endregion
}
