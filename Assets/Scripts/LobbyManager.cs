using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI lobbyNameField;
    [SerializeField] private TextMeshProUGUI lobbyCodeField;

    ConcurrentQueue<string> createdLobbyIds = new();

    public async void CreateLobby(bool isPrivate)
    {
        string lobbyName = lobbyNameField.text;
        int maxPlayers = 2;
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = isPrivate;
        options.Player = GetPlayer();

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            createdLobbyIds.Enqueue(lobby.Id);

            //Calls the HeartbeatLobby method every 15s to keep the lobby active
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
            SceneManager.LoadScene("Lobby");
            Debug.Log(lobby.LobbyCode + " " + lobby.IsPrivate);
        }
        catch(LobbyServiceException e) { Debug.LogException(e); }
    }
    
    public async void JoinLobbyByCode()
    {
        try
        {
            string lobbyCode = lobbyCodeField.text.Trim('\u200b');
            await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, 
                new JoinLobbyByCodeOptions(){Player = GetPlayer()});
            SceneManager.LoadScene("Lobby");
        }
        catch(LobbyServiceException e) { Debug.LogException(e); }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync(new QuickJoinLobbyOptions(){Player = GetPlayer()});
            SceneManager.LoadScene("Lobby");
        }
        catch(LobbyServiceException e) { Debug.LogException(e); }
    }

    private Player GetPlayer()
    {
        return new Player(
            id: AuthenticationService.Instance.PlayerId,
            profile: new PlayerProfile(name: AuthenticationService.Instance.PlayerName)
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
    
    //Deletes all created lobbies by a player when they quit
    void OnApplicationQuit()
    {
        while (createdLobbyIds.TryDequeue(out string lobbyId))
        {
            try
            {
                LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            } catch(LobbyServiceException e) {Debug.LogException(e);}
        }
    }
}
