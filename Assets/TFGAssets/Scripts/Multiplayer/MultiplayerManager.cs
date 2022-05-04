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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public void LeaveRoomAndLoadHomeScene()
    {
        PhotonNetwork.LeaveRoom();
    }

    #region Photon Callback Methods    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("The new Player: " + newPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name + " Player count " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public override void OnErrorInfo(ErrorInfo errorInfo)
    {
        base.OnErrorInfo(errorInfo);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("MultiplayerManager::OnLeftRoom()");
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //base.OnDisconnected(cause);
        Debug.Log("MultiplayerManager::OnDisconnected() Loading Room Selection Scene.");
        PhotonNetwork.LoadLevel("Room_Selection_Scene");//Login_Scene");
    }
    #endregion
}
