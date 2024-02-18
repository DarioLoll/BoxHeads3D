using System;
using PlayFab;
using TMPro;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

namespace Services
{
    public class PopupBox : MonoBehaviour
    {
        public static PopupBox Instance;

        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI tbTitle;
        [SerializeField] private GameObject icon;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite infoIcon;
    
        public Color ErrorIconColor { get; set; } = Color.red;
    
        public Color InfoIconColor { get; set; } = Color.yellow;

        private Image _iconImage;
        private Canvas _canvas;

        public string Message
        {
            get => text.text;
            set => text.text = value;
        }

        public string Title
        {
            get => tbTitle.text;
            set => tbTitle.text = value;
        }

        private Action _onClose = null;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Instance = this;
            _iconImage = icon.GetComponent<Image>();
            _canvas = GetComponent<Canvas>();
        }
    
        public void DisplayError(string message, Action onClose = null)
        {
            Display(message, "Error", onClose);
            _iconImage.sprite = errorIcon;
            _iconImage.color = ErrorIconColor;
        }
    
        public void DisplayInfo(string message, Action onClose = null)
        {
            Display(message, "Info", onClose);
            _iconImage.sprite = infoIcon;
            _iconImage.color = InfoIconColor;
        }

        private void Display(string message, string title, Action onClose = null)
        {
            Close();
            Title = title;
            Message = message;
            _onClose = onClose;
            _canvas.enabled = true;
        }
    
        public void Close()
        {
            _canvas.enabled = false;
            _onClose?.Invoke();
            LoadingScreen.Instance.CloseLoadingScreen();
        }

        public void DisplayPlayFabError(PlayFabError error)
        {
            DisplayError(error.ErrorMessage);
        }
    
        public void DisplayLobbyError(LobbyServiceException exception)
        {
            switch (exception.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby:
                    DisplayError("You are already in this lobby.\n Try restarting your game.");
                    break;
                case LobbyExceptionReason.AlreadyUnsubscribedFromLobby:
                    DisplayError("You already left the lobby.\n Try restarting your game.");
                    break;
                case LobbyExceptionReason.LobbyConflict:
                case LobbyExceptionReason.Conflict:
                    DisplayError("An unknown error occured.\n Try restarting your game");
                    break;
                case LobbyExceptionReason.EntityNotFound:
                    DisplayError("The lobby you are trying to join does not exist.");
                    break;
                case LobbyExceptionReason.IncorrectPassword:
                    DisplayError("The password you entered is incorrect.");
                    break;
                case LobbyExceptionReason.ValidationError:
                case LobbyExceptionReason.InvalidJoinCode:
                    DisplayError("The lobby code you entered is invalid.");
                    break;
                case LobbyExceptionReason.LobbyAlreadyExists:
                    DisplayError("A lobby with this name already exists.");
                    break;
                case LobbyExceptionReason.LobbyFull:
                    DisplayError("The lobby you are trying to join is full.");
                    break;
                case LobbyExceptionReason.LobbyNotFound:
                    DisplayError("The lobby you are trying to join does not exist.");
                    break;
                case LobbyExceptionReason.NoOpenLobbies:
                    DisplayError("There are no open lobbies at the moment.\n Try creating one.");
                    break;
                case LobbyExceptionReason.NetworkError:
                    DisplayError("Couldn't connect to the server.\n Check your internet connection and try again.");
                    break;
                case LobbyExceptionReason.RateLimited:
                    break;
                default:
                    DisplayError("An unknown error occured.\n Try restarting your game.");
                    break;
            }
        }
    }
}
