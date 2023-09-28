using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameField;

    ConcurrentQueue<string> createdLobbyIds = new();
    
    public async void CreateLobby(bool isPrivate)
    {
        string lobbyName = lobbyNameField.text;
        int maxPlayers = 2;
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = isPrivate;

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            createdLobbyIds.Enqueue(lobby.Id);

            //Calls the HeartbeatLobby method every 15s to keep the lobby active
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
            SceneManager.LoadScene("Lobby");
        }
        catch(LobbyServiceException e) { Debug.LogException(e); }
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
    
    //Deletes all creates lobbies by a player when they quit
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
