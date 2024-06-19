using Windows;
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
        private LobbyOptionsWindow _lobbyWindow;
        
        public void SetLobby(Lobby lobby, LobbyOptionsWindow lobbyWindow)
        {
            _lobby = lobby;
            _lobbyWindow = lobbyWindow;
            SetLobbyName(lobby.Name);
            SetLobbyPlayers(lobby.Players.Count, lobby.MaxPlayers);
        }

        public void JoinLobby() => _lobbyWindow.JoinLobby(_lobby);

        private void SetLobbyName(string newName) => lobbyName.text = newName;
        private void SetLobbyPlayers(int players, int maxPlayer) => lobbyPlayers.text = $"{players}/{maxPlayer}";
    }
}
