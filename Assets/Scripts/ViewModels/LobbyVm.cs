using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Models;
using Services;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ViewModels
{
    /// <summary>
    /// Provides access to UI elements in the Lobby Scene
    /// </summary>
    public class LobbyVm : MonoBehaviour
    {
        #region fields
        //Placeholders and buttons in the lobby scene

        [SerializeField] private GameObject mainCanvas;
    
        [SerializeField] private TextMeshProUGUI lobbyName;

        [SerializeField] private GameObject lobbyCodeField;

        [SerializeField] private GameObject btnReady;

        [SerializeField] private GameObject btnStartGame;
        
        [SerializeField] private GameObject btnLeave;

        [SerializeField] private Transform playerTable;

        [SerializeField] private Transform btnChangeColor;

        private Button _buttonReady;
        private Button _buttonStart;
        private Button _buttonLeave;
        
        private const float SpaceBetweenTableRows = 160f;
    
        private TMP_InputField _lobbyCodeInputField;

        private int _thisPlayer;
        private const int PlayerCount = 4;
        private List<LobbyPlayerVm> _players = new();
        private LobbyPlayerVm ThisPlayerVm => _players[_thisPlayer];
        private Color _thisPlayerColor;
        private bool _thisPlayerReady;

        
        private Color ThisPlayerColor
        {
            get => _thisPlayerColor;
            set
            {
                _thisPlayerColor = value;
                ThisPlayerVm.Color = value;
            }
        }
        private bool ThisPlayerReady
        {
            get => _thisPlayerReady;
            set
            {
                _thisPlayerReady = value;
                ThisPlayerVm.SetReady(value);
            }
        }
    
        //When something about the player changed, all other players in the lobby need to be notified
        //This requires sending the update info into the internet
        //Unity limits update player requests to 5 requests in 5 seconds, so to be safe, this timer limits
        //update requests to 1 request per 1.1 seconds
        //https://docs.unity.com/ugs/en-us/manual/lobby/manual/rate-limits
        private const float UpdatePlayerInterval = 1.1f;
        private float _updatePlayerTimer = UpdatePlayerInterval;
        private const float BtnChangeColorY = 320f;

        #endregion

        #region methods

        private void Awake()
        {
            _buttonReady = btnReady.GetComponent<Button>();
            _buttonStart = btnStartGame.GetComponent<Button>();
            _buttonLeave = btnLeave.GetComponent<Button>();
            _lobbyCodeInputField = lobbyCodeField.GetComponent<TMP_InputField>();
        }

        private void Start()
        {
            for (int i = 0; i < PlayerCount; i++)
            {
                _players.Add(playerTable.GetChild(i).GetComponent<LobbyPlayerVm>());
            }
            _thisPlayer = LobbyManager.Instance.JoinedLobby!.Players.IndexOf(LobbyManager.Instance.ThisPlayer);
            _players[_thisPlayer].SetPlayer(LobbyManager.Instance.ThisPlayer);
            _thisPlayerColor = ThisPlayerVm.Color;
            _thisPlayerReady = ThisPlayerVm.IsReady;
            UpdateLobby();
            LobbyManager.Instance.JoinedLobbyChanged += UpdateLobby;
            LobbyManager.Instance.PlayerKicked += OnPlayerKicked;
            LobbyManager.Instance.LobbyStateChanged += OnLobbyStateChanged;
            LobbyManager.Instance.LobbyLeft += OnLobbyLeft;
            LobbyManager.Instance.Busy += OnLobbyManagerBusy;
            LobbyManager.Instance.NoLongerBusy += OnLobbyManagerNoLongerBusy;
        }

        

        private void OnDestroy()
        {
            LobbyManager.Instance.JoinedLobbyChanged -= UpdateLobby;
            LobbyManager.Instance.PlayerKicked -= OnPlayerKicked;
            LobbyManager.Instance.LobbyStateChanged -= OnLobbyStateChanged;
            LobbyManager.Instance.LobbyLeft -= OnLobbyLeft;
            LobbyManager.Instance.Busy -= OnLobbyManagerBusy;
            LobbyManager.Instance.NoLongerBusy -= OnLobbyManagerNoLongerBusy;
        }

        #region Event Handlers

        private void OnLobbyManagerNoLongerBusy()
        {
            _buttonReady.enabled = true;
            _buttonStart.enabled = true;
            _buttonLeave.enabled = true;
        }

        private void OnLobbyManagerBusy()
        {
            _buttonReady.enabled = false;
            _buttonStart.enabled = false;
            _buttonLeave.enabled = false;
        }
        

        private void OnLobbyLeft() => SceneLoader.LoadScene(Scenes.MainMenu);

        private void OnLobbyStateChanged()
        {
            LobbyState newState = LobbyManager.Instance.State;
            if(newState == LobbyState.Starting)
                LoadingScreen.Instance.DisplayLoadingScreen("Starting game");
            if(newState == LobbyState.Started)
                LoadingScreen.Instance.CloseLoadingScreen();
        }

        private void OnPlayerKicked()
        {
            PopupBox.Instance.DisplayInfo("You were kicked from the lobby", () => SceneLoader.LoadScene(Scenes.MainMenu));
        }

        #endregion


        #region UI Event Handlers

        public void ChangeColor()
        {
            Color newColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            ThisPlayerColor = newColor;
        }

        public void LeaveLobby() => LobbyManager.Instance.LeaveLobby();
    
        public void StartGame()
        {
            Lobby lobby = LobbyManager.Instance.JoinedLobby;
            if (lobby == null) return;
            //Start the game, pass in both players
            //Leave the lobbies with both players, load the match scene
            Debug.Log("Starting game");
            LobbyManager.Instance.BeginStartingGameAsHost();
        }

        /// <summary>
        /// Changes the local ready status
        /// </summary>
        public void SetReady()
        {
            Lobby lobby = LobbyManager.Instance.JoinedLobby;
            if(lobby == null) return;
            //The host doesn't have a ready status, they just click on start when they're ready
            if (LobbyManager.Instance.IsHost) return;
            ThisPlayerReady = !_thisPlayerReady;
        }
        
        #endregion

        
    
        /// <summary>
        /// Checks if there are any changes to the player data every 1.1s and sends them to the cloud
        /// </summary>
        private void Update()
        {
            Lobby lobby = LobbyManager.Instance.JoinedLobby;
            if (lobby == null) return;
        
            if (_updatePlayerTimer <= 0)
            {
                Dictionary<string, PlayerDataObject> changes = GetPlayerChanges();
                if(changes.Count > 0) LobbyManager.Instance.UpdatePlayer(changes);
                //Reset the timer
                _updatePlayerTimer = UpdatePlayerInterval;
            }
            //Count down the timer
            else _updatePlayerTimer -= Time.deltaTime;
        }

        /// <summary>
        /// Compares the local data of this player with the data in the cloud and returns the differences
        /// </summary>
        private Dictionary<string, PlayerDataObject> GetPlayerChanges()
        {
            Dictionary<string, PlayerDataObject> changes = new();
        
            //If the color of this player in the cloud is different from the local color, add it to the changes
            string playerColor = "#" + ColorUtility.ToHtmlStringRGB(ThisPlayerVm.Color);
            if(LobbyManager.Instance.ThisPlayer!.Data[LobbyManager.PlayerColorProperty].Value != playerColor)
            {
                changes.Add(LobbyManager.PlayerColorProperty, new PlayerDataObject
                    (PlayerDataObject.VisibilityOptions.Member, playerColor));
            }
            //If this is the joined player, and the ready status in the cloud
            //is different from the local ready status, add it to the changes
            if(!LobbyManager.Instance.IsHost && LobbyManager.Instance.ThisPlayer.Data[LobbyManager.PlayerIsReadyProperty].Value != 
               ThisPlayerVm.IsReady.ToString())
            {
                changes.Add(LobbyManager.PlayerIsReadyProperty, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
                    ThisPlayerVm.IsReady.ToString()));
            }
            return changes;
        }

       
    
        /// <summary>
        /// Called after something in the lobby changed (after every poll = every 1.1s by default)
        /// </summary>
        private void UpdateLobby()
        {
            Lobby lobby = LobbyManager.Instance.JoinedLobby;
            if (lobby == null || LobbyManager.Instance.ThisPlayer == null) return;
        
            _thisPlayer = LobbyManager.Instance.JoinedLobby!.Players.IndexOf(LobbyManager.Instance.ThisPlayer);
        
            //Updating the UI:
            lobbyName.text = lobby.Name;
            //Updating the position of the change color button
            var position = btnChangeColor.localPosition;
            position = new Vector3(position.x, _thisPlayer * -SpaceBetweenTableRows + BtnChangeColorY, position.z);
            btnChangeColor.localPosition = position;
        
            _lobbyCodeInputField.text = lobby.LobbyCode;
            //The host doesn't have a ready status, they just click on start when they're ready
            btnReady.SetActive(LobbyManager.Instance.IsHost == false);
            //Only the host can start the game
            btnStartGame.SetActive(LobbyManager.Instance.IsHost);
            UpdatePlayers();
            if ((LobbyManager.Instance.State is LobbyState.Started or LobbyState.Starting) && _thisPlayerReady)
            {
                LobbyManager.Instance.TryConnectToGame();
            }
        }

        private void UpdatePlayers()
        {
            for (var i = 0; i < _players.Count; i++)
            {
                var player = _players[i];
                if (LobbyManager.Instance.JoinedLobby!.Players.Count <= i)
                {
                    player.gameObject.SetActive(false);
                }
                else
                {
                    player.gameObject.SetActive(true);
                    player.SetPlayer(LobbyManager.Instance.JoinedLobby!.Players[i]);
                }
                if(i != _thisPlayer) continue;
                ThisPlayerVm.Color = ThisPlayerColor;
                ThisPlayerVm.SetReady(LobbyManager.Instance.IsHost || ThisPlayerReady);
            }
            
        }

        

        #endregion
    }
}
