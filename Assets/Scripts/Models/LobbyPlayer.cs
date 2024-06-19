using Managers;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Models
{
    public class LobbyPlayer : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private GameObject kickButton;
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private bool canBeKicked = true;

        private bool _initialized = false;

        public Player Player { get; private set; }

        public Color Color { get; private set; } 
        public bool IsReady { get; private set; }


        public bool IsHost { get; private set; }
        
        
        public void SetPlayer(Player player)
        {
            bool overwriteLocalValues = !_initialized;
            Player = player;
            if (LobbyManager.Instance.JoinedLobby == null) return;
            if(LobbyManager.Instance.ThisPlayer == null) return;
            IsHost = player.Id == LobbyManager.Instance.HostPlayer!.Id;
            if(LobbyManager.Instance.ThisPlayer?.Id != player.Id || overwriteLocalValues)
            {
                SetColor(ColorUtility.TryParseHtmlString
                    (player.Data[LobbyManager.PlayerColorProperty].Value, out Color color)
                    ? color
                    : Color.white);
                SetReady(player.Data[LobbyManager.PlayerIsReadyProperty].Value == true.ToString());
            }
            RefreshKickButton(player);
            UpdateName();
            _initialized = true;
        }

        private void RefreshKickButton(Player player)
        {
            if (!canBeKicked) return;
            kickButton.SetActive(false);
            if(LobbyManager.Instance.IsHost && LobbyManager.Instance.ThisPlayer!.Id != player.Id)
                kickButton.SetActive(true);
        }

        public void SetColor(Color newColor)
        {
            Color = newColor;
            icon.color = newColor;
        }

        private void SetReady(bool ready)
        {
            if (LobbyManager.Instance.JoinedLobby == null) return;
            IsReady = IsHost || ready;
            UpdateName();
        }

        private void UpdateName()
        {
            string suffix = IsHost ? "\nHOST" 
                : IsReady && LobbyManager.Instance.GameStarted ? "\nIN-GAME" 
                : IsReady ? "\nREADY" : "";
            playerName.text = Player.Data[LobbyManager.PlayerNameProperty].Value + suffix;
        }
        
        public void ToggleReady() => SetReady(!IsReady);

        public void KickPlayer()
        {
            if (LobbyManager.Instance.JoinedLobby == null) return;
            LobbyManager.Instance.KickPlayer(Player.Id);
        }
        
    }
}
