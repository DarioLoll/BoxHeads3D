using Managers;
using Models;
using TMPro;
using UnityEngine;

namespace Windows
{
    public class CreateLobbyWindow : AnimatableWindow
    {
        [SerializeField] private ToggleButton publicLobbyToggle;
        [SerializeField] private TMP_InputField lobbyName;

        public async void CreateLobby()
        {
            if (IsBusy) return;
            if(lobbyName.text.Length < 3)
            {
                UIManager.Instance.DisplayError("Lobby name must be at least 3 characters long");
                return;
            }
            DisplayLoading("Creating lobby");
            await LobbyManager.Instance.CreateLobby(lobbyName.text, !publicLobbyToggle.IsOn);
            CloseLoading();
            IsBusy = false;
        }
    }
}
