using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class LoginManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField inputField;
    // Panels
    public GameObject ConnectOptionPanel;
    public GameObject ConnectWithNamePanel;


    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        //PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region UI Callback Methods
    public void backToSelection() 
    {
        ConnectWithNamePanel.SetActive(false);
        ConnectOptionPanel.SetActive(true);
    }

    public void ConnectAnonymously()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void ConnectToPhotonServer()
    {
        if (inputField != null)
        {
            PhotonNetwork.NickName = inputField.text;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    #endregion

    #region Photon Callback Methods
    public override void OnConnected()
    {
        Debug.Log("LoginManager::OnConnected() The server is available!");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("LoginManager::OnConnectedToMaster() Connected to Master Server with player name: " + PhotonNetwork.NickName);
    }
    #endregion
}
