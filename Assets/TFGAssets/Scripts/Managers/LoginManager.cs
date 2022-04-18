using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class LoginManager : MonoBehaviourPunCallbacks
{
    //public TMP_InputField inputField;
    public TextMeshPro playerNameField;
    // Panels
    public GameObject ConnectOptionPanel;
    public GameObject ConnectWithNamePanel;
    // Other Managers
    [SerializeField]
    private LoginUIManager uiManager;

    #region Unity Methods
    // ... Update, Start, Awake, etc.
    #endregion

    #region UI Callback Methods
    public void backToSelection() 
    {
        uiManager.enqueueDebugPanel("LoginManager::BackToSelection()");
        ConnectWithNamePanel.SetActive(false);
        ConnectOptionPanel.SetActive(true);
    }

    public void ConnectAnonymously()
    {
        uiManager.enqueueDebugPanel("LoginManager::ConnectAnonymously()");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void ConnectToPhotonServer()
    {
        uiManager.enqueueDebugPanel("LoginManager::ConnectToPhotonServer()");
        if (playerNameField.text.Length != 0)
        {
            PhotonNetwork.NickName = playerNameField.text;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    #endregion

    #region Photon Callback Methods
    public override void OnConnected()
    {
        Debug.Log("LoginManager::OnConnected() The server is available!");
        uiManager.enqueueDebugPanel("LoginManager::OnConnected() The server is available!");
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.NickName.Length == 0) 
        {
            PhotonNetwork.NickName = "John Doh";
        }
        Debug.Log("LoginManager::OnConnectedToMaster() Connected to Master Server with player name: " + PhotonNetwork.NickName);
        uiManager.enqueueDebugPanel("LoginManager::OnConnectedToMaster() Connected to Master Server with player name: " + PhotonNetwork.NickName);
        uiManager.enqueueDebugPanel("LoginManager::OnConnectedToMaster() Trying to load HR_Gathering_Scene");
        
        // Escena a la que saltamos.
        PhotonNetwork.LoadLevel("Room_Selection_Scene");
    }
    #endregion
}
