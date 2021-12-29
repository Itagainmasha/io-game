using UnityEngine;
using Photon.Pun;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Tooltip("Меню подключения")]
    [SerializeField] private GameObject _connectingMenu; 

    private bool _connectedToMaster = false; /
    private bool _connectingToRoom = false; 

    
    private void Start()
    {
        PhotonNetworkStart(); 
    }

   
    private void PhotonNetworkStart()
    {
        PhotonNetwork.NickName = Random.Range(1000, 9999).ToString(); 
        PhotonNetwork.AutomaticallySyncScene = true; 
        PhotonNetwork.GameVersion = "1"; 
        PhotonNetwork.ConnectUsingSettings(); 
    }

   
    public void JoinRandomRoom()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
        if (_connectedToMaster && !_connectingToRoom)  
        {
            _connectingToRoom = true;  
            PhotonNetwork.JoinRandomRoom(); 
        }
    }

  
    public override void OnJoinedRoom()
    {
        _connectingToRoom = true; 
        PhotonNetwork.LoadLevel("Main"); 
    }

  
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        _connectingToRoom = false; 
        if (_connectedToMaster) 
            CreateRoom(); 
    }

 
    public override void OnCreatedRoom()
    {
        Debug.Log("Room is created"); 
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _connectingToRoom = false; 
        Debug.Log("Create room failed with exception: " + message.ToString()); 
    }

 
    public override void OnConnectedToMaster()
    {
        _connectingMenu.SetActive(false);
        _connectedToMaster = true; 
        Debug.Log("Connected to Master"); 
    }

   
    private void CreateRoom()
    {
        _connectingToRoom = true; 
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = 20}); 
    }
}