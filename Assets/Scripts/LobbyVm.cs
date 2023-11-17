using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Provides access to UI elements in the Lobby Scene
/// </summary>
public class LobbyVm : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private GameObject lobbyCodeField;
    [SerializeField] private TextMeshProUGUI hostPlayerName;
    [SerializeField] private TextMeshProUGUI joinedPlayerName;
    [SerializeField] private TextMeshProUGUI joinedPlayerReady;
    [SerializeField] private GameObject hostPlayer;
    [SerializeField] private GameObject joinedPlayer;
    [SerializeField] private GameObject btnReady;
    [SerializeField] private GameObject btnStartGame;
    [SerializeField] private GameObject btnKickPlayer;

    private TMP_InputField _lobbyCodeInputField;
    private bool _joinedPlayerIsReady = false;
    
    private void Start()
    {
        LobbyManager.Instance.JoinedLobbyChanged += UpdateLobby;
        _lobbyCodeInputField = lobbyCodeField.GetComponent<TMP_InputField>();
    }

    public void LeaveLobby() => LobbyManager.Instance.LeaveLobby();

    public void KickPlayer()
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        if (lobby == null) return;
        if (lobby.Players.Count <= 1) return;
        if (AuthenticationService.Instance.PlayerId != lobby.HostId) return;
        LobbyManager.Instance.KickPlayer(lobby.Players.Find(player => player.Id != lobby.HostId).Id);
    }

    public void SetReady()
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        if(lobby == null) return;
        if (AuthenticationService.Instance.PlayerId == lobby.HostId) return;
        _joinedPlayerIsReady = !_joinedPlayerIsReady;
        joinedPlayerReady.gameObject.SetActive(_joinedPlayerIsReady);
    }
    
    private void UpdateLobby(object sender, EventArgs e)
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        if (lobby == null || 
            //This player is not in the lobby
            !lobby.Players.Exists(player => player.Id == AuthenticationService.Instance.PlayerId)) 
            return;
        
        lobbyName.text = lobby.Name;
        _lobbyCodeInputField.text = lobby.LobbyCode;
        hostPlayerName.text = lobby.Players[0].Data["name"].Value;
        if(AuthenticationService.Instance.PlayerId == lobby.HostId)
        {
            btnReady.SetActive(false);
            btnStartGame.SetActive(true);
            btnKickPlayer.SetActive(true);
        }
        else
        {
            btnReady.SetActive(true);
            btnStartGame.SetActive(false);
            btnKickPlayer.SetActive(false);
        }

        if (lobby.Players.Count > 1)
        {
            joinedPlayerName.text = lobby.Players[1].Data["name"].Value;
            joinedPlayer.SetActive(true);
            joinedPlayerReady.gameObject.SetActive(_joinedPlayerIsReady);
        }
        else
        {
            joinedPlayerName.text = string.Empty;
            joinedPlayer.SetActive(false);
            joinedPlayerReady.gameObject.SetActive(_joinedPlayerIsReady);
        }
        
    }
}
