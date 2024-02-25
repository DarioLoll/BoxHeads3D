using Managers;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Models
{
    public class LobbyElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI lobbyName;
        [SerializeField] private TextMeshProUGUI lobbyPlayers;

        private Lobby _lobby;
        
        public void SetLobby(Lobby lobby)
        {
            _lobby = lobby;
            SetLobbyName(lobby.Name);
            SetLobbyPlayers(lobby.Players.Count, lobby.MaxPlayers);
        }

        public async void JoinLobby() => await LobbyManager.Instance.JoinLobby(_lobby);

        private void SetLobbyName(string newName) => lobbyName.text = newName;
        private void SetLobbyPlayers(int players, int maxPlayer) => lobbyPlayers.text = $"{players}/{maxPlayer}";
    }
}
