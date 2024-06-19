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
        /// <summary>
        /// Key: Player ID from AuthenticationService.Instance.PlayerId
        /// </summary>
        private Dictionary<string, LobbyPlayer> _otherPlayers = new();
        
        private List<MoveAnimation> _runningAnimations = new();
        
        private const float PlayerElementSpacing = 400.0f;
        private const float FirstPlayerElementMarginLeft = 20f;
        
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
                foreach (var otherPlayer in _otherPlayers)
                {
                    Destroy(otherPlayer.Value.gameObject);
                }
                _otherPlayers.Clear();
                gameObject.SetActive(false);
                onComplete?.Invoke();
            };
            UIManager.Instance.ToggleProfileButton();
            thisPlayerWindow.Exit();
            bottomWindow.Exit();
            playersWindow.Exit(callback);
        }
        
        public void StartGame()
        {
            if(LobbyManager.Instance.IsHost)
                LobbyManager.Instance.BeginStartingGameAsHost();
            else
                thisPlayerWindow.thisPlayer.ToggleReady();
        }

        public void RefreshUI()
        {
            if(LobbyManager.Instance.JoinedLobby == null) return;
            if(LobbyManager.Instance.ThisPlayer == null) return;
            RefreshStartButton();
            UpdatePlayers();
            Dictionary<string, Rearrangement> rearrangements = GetRearrangements();
            Rearrange(rearrangements);
            LobbyPlayer thisPlayer = thisPlayerWindow.thisPlayer;
            if (LobbyManager.Instance.GameStarted && thisPlayer.IsReady && !thisPlayer.IsHost)
            {
                LobbyManager.Instance.TryConnectToGame();
            }
        }

        private void Rearrange(Dictionary<string, Rearrangement> rearrangements)
        {
            foreach (var rearrangement in rearrangements)
            {
                LobbyPlayer player;
                if (rearrangement.Value.ToBeInitialized)
                {
                    player = Instantiate(playerPrefab, playersContainer.transform);
                    player.SetPlayer(LobbyManager.Instance.JoinedLobby!.Players.Find(p => p.Id == rearrangement.Key));
                    _otherPlayers[rearrangement.Key] = player;
                }
                else
                    player = _otherPlayers[rearrangement.Key];

                if (!rearrangement.Value.Necessary) continue;
                MoveAnimation moveAnimation = rearrangement.Value.GetAnimation();
                _runningAnimations.Add(moveAnimation);
                moveAnimation.Start(player.GetComponent<RectTransform>(), () =>
                {
                    _runningAnimations.Remove(moveAnimation);
                    if (rearrangement.Value.DestroyOnComplete)
                    {
                        Destroy(player.gameObject);
                        _otherPlayers.Remove(rearrangement.Key);
                    }
                });
            }
        }

        private Dictionary<string, Rearrangement> GetRearrangements()
        {
            Dictionary<string, Rearrangement> rearrangements = new();
            //Remove players that left the lobby
            foreach (var player in _otherPlayers)
            {
                bool playerLeft = LobbyManager.Instance.JoinedLobby?.Players.Any(p => p.Id == player.Key) != true;
                if (playerLeft)
                {
                    Vector3 currentPosition = player.Value.transform.localPosition;
                    var rearrangementLeftPlayer = rearrangements.TryGetValue(player.Key, out Rearrangement existingRearrangement)
                        ? existingRearrangement : new Rearrangement(player.Key, currentPosition);
                    rearrangementLeftPlayer.ToY = -UIManager.VerticalSlideDistance;
                    rearrangementLeftPlayer.DestroyOnComplete = true;
                    rearrangements[player.Key] = rearrangementLeftPlayer;
                    //Slide other players to the left
                    var playersToSlide = _otherPlayers.Where(p =>
                    {
                        bool isToTheRight = p.Value.transform.localPosition.x > currentPosition.x;
                        return p.Key != player.Key && isToTheRight;
                    });
                    foreach (var otherPlayer in playersToSlide)
                    {
                        Vector3 position = otherPlayer.Value.transform.localPosition;
                        var slideLeftRearrangement = rearrangements.TryGetValue(otherPlayer.Key, out Rearrangement existing)
                            ? existing : new Rearrangement(otherPlayer.Key, position);
                        slideLeftRearrangement.ToX -= PlayerElementSpacing;
                        rearrangements[otherPlayer.Key] = slideLeftRearrangement;
                    }
                }
            }
            //Add new players
            List<Player> players = LobbyManager.Instance.JoinedLobby!.Players;
            foreach (var player in players)
            {
                if(player.Id == LobbyManager.Instance.ThisPlayer!.Id) continue;
                bool playerExists = _otherPlayers.ContainsKey(player.Id);
                if (!playerExists)
                {
                    var newPlayer = new Rearrangement(player.Id, new Vector3(FirstPlayerElementMarginLeft, -UIManager.VerticalSlideDistance, 0))
                    {
                        ToBeInitialized = true,
                        ToY = 0,
                        ToX = FirstPlayerElementMarginLeft
                    };
                    rearrangements.Add(player.Id, newPlayer);
                    //Slide other players to the right
                    foreach (var otherPlayer in _otherPlayers)
                    {
                        Vector3 position = otherPlayer.Value.transform.localPosition;
                        var slideRightRearrangement = rearrangements.TryGetValue(otherPlayer.Key, out Rearrangement existing)
                            ? existing : new Rearrangement(otherPlayer.Key, position);
                        slideRightRearrangement.ToX += PlayerElementSpacing;
                        rearrangements[otherPlayer.Key] = slideRightRearrangement;
                    }
                    foreach (var rearrangement in rearrangements)
                    {
                        if (rearrangement.Value.ToBeInitialized && rearrangement.Key != player.Id)
                        {
                            var adjustedRearrangement = rearrangement.Value;
                            adjustedRearrangement.ToX += PlayerElementSpacing;
                            rearrangements[rearrangement.Key] = adjustedRearrangement;
                        }
                    }
                }
            }
            return rearrangements;
        }

        private struct Rearrangement
        {
            public string PlayerId { get; set; }
            public float FromX { get; set; }
            public float FromY { get; set; }
            public float ToX { get; set; }
            public float ToY { get; set; }
            public bool DestroyOnComplete { get; set; }
            public bool ToBeInitialized { get; set; }
            
            public bool Necessary => Math.Abs(FromX - ToX) > 0.1 || Math.Abs(FromY - ToY) > 0.1;
            public MoveAnimation GetAnimation()
            {
                return new MoveAnimation
                {
                    Duration = UIManager.Instance.HoverBaseDuration,
                    From = new Vector3(FromX, FromY),
                    To = new Vector3(ToX, ToY)
                };
            }

            public Rearrangement(string playerId, Vector3 pos)
            {
                PlayerId = playerId;
                FromX = pos.x;
                FromY = pos.y;
                ToX = pos.x;
                ToY = pos.y;
                DestroyOnComplete = false;
                ToBeInitialized = false;
            }
        }
        
        
        /// <summary>
        /// Checks if there are any changes to the player data every 1.1s and sends them to the cloud
        /// </summary>
        private void Update()
        {
            Lobby lobby = LobbyManager.Instance.JoinedLobby;
            if (lobby == null) return;
            if(LobbyManager.Instance.ThisPlayer == null) return;
        
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
            foreach (var lobbyPlayer in _otherPlayers)
            {
                Player player = LobbyManager.Instance.JoinedLobby?.Players.Find(p => p.Id == lobbyPlayer.Key);
                if (player != null)
                    lobbyPlayer.Value.SetPlayer(player);
            }
            var thisPlayer = LobbyManager.Instance.ThisPlayer;
            if(thisPlayer != null)
                thisPlayerWindow.thisPlayer.SetPlayer(thisPlayer);
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
