using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Managers;
using Models;
using TMPro;
using UI;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Windows
{
    public class LobbyWindow : AnimatableWindow
    {
        [SerializeField] ThisPlayerWindow thisPlayerWindow;
        [SerializeField] AnimatableWindow bottomWindow;
        [SerializeField] AnimatableWindow playersWindow;
        [SerializeField] GameObject playersContainer;
        [SerializeField] Button startButton;
        [SerializeField] LobbyPlayer playerPrefab;
        [SerializeField] TextMeshProUGUI lobbyCode;
        
        private TextMeshProUGUI _startButtonText;
        private List<LobbyPlayer> _lobbyPlayers = new();
        
        private const float PlayerElementSpacing = 400.0f;
        private const float FirstPlayerElementMarginLeft = 20f;

        private bool _refreshing = false;
        
        private const float UpdatePlayerInterval = 1.1f;
        private float _updatePlayerTimer;

        private void Awake()
        {
            _startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            RefreshStartButton();
            UpdatePlayers();
        }

        public override void Enter(Action onComplete = null)
        {
            Action callback = () =>
            {
                LobbyManager.Instance.JoinedLobbyChanged += RefreshUI;
                onComplete?.Invoke();
            };
            gameObject.SetActive(true);
            UIManager.Instance.ToggleProfileButton();
            thisPlayerWindow.Enter();
            bottomWindow.Enter();
            playersWindow.Enter(callback);
        }
        
        public override void Exit(Action onComplete = null)
        {
            LobbyManager.Instance.JoinedLobbyChanged -= RefreshUI;
            Action callback = () =>
            {
                gameObject.SetActive(false);
                for (int i = 0; i < playersContainer.transform.childCount; i++)
                {
                    Destroy(playersContainer.transform.GetChild(i).gameObject);
                }
                onComplete?.Invoke();
            };
            UIManager.Instance.ToggleProfileButton();
            thisPlayerWindow.Exit();
            bottomWindow.Exit();
            playersWindow.Exit(callback);
        }

        public async void RefreshUI()
        {
            if (_refreshing) return;
            if(LobbyManager.Instance.JoinedLobby == null) return;
            _refreshing = true;
            RefreshStartButton();
            //Remove players that left the lobby
            await RemoveAbsentPlayers();
            //Update players
            UpdatePlayers();
            //Add new players
            await AddNewPlayers();
            _refreshing = false;
        }
        
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
                if(changes.Count > 0) 
                    LobbyManager.Instance.UpdatePlayer(changes);
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
            string playerColor = "#" + ColorUtility.ToHtmlStringRGB(thisPlayerWindow.thisPlayer.Color);
            if(LobbyManager.Instance.ThisPlayer!.Data[LobbyManager.PlayerColorProperty].Value != playerColor)
            {
                changes.Add(LobbyManager.PlayerColorProperty, new PlayerDataObject
                    (PlayerDataObject.VisibilityOptions.Member, playerColor));
            }
            //If this is the joined player, and the ready status in the cloud
            //is different from the local ready status, add it to the changes
            if(!LobbyManager.Instance.IsHost && LobbyManager.Instance.ThisPlayer.Data[LobbyManager.PlayerIsReadyProperty].Value != 
               thisPlayerWindow.thisPlayer.IsReady.ToString())
            {
                changes.Add(LobbyManager.PlayerIsReadyProperty, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
                    thisPlayerWindow.thisPlayer.IsReady.ToString()));
            }
            return changes;
        }

        private void UpdatePlayers()
        {
            foreach (var lobbyPlayer in _lobbyPlayers)
            {
                Player player = LobbyManager.Instance.JoinedLobby?.Players.Find(p => p.Id == lobbyPlayer.Player.Id);
                lobbyPlayer.SetPlayer(player ?? lobbyPlayer.Player);
            }
            var thisPlayer = LobbyManager.Instance.ThisPlayer;
            thisPlayerWindow.thisPlayer.SetPlayer(thisPlayer);
        }

        private async Task AddNewPlayers()
        {
            List<Player> players = LobbyManager.Instance.JoinedLobby?.Players;
            if (players == null) return;
            foreach (var player in players)
            {
                if(player.Id == LobbyManager.Instance.ThisPlayer!.Id) continue;
                bool playerExists = _lobbyPlayers.Find(p => p.Player.Id == player.Id) != null;
                if (!playerExists)
                {
                    await AddPlayer(player);
                }
            }
        }
        
        private async Task AddPlayer(Player player)
        {
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            Dictionary<LobbyPlayer, bool> animationsFinished = new();
            LobbyPlayer lobbyPlayer = Instantiate(playerPrefab, playersContainer.transform);
            lobbyPlayer.transform.localPosition = new Vector3(FirstPlayerElementMarginLeft, -UIManager.VerticalSlideDistance, 0);
            lobbyPlayer.SetPlayer(player);
            _lobbyPlayers.Add(lobbyPlayer);
            UIManager.Instance.Animator.Slide(lobbyPlayer.gameObject, FirstPlayerElementMarginLeft, 0);
            //Slide other players to the right
            foreach (var otherPlayer in _lobbyPlayers)
            {
                if(otherPlayer == lobbyPlayer) continue;
                Vector3 position = otherPlayer.transform.localPosition;
                animationsFinished.Add(otherPlayer, false);
                UIManager.Instance.Animator.Slide
                    (otherPlayer.gameObject, position.x + PlayerElementSpacing, position.y, () =>
                    {
                        animationsFinished[otherPlayer] = true;
                        bool allFinished = animationsFinished.All(pair => pair.Value == true);
                        if(allFinished)
                            task.SetResult(true);
                    }, 0.5f);
            }
        }

        private async Task RemoveAbsentPlayers()
        {
            foreach (var player in _lobbyPlayers)
            {
                if(player.Player.Id == LobbyManager.Instance.ThisPlayer!.Id) continue;
                if (PlayerLeft(player))
                {
                    await RemovePlayer(player);
                }
            }
        }

        private async Task RemovePlayer(LobbyPlayer lobbyPlayer)
        {
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            UIManager.Instance.Animator.SlideOut(lobbyPlayer.gameObject, ExitingAnimation.SlideOutToBottom, () =>
            {
                Destroy(lobbyPlayer.gameObject);
            });
            //Slide other players to the left
            Dictionary<LobbyPlayer,bool> animationsFinished = new Dictionary<LobbyPlayer,bool>();
            _lobbyPlayers.ForEach(player =>
            {
                Vector3 position = player.transform.localPosition;
                bool isToTheRight = position.x > lobbyPlayer.transform.localPosition.x;
                if (isToTheRight)
                {
                    animationsFinished.Add(player,false);
                    UIManager.Instance.Animator.Slide
                        (player.gameObject,position.x - PlayerElementSpacing, position.y, () =>
                        {
                            animationsFinished[player] = true;
                            bool allFinished = animationsFinished.All(pair => pair.Value == true);
                            if(allFinished)
                                task.SetResult(true);
                        }, 0.5f);
                }
            });
            await task.Task;
        }

        private bool PlayerLeft(LobbyPlayer player)
        {
            return LobbyManager.Instance.JoinedLobby?.Players.Find(p => p.Id == player.Player.Id) == null;
        }

        private void RefreshStartButton()
        {
            lobbyCode.text = LobbyManager.Instance.JoinedLobby?.LobbyCode;
            if(LobbyManager.Instance.IsHost)
                _startButtonText.text = "Start Game";
            else if(LobbyManager.Instance.GameStarted)
                _startButtonText.text = "Join Game";
            else if(thisPlayerWindow.thisPlayer.IsReady)
                _startButtonText.text = "Not Ready";
            else
                _startButtonText.text = "Ready";
        }
    }
}
