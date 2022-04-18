using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNetworkSetup : MonoBehaviourPunCallbacks
{
    public GameObject LocalOVRRigGameObject;
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

            // Allow/Block render of head and body
            SetLayerRecursively(AvatarHead, 7); // CustomLayer: LocalAvatarHead
            SetLayerRecursively(AvatarBody, 8); // CustomLayer: LocalAvatarBody
        }
        else
        {
            LocalOVRRigGameObject.SetActive(false);
            // The player is Remote

            // Allow/Block render of head and body
            SetLayerRecursively(AvatarHead, 0); // Default Layer (Rendereable)
            SetLayerRecursively(AvatarBody, 0); // Default Layer (Rendereable)
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
