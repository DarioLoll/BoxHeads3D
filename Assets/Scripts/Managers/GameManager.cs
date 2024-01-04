using System;
using System.Collections.Generic;
using Models;
using Services;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private Transform playerPrefab;
        public Vector3 spawnPoint;

        private NetworkList<PlayerId> _connectedPlayers;

        private void Awake()
        {
            _connectedPlayers = new NetworkList<PlayerId>();
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            if (LobbyManager.Instance.ThisPlayer == null) return;
            RegisterPlayerServerRpc(NetworkManager.LocalClientId, LobbyManager.Instance.ThisPlayer.Id);
        }

        private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, 
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if(!IsServer) return;
            NetworkManager.OnClientConnectedCallback += SpawnPlayer;
            SpawnPlayers();
        }
        
        
        private async void OnClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.ServerClientId)
            {
                Cursor.lockState = CursorLockMode.None;
                LobbyManager.Instance.LeaveLobby();
                await SceneLoader.LoadSceneAsync(Scenes.MainMenu);
                PopupBox.Instance.DisplayInfo("Host left the game.");
                return;
            }

            if (!NetworkManager.IsHost) return;
            PlayerId playerId = GetPlayerId(clientId)!.Value;
            LobbyManager.Instance.KickPlayer(playerId.LobbyPlayerId.Value);
            _connectedPlayers.Remove(playerId);
        }
    

        private void SpawnPlayers()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients) 
                SpawnPlayer(client.Key);
        }

        private void SpawnPlayer(ulong clientId)
        {
            if(NetworkManager.SpawnManager.GetPlayerNetworkObject(clientId) != null) return;
            
            var player = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            AssignNickAndColorOnNetwork(clientId);
        }

        private void AssignNickAndColorOnNetwork(ulong clientId)
        {
            if(!IsServer) return;
            GamePlayer player = NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<GamePlayer>();
            Player lobbyPlayer = LobbyManager.Instance.GetPlayer(GetPlayerId(clientId)?.LobbyPlayerId.Value);
            if (lobbyPlayer == null) return;
            player.AssignNickAndColorOnNetworkServerRpc(lobbyPlayer.Data[LobbyManager.PlayerNameProperty].Value,
                lobbyPlayer.Data[LobbyManager.PlayerColorProperty].Value);
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
            NetworkManager.OnClientConnectedCallback -= SpawnPlayer;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RegisterPlayerServerRpc(ulong clientId, FixedString64Bytes lobbyPlayerId)
        {
            _connectedPlayers.Add(new PlayerId(lobbyPlayerId, clientId));
        }
        
        private PlayerId? GetPlayerId(ulong clientId)
        {
            foreach (PlayerId connectedPlayer in _connectedPlayers)
            {
                if (connectedPlayer.ClientId == clientId)
                    return connectedPlayer;
            }

            return null;
        }

        private PlayerId? GetPlayerId(string playerId)
        {
            foreach (PlayerId connectedPlayer in _connectedPlayers)
            {
                if (connectedPlayer.LobbyPlayerId.Value == playerId)
                    return connectedPlayer;
            }

            return null;
        }
        
    }
}
