using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameObject MultiplayerVRPlayerPrefab;

    [SerializeField]
    GameObject RemoteVRPlayerPrefab;


    public Vector3 SpawnPosition;

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate players in scene.
        if (PhotonNetwork.IsConnectedAndReady)
        {
            //if (MultiplayerManager.Instance != null) MultiplayerManager.Instance.EnqueueDebugText("Instantiating Local Player prefab into multiplayer scene.");
            // Usando un player prefab.
            // Local OVRPlayer Controller access to headset data, therefore the prefab instantiation should not have
            // access to that.
            // 1.- Create an empty object in a scene, rename it to GenericVRPlayer
            // 2.- Duplicate a OVRPlayerController and add the duplicate as child of GenericVRPlayer
            // 3.- Create a Resources folder in your assets, drag GenericVRPlayer to create a prefab
            // 4.- Remove GenericVRPlayer from scene.
            // 5.- Click on GenericVRPlayer Prefab and add PhotonView script as component. It identifies an object through the network.
            //     Each player has a viewID that differenciates them.
            //PhotonNetwork.Instantiate(MultiplayerVRPlayerPrefab.name, SpawnPosition, Quaternion.identity);

            
            if (photonView.IsMine)
            {
                Debug.Log("Instantiating player prefab into multiplayer scene.");
                //if (MultiplayerManager.Instance != null) MultiplayerManager.Instance.EnqueueDebugText("Instantiating Local Player prefab into multiplayer scene.");
                // Usando un player prefab.
                // Local OVRPlayer Controller access to headset data, therefore the prefab instantiation should not have
                // access to that.
                // 1.- Create an empty object in a scene, rename it to GenericVRPlayer
                // 2.- Duplicate a OVRPlayerController and add the duplicate as child of GenericVRPlayer
                // 3.- Create a Resources folder in your assets, drag GenericVRPlayer to create a prefab
                // 4.- Remove GenericVRPlayer from scene.
                // 5.- Click on GenericVRPlayer Prefab and add PhotonView script as component. It identifies an object through the network.
                //     Each player has a viewID that differenciates them.

                PhotonNetwork.Instantiate(MultiplayerVRPlayerPrefab.name, SpawnPosition, Quaternion.identity);
            }
            else
            {
                //if (MultiplayerManager.Instance != null) MultiplayerManager.Instance.EnqueueDebugText("Instantiating Remote Player prefab into multiplayer scene.");
                PhotonNetwork.Instantiate(RemoteVRPlayerPrefab.name, SpawnPosition, Quaternion.identity);
            }
            
        }
    }
}
