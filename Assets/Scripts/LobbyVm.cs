using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Provides access to UI elements in the Lobby Scene
/// </summary>
public class LobbyVm : MonoBehaviour
{
    #region fields
    //Placeholders and buttons in the lobby scene
    [SerializeField]
    private TextMeshProUGUI lobbyName;

    [SerializeField]
    private GameObject lobbyCodeField;

    [SerializeField]
    private TextMeshProUGUI hostPlayerName;

    [SerializeField]
    private TextMeshProUGUI joinedPlayerName;

    [SerializeField]
    private TextMeshProUGUI joinedPlayerReady;

    [SerializeField]
    private GameObject hostPlayer;

    [SerializeField]
    private GameObject joinedPlayer;

    [SerializeField]
    private GameObject btnReady;

    [SerializeField]
    private GameObject btnStartGame;

    [SerializeField]
    private GameObject btnKickPlayer;

    private TMP_InputField _lobbyCodeInputField;
    
    /// <summary>
    /// Whether the player that joined the lobby (not the host) is ready for the game to start
    /// </summary>
    private bool _joinedPlayerIsReady = false;
    
    //When something about the player changed, all other players in the lobby need to be notified
    //This requires sending the update info into the internet
    //Unity limits update player requests to 5 requests in 5 seconds, so to be safe, this timer limits
    //update requests to 1 request per 1.1 seconds
    //https://docs.unity.com/ugs/en-us/manual/lobby/manual/rate-limits
    private const float UpdatePlayerInterval = 1.1f;
    private float _updatePlayerTimer = UpdatePlayerInterval;
    #endregion

    #region methods
    private void Start()
    {
        _lobbyCodeInputField = lobbyCodeField.GetComponent<TMP_InputField>();
        
        LobbyManager.Instance.JoinedLobbyChanged += UpdateLobby;
        UpdateLobby();
    }

    /// <summary>
    /// Checks if the local ready status of the joined player is the same as
    /// the one in the cloud, and if not, updates the cloud if at least 1.1s
    /// have passed since the last update
    /// </summary>
    private void Update()
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        //The host doesn't have a ready status, they just click on start when they're ready
        if (lobby == null || AuthenticationService.Instance.PlayerId == lobby.HostId) return;
        
        if (_updatePlayerTimer <= 0)
        {
            if(lobby.Players[1].Data[LobbyManager.PlayerIsReadyProperty].Value != _joinedPlayerIsReady.ToString())
                LobbyManager.Instance.UpdatePlayer(new Dictionary<string, PlayerDataObject>()
                {
                    {LobbyManager.PlayerIsReadyProperty, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
                        _joinedPlayerIsReady.ToString())}
                });
            //Reset the timer
            _updatePlayerTimer = UpdatePlayerInterval;
        }
        //Count down the timer
        else _updatePlayerTimer -= Time.deltaTime;
    }

    public void LeaveLobby() => LobbyManager.Instance.LeaveLobby();
    
    public void StartGame()
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        if (lobby == null) return;
        if (lobby.Players.Count != 2) return;
        if(lobby.Players[1].Data[LobbyManager.PlayerIsReadyProperty].Value != true.ToString()) return;
        //Start the game, pass in both players
        //Leave the lobbies with both players, load the match scene
        Debug.Log("Starting game");
    }

    /// <summary>
    /// Can only be called by the host of the lobby. Kicks the other player from the lobby
    /// </summary>
    public void KickPlayer()
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        if (lobby == null) return;
        if (lobby.Players.Count <= 1) return;
        if (AuthenticationService.Instance.PlayerId != lobby.HostId) return;
        LobbyManager.Instance.KickPlayer(lobby.Players.Find(player => player.Id != lobby.HostId).Id);
    }

    /// <summary>
    /// Changes the local ready status
    /// </summary>
    public void SetReady()
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        if(lobby == null) return;
        //The host doesn't have a ready status, they just click on start when they're ready
        if (AuthenticationService.Instance.PlayerId == lobby.HostId) return;
        _joinedPlayerIsReady = !_joinedPlayerIsReady;
        //Update the UI
        joinedPlayerReady.gameObject.SetActive(_joinedPlayerIsReady);
    }
    
    /// <summary>
    /// Called after something in the lobby changed (after every poll = every 1.1s by default)
    /// </summary>
    private void UpdateLobby(object sender = null, EventArgs e = default)
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        if (lobby == null || 
            //This player is not in the lobby
            !lobby.Players.Exists(player => player.Id == AuthenticationService.Instance.PlayerId)) 
            return;
        
        //Updating the UI:
        lobbyName.text = lobby.Name;
        _lobbyCodeInputField.text = lobby.LobbyCode;
        hostPlayerName.text = lobby.Players[0].Data[LobbyManager.PlayerNameProperty].Value;
        //The host doesn't have a ready status, they just click on start when they're ready
        btnReady.SetActive(AuthenticationService.Instance.PlayerId != lobby.HostId);
        //Only the host can start the game
        btnStartGame.SetActive(AuthenticationService.Instance.PlayerId == lobby.HostId);
        //Only the host can kick the other player
        btnKickPlayer.SetActive(AuthenticationService.Instance.PlayerId == lobby.HostId && lobby.Players.Count > 1);
        
        if (lobby.Players.Count > 1)
        {
            //Updating the second player
            joinedPlayer.SetActive(true); //Enabling the player prefab
            joinedPlayerName.text = lobby.Players[1].Data[LobbyManager.PlayerNameProperty].Value;
            //If this is the host, it needs to get the ready status of the joined player from the lobby data
            if (AuthenticationService.Instance.PlayerId == lobby.HostId)
            {
                joinedPlayerReady.gameObject.SetActive
                    (lobby.Players[1].Data[LobbyManager.PlayerIsReadyProperty].Value == true.ToString());
            }
            //If this is the joined player, it can get the ready status from the local variable
            else
            {
                joinedPlayerReady.gameObject.SetActive(_joinedPlayerIsReady);
            }
        }
        else
        {
            //The second player is hidden
            joinedPlayerName.text = string.Empty;
            joinedPlayer.SetActive(false);
            joinedPlayerReady.gameObject.SetActive(false);
        }
    }
    #endregion
}
