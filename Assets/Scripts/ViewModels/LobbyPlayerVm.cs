using Managers;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

namespace ViewModels
{
    public class LobbyPlayerVm : MonoBehaviour
    {
        [SerializeField] private Transform icon;
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private GameObject readyIcon;
        [SerializeField] private GameObject kickButton;

        [SerializeField] private Sprite readyImage;
        [SerializeField] private Sprite hostImage;
    
        private Image _iconImage;
        private Image _readyIconImage;
    
        private readonly Color _checkmarkColor = new Color(0.0f, 0.8f, 0.0f);
        private readonly Color _crownColor = new Color(1.0f, 0.8f, 0.0f);

        private Player _player;
        private bool _isReady;

        public bool IsReady
        {
            get => _isReady || _player == null || _player.Id == LobbyManager.Instance.HostPlayer!.Id ;
            private set => _isReady = value;
        }

        public Color Color 
        {
            get => _iconImage.color;
            set => _iconImage.color = value;
        }

        private void Awake()
        {
            _iconImage = icon.GetComponent<Image>();
            _readyIconImage = readyIcon.GetComponent<Image>();
        }
    
        public void SetPlayer(Player player)
        {
            _player = player;
            if (LobbyManager.Instance.JoinedLobby == null) return;
            playerName.text = player.Data[LobbyManager.PlayerNameProperty].Value;
            Color = ColorUtility.TryParseHtmlString
                (player.Data[LobbyManager.PlayerColorProperty].Value, out Color color)
                ? color
                : Color.white;
            kickButton.SetActive(false);
            if (player.Id == LobbyManager.Instance.HostPlayer!.Id)
            {
                IsReady = true;
                readyIcon.SetActive(true);
                _readyIconImage.sprite = hostImage;
                _readyIconImage.color = _crownColor;
                return;
            }
            if(LobbyManager.Instance.IsHost)
                kickButton.SetActive(true);
            _readyIconImage.sprite = readyImage;
            _readyIconImage.color = _checkmarkColor;
            SetReady(player.Data[LobbyManager.PlayerIsReadyProperty].Value == true.ToString());
        }

        public void SetReady(bool isReady)
        {
            IsReady = isReady;
            readyIcon.SetActive(isReady);
        }

        public void KickPlayer()
        {
            if (LobbyManager.Instance.JoinedLobby == null) return;
            LobbyManager.Instance.KickPlayer(_player.Id);
        }
    
    }
}
