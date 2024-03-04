using System;
using Models;
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
        [SerializeField] private Image icon;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite infoIcon;
        [SerializeField] private Sprite successIcon;
        [SerializeField] private RectTransform toTransform;
        [SerializeField] private CanvasGroup background;

        private const float YWhenHidden = 750;

        private ImageColorRefresher _iconImageRefresher;
        
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
            _canvas = GetComponent<Canvas>();
            _iconImageRefresher = icon.GetComponent<ImageColorRefresher>();
        }
    
        public void DisplayError(string message, Action onClose = null)
        {
            icon.sprite = errorIcon;
            _iconImageRefresher.Color = ColorType.PrimaryBackgroundRed;
            Display(message, "Error", onClose);
        }
    
        public void DisplayInfo(string message, Action onClose = null)
        {
            icon.sprite = infoIcon;
            _iconImageRefresher.Color = ColorType.PrimaryBackground;
            Display(message, "Info", onClose);
        }
        
        public void DisplaySuccess(string message, Action onClose = null)
        {
            icon.sprite = successIcon;
            _iconImageRefresher.Color = ColorType.PrimaryBackgroundGreen;
            Display(message, "Success", onClose);
        }

        private void Display(string message, string title, Action onClose = null)
        {
            Close(() =>
            {
                Title = title;
                Message = message;
                _onClose = onClose;
                _canvas.enabled = true;
                
                FadeBackground(false);
                Animate(YWhenHidden,0, 0.3f, _onClose);
            });
        }

        public void Close() => Close(null);
    
        public void Close(Action callback)
        {
            Action onClose = () =>
            {
                _canvas.enabled = false;
                _onClose?.Invoke();
                LoadingScreen.Instance.CloseLoadingScreen();
                callback?.Invoke();
            };
            FadeBackground(true);
            Animate(0, YWhenHidden, 0.3f, onClose);
        }
        
        private void Animate(float fromY, float toY, float duration, Action callback = null)
        {
            LeanTween.value(toTransform.gameObject, fromY, toY, duration)
                .setOnUpdate((float value) =>
                {
                    toTransform.anchoredPosition = new Vector2(toTransform.anchoredPosition.x, value);
                }).setOnComplete(() =>
                {
                    callback?.Invoke();
                });
        }
        
        private void FadeBackground(bool fadeOut, Action onComplete = null)
        {
            LeanTween.alphaCanvas(background, fadeOut ? 0 : 0.9f, 0.3f).setOnComplete(() =>
            {
                onComplete?.Invoke();
            });
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
