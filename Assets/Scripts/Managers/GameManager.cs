using System;
using System.Collections;
using System.Collections.Generic;
using Collectables;
using Inventories;
using JetBrains.Annotations;
using Models;
using Services;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private Transform playerPrefab;
        [SerializeField] private Image overlay;
        [SerializeField] private GameObject optionsCanvas;
        [SerializeField] private TMP_InputField lobbyCodeInput;
        [SerializeField] private GameObject lobbyCodeLabel;

        public Vector3? SpawnPoint { get; private set; }
        public static GameManager Instance { get; private set; }
        
        [CanBeNull] public event Action<Transform> ThisPlayerSpawned;
        public Transform ThisPlayer { get; private set; }
        
        
        private NetworkList<PlayerId> _connectedPlayers;

        private void Awake()
        {
            Instance = this;
            _connectedPlayers = new NetworkList<PlayerId>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                optionsCanvas.SetActive(!optionsCanvas.gameObject.activeSelf);
                Cursor.lockState = optionsCanvas.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }
        
        public void LeaveGame()
        {
            Cursor.lockState = CursorLockMode.None;
            if(LobbyManager.Instance.JoinedLobby != null)
                LobbyManager.Instance.LeaveLobby();
            Fade(true, BackToMenu);
        }
        
        public void ResumeGame()
        {
            optionsCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }


        public void Fade(bool fadeOut, Action onComplete = null)
        {
            overlay.color = fadeOut ? Color.clear : Color.black;
            overlay.gameObject.SetActive(true);
            LeanTween.alpha(overlay.rectTransform, fadeOut ? 1 : 0, 3f).setOnComplete(() =>
            {
                if (!fadeOut) overlay.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
        
        public void RegisterThisPlayer(Transform thisPlayer)
        {
            ThisPlayer = thisPlayer;
            OnThisPlayerSpawned(thisPlayer);
        }

        private async void BackToMenu()
        {
            NetworkManager.Shutdown();
            await SceneLoader.LoadSceneAsync(Scenes.Menu);
            PopupBox.Instance.DisplayInfo("Host left the game.");
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void OnObjectDestroyedServerRpc(NetworkObjectReference collectable)
        {
            //get network object from reference
            if (collectable.TryGet(out var networkObject))
            {
                ICollectable collectableComponent = networkObject.GetComponent<ICollectable>();
                var itemToDrop = Items.Singleton.Get(collectableComponent.Stats.DroppedItem);
                var itemDropCount = collectableComponent.ItemDropCount;
                var position = networkObject.transform.position;
                networkObject.Despawn();
                for (int i = 0; i < itemDropCount; i++)
                {
                    var layerMask = LayerMask.GetMask("Default");
                    var randomOffset = new Vector3(UnityEngine.Random.Range(-2f, 2f), 20, UnityEngine.Random.Range(-2f, 2f));
                    if(!Physics.Raycast(position + randomOffset, Vector3.down, out var hit, Mathf.Infinity, layerMask)) continue;
                    var item = Instantiate(itemToDrop.Model, hit.point, Quaternion.identity);
                    item.layer = LayerMask.NameToLayer("Collectables");
                    item.GetComponent<ItemVm>().IsDropped = true;
                    item.GetComponent<NetworkObject>().Spawn();
                }
            }
            else throw new Exception("Failed to get network object from reference");
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log("Game manager network spawned");
            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
            TerrainGenerator.Instance.GenerateSpawn();
            if(LobbyManager.Instance.JoinedLobby == null) return;
            lobbyCodeInput.text = LobbyManager.Instance.JoinedLobby.LobbyCode;
            lobbyCodeLabel.SetActive(true);
            lobbyCodeInput.gameObject.SetActive(true);
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            RegisterPlayerServerRpc(NetworkManager.LocalClientId, LobbyManager.Instance.ThisPlayer!.Id);

        }

        private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, 
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            Debug.Log("Scene loaded: " + sceneName);
            StartCoroutine(GenerateSpawnPoint());
        }
        
        
        private void OnClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.ServerClientId)
            {
                LeaveGame();
                return;
            }

            if (!NetworkManager.IsHost) return;
            PlayerId playerId = GetPlayerId(clientId)!.Value;
            LobbyManager.Instance.KickPlayer(playerId.LobbyPlayerId.Value);
            _connectedPlayers.Remove(playerId);
        }
        
        private void SpawnPlayers()
        {
            Debug.Log("Spawn players...");
            foreach (var connectedPlayer in _connectedPlayers)
            {
                SpawnPlayer(connectedPlayer.ClientId);
            }
        }

        private void SpawnPlayer(ulong clientId)
        {
            if(NetworkManager.SpawnManager.GetPlayerNetworkObject(clientId) != null) return;
            Debug.Log("Spawning player...");
            var player = Instantiate(playerPrefab, SpawnPoint.Value, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            if(LobbyManager.Instance.JoinedLobby != null)
                AssignNickAndColorOnNetwork(clientId);
        }
        
        private IEnumerator GenerateSpawnPoint()
        {
            yield return new WaitForSeconds(3);
            Debug.Log("Finding spawn point...");
            var rayStart = new Vector3(0, 250, 0);

            if (!Physics.Raycast(rayStart, Vector3.down, out var hit, Mathf.Infinity))
                throw new Exception("Failed to find spawn point");
            SpawnPoint = hit.point + Vector3.up * 3;
            if(LobbyManager.Instance.JoinedLobby != null)
                SpawnPlayers();
            else SpawnPlayer(NetworkManager.LocalClientId);
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
        
        [ServerRpc(RequireOwnership = false)]
        private void RegisterPlayerServerRpc(ulong clientId, FixedString64Bytes lobbyPlayerId)
        {
            _connectedPlayers.Add(new PlayerId(lobbyPlayerId, clientId));
            SpawnPlayer(clientId);
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

        protected virtual void OnThisPlayerSpawned(Transform obj)
        {
            ThisPlayerSpawned?.Invoke(obj);
        }
    }
}
