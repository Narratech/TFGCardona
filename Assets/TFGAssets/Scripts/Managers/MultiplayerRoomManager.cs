using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MultiplayerRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public int maxPlayersPerRoom = 2;

    private string mapType;

    [SerializeField]
    public TextMeshProUGUI OccupancyRateText_ForClassroom;
    public TextMeshProUGUI OccupancyRateText_ForOutdoors;

    #region Unity Lifecycle
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinLobby();
        }

        mapType = MultiplayerVRConstants.MAP_TYPE_VALUE_INVALID;
    }
    #endregion

    #region UI Callback Methods

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Callback del botón en escena para el escenario exteriores
    /// </summary>
    public void OnEnterButtonClicked_Outdoor()
    {
        mapType = MultiplayerVRConstants.MAP_TYPE_VALUE_OUTDOOR;
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { MultiplayerVRConstants.MAP_TYPE_KEY, mapType } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, (byte)maxPlayersPerRoom); // Expected room and expected max players.
    }

    /// <summary>
    /// Callback del botón en escena para el escenario Aula
    /// </summary>
    public void OnEnterButtonClicked_Classroom()
    {
        mapType = MultiplayerVRConstants.MAP_TYPE_VALUE_CLASSROOM;
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { MultiplayerVRConstants.MAP_TYPE_KEY, mapType } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, (byte)maxPlayersPerRoom); // Expected room and expected max players.
    }

    #endregion

    #region Photon Callback Methods
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("MultiplayerRoomManager()::OnJoinRandomFailed() There are no rooms created");
        Debug.Log(message);

        CreateAndJoinRoom();
    }

    public override void OnCreatedRoom()
    {
        //base.OnCreatedRoom();
        Debug.Log("A room is created with the name: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        //base.OnJoinedRoom();
        Debug.Log("The Local Player: " + PhotonNetwork.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name + " Player count " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MultiplayerVRConstants.MAP_TYPE_KEY))
        {
            object mapType;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(MultiplayerVRConstants.MAP_TYPE_KEY, out mapType))
            {
                Debug.Log("Joined room with the map: " + (string)mapType);

                // LOAD SCENE TO LOCAL PLAYER
                if ((string)mapType == MultiplayerVRConstants.MAP_TYPE_VALUE_CLASSROOM)
                {
                    // Load Classroom scene
                    PhotonNetwork.LoadLevel("HR_Classroom");
                }
                else if ((string)mapType == MultiplayerVRConstants.MAP_TYPE_VALUE_OUTDOOR)
                {
                    // Load Outdoor sceen
                    PhotonNetwork.LoadLevel("HR_Outdoors");
                }

            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("The new Player: " + newPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name + " Player count " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    // Called everytime the room is created, a player joins, or it's properties has changed
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //base.OnRoomListUpdate(roomList);
        if (roomList.Count == 0)
        {
            // there is no room at all
            OccupancyRateText_ForClassroom.text = 0 + "/" + maxPlayersPerRoom;
            OccupancyRateText_ForOutdoors.text = 0 + "/" + maxPlayersPerRoom;
        }

        foreach (RoomInfo room in roomList)
        {
            Debug.Log("Info for room: " + room.Name);
            // Warning: Custom properties of a room can only be accessed from inside the room.
            if (room.Name.Contains(MultiplayerVRConstants.MAP_TYPE_VALUE_OUTDOOR))
            {
                // it's an outdoor map room
                // Update occupancy text.
                OccupancyRateText_ForOutdoors.text = room.PlayerCount + " / " + maxPlayersPerRoom;
            }
            else if (room.Name.Contains(MultiplayerVRConstants.MAP_TYPE_VALUE_CLASSROOM))
            { 
                OccupancyRateText_ForClassroom.text = room.PlayerCount + " / " + maxPlayersPerRoom;
            }
        }
    }

    public override void OnJoinedLobby()
    {
        //base.OnJoinedLobby();
        Debug.Log("MultiplayerRoomManager::OnJoinedLobby() - Joined to Lobby!");
    }
    #endregion

    #region Private Methods
    private void CreateAndJoinRoom()
    {
        // Generate random room name
        string randomRoomName = "Room_" + mapType + Random.Range(0,10000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayersPerRoom;


        // Propiedades Custom del Lobby donde Photon organiza los rooms.
        string[] roomPropsInLobby = { MultiplayerVRConstants.MAP_TYPE_KEY };
        roomOptions.CustomRoomPropertiesForLobby = roomPropsInLobby;
        
        // Creamos una hashTable para las
        // Propiedades Custom de las Rooms. (Hashtable key/value)
        // Escenas a utilizar en la room:
        // 1. HR_Outdoor = outdoor
        // 2. HR_ClassRoom = classroom
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { MultiplayerVRConstants.MAP_TYPE_KEY, mapType } };
        roomOptions.CustomRoomProperties = customRoomProperties;


        // Automated lobby
        PhotonNetwork.CreateRoom(randomRoomName, roomOptions);
    }
    #endregion
}
