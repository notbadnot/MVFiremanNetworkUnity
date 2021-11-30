using System;
using Photon.Pun;
using UnityEngine;
using View.Components;

namespace Game
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public event Action<bool, string> RoomJoinEvent;
        public event Action ConnectedEvent;
        public event Action ModelChangedEvent;

        [SerializeField]private SharedModel _sharedModel;

        public int FiremanId => _sharedModel.FiremanID;
        public GameState GameState => _sharedModel.GameState;
        public bool IsMaster => PhotonNetwork.LocalPlayer.IsMasterClient;
        
        public void Start()
        {
            _sharedModel.ChangedEvent += () =>
            {
                ModelChangedEvent?.Invoke();
            };
        }

        public void Connect()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public void CreateGame(string room)
        {
            PhotonNetwork.LocalPlayer.CustomProperties["PlayerName"] = PlayerPrefs.GetString("PlayerName");
            PhotonNetwork.CreateRoom(room);
        }

        public void FindRandomRoom()
        {
            PhotonNetwork.LocalPlayer.CustomProperties["PlayerName"] = PlayerPrefs.GetString("PlayerName");
            PhotonNetwork.JoinRandomRoom();
        }

        public PlayerController CreatePlayer(string prefabName, Vector3 position, Quaternion rotation)
        {
            var player = PhotonNetwork.Instantiate(prefabName, position, rotation);
            var playerController = player.GetComponent<PlayerController>();
            
            return playerController;
        }
        
        public void SetFireman(int id)
        {
            photonView.RPC(nameof(SetFiremanRpc), RpcTarget.MasterClient, id);
        }

        [PunRPC]
        private void SetFiremanRpc(int id)
        {
            _sharedModel.SetFireman(id);
        }

        public void StartGame()
        {
            photonView.RPC(nameof(ChangeState), RpcTarget.MasterClient, GameState.Play);
        }
        
        public void EndGame()
        {
            photonView.RPC(nameof(ChangeState), RpcTarget.MasterClient, GameState.End);
        }
        
        [PunRPC]
        private void ChangeState(GameState state)
        {
            _sharedModel.SetState(state);
        }
        
        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster");
            ConnectedEvent?.Invoke();
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
            RoomJoinEvent?.Invoke(true, string.Empty);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("OnCreateRoomFailed");
            RoomJoinEvent?.Invoke(false, "message");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("OnCreateRoomFailed");
            RoomJoinEvent?.Invoke(false, message);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRandomFailed");
            RoomJoinEvent?.Invoke(false, message);
        }
    }
}