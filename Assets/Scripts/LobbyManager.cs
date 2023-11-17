using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LobbyManager : MonoBehaviour
{
    //There is only one lobby manager in the game per player
    /// <summary>
    /// The singleton instance of this class
    /// </summary>
    public static LobbyManager Instance { get; private set; }
    
    
    /// <summary>
    /// The lobby this player is currently in
    /// </summary>
    [CanBeNull] private Lobby _joinedLobby;
    
    [CanBeNull]
    public Lobby JoinedLobby
    {
        get => _joinedLobby;
        set
        {
            _joinedLobby = value;
            OnJoinedLobbyChanged();
        }
    }

    /// <summary>
    /// Is called when the player joins or leaves a lobby
    /// </summary>
    public event EventHandler JoinedLobbyChanged;
    
    /// <summary>
    /// The input field in the lobby creation menu storing the name for the lobby to be created
    /// </summary>
    [SerializeField] private TextMeshProUGUI lobbyNameField;
    
    /// <summary>
    /// The input field in the lobby joining menu storing the lobby code to join
    /// </summary>
    [SerializeField] private TextMeshProUGUI lobbyCodeField;
    
    private const string LobbySceneName = "Lobby";
    private const string MainMenuSceneName = "MainMenu";
    
    /*Lobbies are stored somewhere in the internet and so all users need to constantly poll for updates
     The limit for requests by unity are 1 per second*/
    private const float PollingForLobbyUpdatesInterval = 1.1f;
    private float _pollingForLobbyUpdatesTimer = PollingForLobbyUpdatesInterval;

    
    //Assigns the singleton instance of this class and makes sure it is not destroyed on scene change
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        HandlePollingForLobbyUpdates();
    }


    /// <summary>
    /// Creates a new lobby with the name given in the input field
    /// </summary>
    /// <param name="isPrivate">If the lobby is private: <c>true</c> or public: <c>false</c></param>
    public async void CreateLobby(bool isPrivate)
    {
        string lobbyName = lobbyNameField.text;
        int maxPlayers = 2;
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = GetPlayer()
        };

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            JoinedLobby = lobby;

            //Calls the HeartbeatLobby method every 15s to keep the lobby active
            StartCoroutine(HeartbeatLobbyCoroutine(JoinedLobby!.Id, 15));
            SceneManager.LoadScene(LobbySceneName);
            Debug.Log($"Created lobby {lobbyName} with id {lobby.Id} and code {lobby.LobbyCode}");
        }
        catch(LobbyServiceException e) { Debug.LogException(e); }
    }
    
    /// <summary>
    /// Joins a lobby with the given lobby code in the input field
    /// </summary>
    public async void JoinLobbyByCode()
    {
        try
        {
            string lobbyCode = lobbyCodeField.text.Trim('\u200b');
            JoinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, 
                new JoinLobbyByCodeOptions()
                {
                    Player = GetPlayer()
                });
            SceneManager.LoadScene(LobbySceneName);
        }
        catch(LobbyServiceException e) { Debug.LogException(e); }
    }

    /// <summary>
    /// Joins a random public lobby
    /// </summary>
    public async void QuickJoinLobby()
    {
        try
        {
            JoinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(new QuickJoinLobbyOptions()
            {
                Player = GetPlayer()
            });
            SceneManager.LoadScene(LobbySceneName);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }
    
    /// <summary>
    /// Leaves the lobby the player is currently in and loads the previous scene
    /// </summary>
    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby!.Id, AuthenticationService.Instance.PlayerId);
            JoinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }
    
    public async void KickPlayer(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby!.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }
    
    /// <summary>
    /// Polls for lobby updates every <see cref="PollingForLobbyUpdatesInterval"/> seconds
    /// </summary>
    private void HandlePollingForLobbyUpdates()
    {
        if (JoinedLobby == null) return;
        _pollingForLobbyUpdatesTimer -= Time.deltaTime;
        if (_pollingForLobbyUpdatesTimer < 0)
        {
            _pollingForLobbyUpdatesTimer = PollingForLobbyUpdatesInterval;
            PollForLobbyUpdates();
        }
    }
    
    private async void PollForLobbyUpdates()
    {
        try
        {
            JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby!.Id);
            if (JoinedLobby != null && 
                !JoinedLobby.Players.Exists(player => player.Id == AuthenticationService.Instance.PlayerId))
                JoinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }
    
    /// <summary>
    /// Creates a <see cref="Player"/> object with the current player's id and name
    /// </summary>
    private Player GetPlayer()
    {
        return new Player(
            id: AuthenticationService.Instance.PlayerId,
            data: new Dictionary<string, PlayerDataObject>()
            {
                {"name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerName)}
            }
        );
    }

    
    //Resets the 30s timer for the lobby with the given id to keep it in the active state
    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            try
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            } catch(LobbyServiceException e) { Debug.LogException(e); }

            yield return delay;
        }
    }

    
    protected virtual void OnJoinedLobbyChanged()
    {
        if (JoinedLobby == null)
        {
            SceneManager.LoadScene(MainMenuSceneName);
            return;
        }
        if(JoinedLobby != null && SceneManager.GetActiveScene().name != LobbySceneName)
            SceneManager.LoadScene(LobbySceneName);
        
        JoinedLobbyChanged?.Invoke(this, EventArgs.Empty);
    }
    
}
