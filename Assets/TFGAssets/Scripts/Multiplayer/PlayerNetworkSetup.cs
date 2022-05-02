using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNetworkSetup : MonoBehaviourPunCallbacks
{
    public GameObject LocalOVRRigGameObject;
    public GameObject LocalManagers;
    public GameObject AvatarHead;
    public GameObject AvatarBody;
    public GameObject AvatarHandR;
    public GameObject AvatarHandL;

    // Start is called before the first frame update
    void Start()
    {
        // For now we dont draw the avatar hands
        AvatarHandR.SetActive(false);
        AvatarHandL.SetActive(false);

        // Setup player (Tells us if the player is local or remote
        if (photonView.IsMine)
        {
            // The Player is local
            LocalOVRRigGameObject.SetActive(true);
            LocalManagers.SetActive(true);

            // Allow/Block render of head and body
            SetLayerRecursively(AvatarHead, 7); // CustomLayer: LocalAvatarHead
            SetLayerRecursively(AvatarBody, 8); // CustomLayer: LocalAvatarBody
        }
        else
        {
            // The player is Remote
            LocalOVRRigGameObject.SetActive(false);
            LocalManagers.SetActive(false);

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
