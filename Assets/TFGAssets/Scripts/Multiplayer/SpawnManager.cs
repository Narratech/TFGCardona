using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    GameObject GenericVRPlayerPrefab;

    public Vector3 SpawnPosition;

    // Start is called before the first frame update
    void Awake()
    {
        // Instantiate players in scene.
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Instantiating player prefab into multiplayer scene.");
            // Usando un player prefab.
            // Local OVRPlayer Controller access to headset data, therefore the prefab instantiation should not have
            // access to that.
            // 1.- Create an empty object in a scene, rename it to GenericVRPlayer
            // 2.- Duplicate a OVRPlayerController and add the duplicate as child of GenericVRPlayer
            // 3.- Create a Resources folder in your assets, drag GenericVRPlayer to create a prefab
            // 4.- Remove GenericVRPlayer from scene.
            // 5.- Click on GenericVRPlayer Prefab and add PhotonView script as component. It identifies an object through the network.
            //     Each player has a viewID that differenciates them.

            PhotonNetwork.Instantiate(GenericVRPlayerPrefab.name, SpawnPosition, Quaternion.identity);
        }
    }
}
