using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuVm : MonoBehaviour
{
    #region fields
    /// <summary>
    /// The canvas containing the online menu (lobby creation, joining, etc.)
    /// </summary>
    [SerializeField] 
    private GameObject canvasOnline;

    /// <summary>
    /// The canvas containing the lobby creation menu (lobby name, private/public, etc.)
    /// </summary>
    [SerializeField] 
    private GameObject canvasLobby;
    
    /// <summary>
    /// The input field in the lobby creation menu storing the name for the lobby to be created
    /// </summary>
    [SerializeField] 
    private TextMeshProUGUI lobbyNameField;
    
    /// <summary>
    /// The input field in the lobby joining menu storing the lobby code to join
    /// </summary>
    [SerializeField] 
    private TextMeshProUGUI lobbyCodeField;
    
    /// <summary>
    /// The animated text that is displayed when the game is searching for a lobby
    /// </summary>
    [SerializeField] private GameObject searchingForLobbyText;
    private TextAnimator _searchingForLobbyAnimator;

    /// <summary>
    /// The animated text that is displayed when the game is creating a lobby
    /// </summary>
    [SerializeField] private GameObject creatingLobbyText;
    private TextAnimator _creatingLobbyAnimator;
    
    #endregion
    

    #region methods
    

    private void Start()
    {
        _searchingForLobbyAnimator = searchingForLobbyText.GetComponent<TextAnimator>();
        _creatingLobbyAnimator = creatingLobbyText.GetComponent<TextAnimator>();
    }

    public void StartOfflineGame() => SceneManager.LoadScene("Battlefield");
    public void ShowOnlineMenu() => canvasOnline.SetActive(true);
    public void HideOnlineMenu() => canvasOnline.SetActive(false);
    
    
    public void ShowLobbyCreationMenu()
    {
        canvasOnline.SetActive(false);
        canvasLobby.SetActive(true);
    }
    
    public void HideLobbyCreationMenu()
    {
        canvasOnline.SetActive(true);
        canvasLobby.SetActive(false);
    }
    
    /// <summary>
    /// Closes the application
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// <inheritdoc cref="LobbyManager.JoinLobbyByCode"/>
    /// </summary>
    public async void JoinLobby()
    {
        _searchingForLobbyAnimator.StartAnimation();
        ErrorDisplay.Instance.mainCanvas = canvasOnline;
        await LobbyManager.Instance.JoinLobbyByCode(lobbyCodeField.text.Trim('\u200b'));
        _searchingForLobbyAnimator.StopAnimation();
    }

    /// <summary>
    /// <inheritdoc cref="LobbyManager.QuickJoinLobby"/>
    /// </summary>
    public async void QuickJoinLobby()
    {
        _searchingForLobbyAnimator.StartAnimation();
        ErrorDisplay.Instance.mainCanvas = canvasOnline;
        await LobbyManager.Instance.QuickJoinLobby();
        _searchingForLobbyAnimator.StopAnimation();
    }

    /// <summary>
    /// <inheritdoc cref="LobbyManager.CreateLobby"/>
    /// </summary>
    /// <param name="isPrivate">
    /// If the lobby will be private (only join-able with code) or public (join-able with quick join as well)</param>
    public async void CreateLobby(bool isPrivate)
    {
        _creatingLobbyAnimator.StartAnimation();
        ErrorDisplay.Instance.mainCanvas = canvasLobby;
        await LobbyManager.Instance.CreateLobby(lobbyNameField.text.Trim('\u200b'), isPrivate);
        _creatingLobbyAnimator.StopAnimation();
    }

    #endregion
}