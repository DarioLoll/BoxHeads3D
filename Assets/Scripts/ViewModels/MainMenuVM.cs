using System;
using Managers;
using Services;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ViewModels
{
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
    

    
        #endregion
    

        #region methods

        private void Start()
        {
            LobbyManager.Instance.CreatingLobby += OnCreatingLobby;
            LobbyManager.Instance.JoiningLobby += OnJoiningLobby;
            LobbyManager.Instance.LobbyCreated += OnJoinedLobby;
            LobbyManager.Instance.LobbyJoined += OnJoinedLobby;
        }
        

        private void OnJoinedLobby()
        {
            LoadingScreen.Instance.CloseLoadingScreen();
            SceneLoader.LoadScene(Scenes.Lobby);
        }

        private void OnJoiningLobby()
        {
            LoadingScreen.Instance.DisplayLoadingScreen("Searching for lobby");
        }
        private void OnCreatingLobby()
        {
            LoadingScreen.Instance.DisplayLoadingScreen("Creating a lobby");
        }

        private void OnDestroy()
        {
            LobbyManager.Instance.CreatingLobby -= OnCreatingLobby;
            LobbyManager.Instance.JoiningLobby -= OnJoiningLobby;
            LobbyManager.Instance.LobbyCreated -= OnJoinedLobby;
            LobbyManager.Instance.LobbyJoined -= OnJoinedLobby;
        }

        public void StartOfflineGame()
        {
            NetworkManager.Singleton.StartHost();
            SceneLoader.LoadSceneOnNetwork(Scenes.Game);
        }

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
            string lobbyCode = lobbyCodeField.text.Trim('\u200b');
            if (string.IsNullOrEmpty(lobbyCode))
            {
                PopupBox.Instance.DisplayError("Please enter a lobby code.");
                return;
            }
            await LobbyManager.Instance.JoinLobbyByCode(lobbyCode);
        }

        /// <summary>
        /// <inheritdoc cref="LobbyManager.QuickJoinLobby"/>
        /// </summary>
        public async void QuickJoinLobby()
        {
            await LobbyManager.Instance.QuickJoinLobby();
        }

        /// <summary>
        /// <inheritdoc cref="LobbyManager.CreateLobby"/>
        /// </summary>
        /// <param name="isPrivate">
        /// If the lobby will be private (only join-able with code) or public (join-able with quick join as well)</param>
        public async void CreateLobby(bool isPrivate)
        {
            await LobbyManager.Instance.CreateLobby(lobbyNameField.text.Trim('\u200b'), isPrivate);
        }

        #endregion
    }
}