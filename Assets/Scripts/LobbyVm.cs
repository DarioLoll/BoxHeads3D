using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

    [SerializeField] private TextMeshProUGUI leftPlayerName;

    [SerializeField] private TextMeshProUGUI rightPlayerName;

    [SerializeField] private TextMeshProUGUI joinedPlayerReady;

    [SerializeField] private GameObject leftPlayer;

    [SerializeField] private GameObject rightPlayer;

    [SerializeField] private GameObject btnReady;

    [SerializeField] private GameObject btnStartGame;

    [SerializeField] private GameObject btnKickPlayer;
    
    [SerializeField] private GameObject hostLabel;

    private TMP_InputField _lobbyCodeInputField;

    
    /// <summary>
    /// Whether the player that joined the lobby (not the host) is ready for the game to start
    /// </summary>
    private bool _joinedPlayerIsReady = false;

    private PlayerCustomizer _leftPlayerController;
    private PlayerCustomizer _rightPlayerController;
    
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
        _leftPlayerController = leftPlayer.GetComponent<PlayerCustomizer>();
        _rightPlayerController = rightPlayer.GetComponent<PlayerCustomizer>();
        ErrorDisplay.Instance.mainCanvas = mainCanvas;
        
        LobbyManager.Instance.JoinedLobbyChanged += UpdateLobby;
        ChangeColor();
        UpdateLobby();
    }

    public void ChangeColor()
    {
        _leftPlayerController.PlayerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        Debug.Log("Changed color to " + _leftPlayerController.PlayerColor);
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
        string playerColor = "#" + ColorUtility.ToHtmlStringRGB(_leftPlayerController.PlayerColor);
        if(LobbyManager.Instance.ThisPlayer!.Data[LobbyManager.PlayerColorProperty].Value != playerColor)
        {
            changes.Add(LobbyManager.PlayerColorProperty, new PlayerDataObject
                (PlayerDataObject.VisibilityOptions.Member, playerColor));
        }
        //If this is the joined player, and the ready status in the cloud
        //is different from the local ready status, add it to the changes
        if(!LobbyManager.Instance.IsHost && LobbyManager.Instance.ThisPlayer.Data[LobbyManager.PlayerIsReadyProperty].Value != 
           _joinedPlayerIsReady.ToString())
        {
            changes.Add(LobbyManager.PlayerIsReadyProperty, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
                _joinedPlayerIsReady.ToString()));
        }
        return changes;
    }

    public void LeaveLobby() => LobbyManager.Instance.LeaveLobby();
    
    public void StartGame()
    {
        Lobby lobby = LobbyManager.Instance.JoinedLobby;
        if (lobby == null) return;
        if (lobby.Players.Count != 2) return;
        if(LobbyManager.Instance.JoinedPlayer!.Data[LobbyManager.PlayerIsReadyProperty].Value != true.ToString()) return;
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
        if (!LobbyManager.Instance.IsHost) return;
        LobbyManager.Instance.KickPlayer(LobbyManager.Instance.JoinedPlayer!.Id);
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
        if (lobby == null || LobbyManager.Instance.ThisPlayer == null) return;
        
        //Updating the UI:
        lobbyName.text = lobby.Name;
        _lobbyCodeInputField.text = lobby.LobbyCode;
        //Everyone sees themselves on the left side
        leftPlayerName.text = LobbyManager.Instance.ThisPlayer.Data[LobbyManager.PlayerNameProperty].Value;
        //The host doesn't have a ready status, they just click on start when they're ready
        btnReady.SetActive(LobbyManager.Instance.IsHost == false);
        //Only the host can start the game
        btnStartGame.SetActive(LobbyManager.Instance.IsHost);
        //Only the host can kick the other player
        btnKickPlayer.SetActive(LobbyManager.Instance.IsHost && lobby.Players.Count > 1);
        
        //Setting the host and ready label to the correct player (side)
        SetHostAndReadyLabels();
        UpdateOtherPlayer();
    }

    private void SetHostAndReadyLabels()
    {
        var hostPosition = hostLabel.transform.position;
        var readyPosition = joinedPlayerReady.transform.position;
        if (LobbyManager.Instance.IsHost)
        {
            hostPosition = new Vector3(-Mathf.Abs(hostPosition.x), hostPosition.y, hostPosition.z);
            readyPosition = new Vector3(Mathf.Abs(readyPosition.x), readyPosition.y, readyPosition.z);
            //If this is the host, it needs to get the ready status of the joined player from the lobby data
            _joinedPlayerIsReady = LobbyManager.Instance.JoinedPlayer?.Data[LobbyManager.PlayerIsReadyProperty].Value ==
                                   true.ToString();
        }
        else
        {
            hostPosition = new Vector3(Mathf.Abs(hostPosition.x), hostPosition.y, hostPosition.z);
            readyPosition = new Vector3(-Mathf.Abs(readyPosition.x), readyPosition.y, readyPosition.z);
        }

        hostLabel.transform.position = hostPosition;
        joinedPlayerReady.transform.position = readyPosition;
        joinedPlayerReady.gameObject.SetActive(_joinedPlayerIsReady);
    }

    private void UpdateOtherPlayer()
    {
        if (LobbyManager.Instance.JoinedLobby!.Players.Count > 1)
        {
            //Updating the second player
            rightPlayer.SetActive(true); //Enabling the player prefab
            _rightPlayerController.PlayerColor = ColorUtility.TryParseHtmlString
                (LobbyManager.Instance.OtherPlayer!.Data[LobbyManager.PlayerColorProperty].Value, out Color color)
                ? color
                : Color.blue;
            rightPlayerName.text = LobbyManager.Instance.OtherPlayer!.Data[LobbyManager.PlayerNameProperty].Value;
        }
        else
        {
            //The second player is hidden
            rightPlayerName.text = string.Empty;
            rightPlayer.SetActive(false);
        }
    }

    #endregion
}
