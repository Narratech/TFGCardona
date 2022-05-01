using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNetworkSetup : MonoBehaviourPunCallbacks
{
    public GameObject LocalOVRRigGameObject;
    public GameObject LocalGR;
    public GameObject LocalPersistence;
    public GameObject LocalTextManager;
    public GameObject SpeechRecognition;
    public GameObject AvatarHead;
    public GameObject AvatarBody;

    // Start is called before the first frame update
    void Start()
    {
        // Setup player (Tells us if the player is local or remote
        if (photonView.IsMine)
        {
            // The Player is local
            LocalOVRRigGameObject.SetActive(true);
            LocalGR.SetActive(true);
            LocalPersistence.SetActive(true);
            LocalTextManager.SetActive(true);
            SpeechRecognition.SetActive(true);

            // Allow/Block render of head and body
            SetLayerRecursively(AvatarHead, 7); // CustomLayer: LocalAvatarHead
            SetLayerRecursively(AvatarBody, 8); // CustomLayer: LocalAvatarBody
        }
        else
        {
            // The player is Remote
            LocalOVRRigGameObject.SetActive(false);
            LocalGR.SetActive(false);
            LocalPersistence.SetActive(false);
            LocalTextManager.SetActive(false);
            SpeechRecognition.SetActive(false);

            // Allow/Block render of head and body
            SetLayerRecursively(AvatarHead, 0); // Default Layer (Rendereable)
            SetLayerRecursively(AvatarBody, 0); // Default Layer (Rendereable)
        }
    }

    void SetLayerRecursively(GameObject go, int layerNumber)
    {
        if (go == null) return;
        foreach (Transform transf in go.GetComponentInChildren<Transform>(true))
        {
            transf.gameObject.layer = layerNumber;
        }
    }
}
