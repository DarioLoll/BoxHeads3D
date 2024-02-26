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
            await LobbyManager.Instance.CreateLobby(lobbyName.text, !publicLobbyToggle.IsOn);
        }
    }
}
