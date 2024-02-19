using System;
using System.Collections.Generic;
using Windows;
using Models;
using PlayFab;
using Services;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
    
        public UIAnimator Animator { get; private set; }
        
        public Theme currentTheme;
        public event Action<Theme> ThemeChanged;

        #region Windows

            public Dictionary<Window, AnimatableWindow> Windows { get; private set; }
            public Window CurrentWindow { get; private set; }
            public bool IsOnMainMenu { get; private set; }
            public Window? CurrentModalWindow;
            [SerializeField] private AnimatableWindow logIn;
            [SerializeField] private AnimatableWindow signUp;
            [SerializeField] private AnimatableWindow displayName;
            [SerializeField] private AnimatableWindow resetPassword;
            [SerializeField] private AnimatableWindow emailVerification;
            [SerializeField] private AnimatableWindow addAccountData;
            [SerializeField] private AnimatableWindow profile;
            [SerializeField] private AnimatableWindow mainMenu;
            [SerializeField] private AnimatableWindow titleScreen;
            [SerializeField] private AnimatableWindow settings;
            

        #endregion
        
        public EmailVerificationWindow EmailVerificationWindow { get; private set; }
        public Image backgroundImage;
        public GameObject mainMenuObscure;
        public GameObject mainMenuTitle;
        private TextMeshProUGUI _mainMenuTitle;
        
        public GameObject multiplayerButton;
        private ButtonBase _multiplayerButton;
    
        public float FadeBaseDuration = 0.5f;
        public float HoverBaseDuration = 0.2f;
        public float EnteringAnimationDuration = 0.5f;
        public float ButtonOnClickSizeMultiplier = 0.9f;
        
        public const float HorizontalSlideDistance = 2000f;
        public const float VerticalSlideDistance = 1100f;
    
        public PrimaryColor primaryColor = PrimaryColor.Green;
    
        public Color PrimaryBackgroundColor => primaryColor switch
        {
            PrimaryColor.Green => ThemeManager.GetColor(ColorType.PrimaryBackgroundGreen, currentTheme),
            PrimaryColor.Tale => ThemeManager.GetColor(ColorType.PrimaryBackgroundTale, currentTheme),
            _ => ThemeManager.GetColor(ColorType.PrimaryBackgroundGreen, currentTheme)
        };
        public Color PrimaryBackgroundHoverColor => primaryColor switch
        {
            PrimaryColor.Green => ThemeManager.GetColor(ColorType.PrimaryBackgroundHoverGreen, currentTheme),
            PrimaryColor.Tale => ThemeManager.GetColor(ColorType.PrimaryBackgroundHoverTale, currentTheme),
            _ => ThemeManager.GetColor(ColorType.PrimaryBackgroundHoverGreen, currentTheme)
        };

        private void Awake()
        {
            if(Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                Animator = new UIAnimator(this);
                EmailVerificationWindow = emailVerification.gameObject.GetComponent<EmailVerificationWindow>(); 
                _mainMenuTitle = mainMenuTitle.GetComponent<TextMeshProUGUI>();
            }
        }

        private void Start()
        {
            InitializeWindowDictionary();
            PlayFabManager manager = PlayFabManager.Instance;
            manager.LoggedIn += OnLoggedIn;
            manager.EmailVerificationRequested += OnEmailVerificationRequested;
            manager.PasswordResetRequested += OnPasswordResetRequested;
            manager.LoggedOut += BackToLogin;
            manager.DisplayNameUpdated += OnDisplayNameUpdated;
            
            if (manager.IsOffline)
            {
                //Show offline message
                EnterWindow(Window.MainMenu);
                _multiplayerButton = multiplayerButton.GetComponent<ButtonBase>();
                _multiplayerButton.Disable();
            }
            else if (manager.IsLoggedIn) 
                EnterWindow(Window.MainMenu);
            else
                EnterWindow(Window.LogIn);
        }
        
        [ContextMenu("Change Theme")]
        public void ChangeTheme()
        {
            currentTheme = currentTheme == Theme.Dark ? Theme.Light : Theme.Dark;
            OnThemeChanged(currentTheme);
            RefreshTheme();
        }
        
        public void ChangeColorScheme()
        {
            primaryColor = primaryColor == PrimaryColor.Green ? PrimaryColor.Tale : PrimaryColor.Green;
            RefreshTheme();
        }

        private void RefreshTheme()
        {
            foreach (var window in Windows.Values)
            {
                window.SetTheme();
            }
        }

        private void InitializeWindowDictionary()
        {
            Windows = new Dictionary<Window, AnimatableWindow>
            {
                {Window.LogIn, logIn},
                {Window.SignUp, signUp},
                {Window.DisplayName, displayName},
                {Window.ResetPassword, resetPassword},
                {Window.EmailVerification, emailVerification},
                {Window.AddAccountData, addAccountData},
                {Window.Profile, profile},
                {Window.MainMenu, mainMenu},
                {Window.TitleScreen, titleScreen},
                {Window.Settings, settings}
            };
        }

        public void SwitchToWindow(int windowValue)
        {
            Window window = (Window) windowValue;
            SwitchToWindow(window);
        }
        
        public void SwitchToWindow(Window window)
        {
            AnimatableWindow animatableWindow = Windows[window];
            if (animatableWindow.isModal && IsOnMainMenu)
            {
                ExitCurrentModalWindow(() =>
                {
                    EnterWindow(window);
                });
            }
            else
            {
                if (CurrentWindow != window) 
                    ExitWindow(CurrentWindow, () => EnterWindow(window));
            }
        }
        
        public void EnterWindow(Window window, Action onComplete = null)
        {
            if (window == Window.MainMenu)
                IsOnMainMenu = true;
            AnimatableWindow animatableWindow = Windows[window];
            if (animatableWindow.isModal && IsOnMainMenu)
            {
                CurrentModalWindow = window;
                OnModalWindowEnter();
            }
            else
                CurrentWindow = window;
            animatableWindow.Enter(onComplete);
        }

        public void ExitWindow(Window window, Action onComplete = null)
        {
            if(window == Window.MainMenu)
                IsOnMainMenu = false;
            var animatableWindow = Windows[window];
            if (animatableWindow.isModal && IsOnMainMenu)
            {
                CurrentModalWindow = null;
                OnModalWindowExit();
            }
            animatableWindow.Exit(onComplete);
        }
        
        public void ExitCurrentModalWindow(Action onComplete = null)
        {
            if (CurrentModalWindow.HasValue) 
                ExitWindow(CurrentModalWindow.Value, onComplete);
            else 
                onComplete?.Invoke();
        }

        private void OnModalWindowEnter()
        {
            mainMenuObscure.SetActive(true);
            Animator.FadeIn(mainMenuObscure.gameObject);
        }
        
        private void OnModalWindowExit()
        {
            Animator.FadeOut(mainMenuObscure.gameObject, () => mainMenuObscure.SetActive(false));
        }

        private void OnDisplayNameUpdated(PlayFabPlayer obj)
        {
            if(!IsOnMainMenu)
                SwitchToWindow(Window.MainMenu);
        }

        public void BackToLogin()
        {
            ExitCurrentModalWindow(() =>
            {
                ExitWindow(Window.MainMenu);
                SwitchToWindow(Window.LogIn);
            });
        }

        private void OnPasswordResetRequested(string email)
        {
            SwitchToEmailVerificationWindow(EmailTypes.PasswordReset, email);
        }

        private void OnEmailVerificationRequested(string email)
        {
            SwitchToEmailVerificationWindow(EmailTypes.Verification, email);
        }

        private void SwitchToEmailVerificationWindow(EmailTypes type, string email)
        {
            EmailVerificationWindow.SetEmailCanvas(type, email);
            SwitchToWindow(Window.EmailVerification);
        }

        private void OnLoggedIn(PlayFabPlayer obj)
        {
            if (IsOnMainMenu)
                ExitCurrentModalWindow(() =>
                {
                    Windows[Window.Profile].gameObject.SetActive(false);
                    Windows[Window.Profile].gameObject.SetActive(true);
                });
            else
            {
                if (string.IsNullOrEmpty(PlayFabManager.Instance.Player?.DisplayName))
                    SwitchToWindow(Window.DisplayName);
                else
                    SwitchToWindow(Window.MainMenu);
            }
        }

        public void ChangeMainTitle(string newTitle)
        {
            Exit(mainMenuTitle, ExitingAnimation.SlideOutToTop, () =>
            {
                _mainMenuTitle.text = newTitle;
                Enter(mainMenuTitle, EnteringAnimation.SlideInFromTop);
            });
        }

        public Color GetColor(ColorType colorType)
        {
            return colorType switch
            {
                ColorType.PrimaryBackground => PrimaryBackgroundColor,
                ColorType.PrimaryBackgroundHover => PrimaryBackgroundHoverColor,
                _ => ThemeManager.GetColor(colorType, currentTheme)
            };
        }

        #region Entering Animations

            public void Enter(GameObject go, EnteringAnimation enteringAnimation, Action callback = null)
            {
                if(Animator.IsBeingAnimated(go))
                    return;
                go.SetActive(true);
                
                if (enteringAnimation.ToString().StartsWith("SlideIn"))
                {
                    Animator.SlideIn(go, enteringAnimation, callback);
                }
                else if(enteringAnimation == EnteringAnimation.RotateIn)
                    Animator.Rotate(go, true, callback);
            }
        


        #endregion

        #region Exiting Animations
            
            public void Exit(GameObject go, ExitingAnimation exitingAnimation, Action callback = null)
            {
                if(Animator.IsBeingAnimated(go))
                    return;
                Vector3 pos = go.transform.localPosition;
                Vector3 rot = go.transform.localEulerAngles;
                Action callbackAction = () =>
                {
                    go.SetActive(false);
                    go.transform.localPosition = pos;
                    go.transform.localEulerAngles = rot;
                    callback?.Invoke();
                };
                
                if (exitingAnimation.ToString().StartsWith("SlideOut"))
                {
                    Animator.SlideOut(go, exitingAnimation, callbackAction);
                }
                else if(exitingAnimation == ExitingAnimation.RotateOut)
                    Animator.Rotate(go, false, callbackAction);
            }

        #endregion
        
        

        public void CloseWindow()
        {
            if (IsOnMainMenu)
            {
                if(CurrentModalWindow.HasValue)
                    ExitCurrentModalWindow();
                else if(CurrentWindow != Window.TitleScreen)
                    SwitchToWindow(Window.TitleScreen);
                else
                    ExitWindow(Window.MainMenu,() => Application.Quit());
            }
            else
            {
                if(CurrentWindow != Window.LogIn)
                    SwitchToWindow(Window.LogIn);
                else
                    Application.Quit();
            }
        }
        
        public void DisplayError(PlayFabError message)
        {
            PopupBox.Instance.DisplayPlayFabError(message);
        }


        protected virtual void OnThemeChanged(Theme obj)
        {
            ThemeChanged?.Invoke(obj);
        }
        
    }

    public enum PrimaryColor
    {
        Green,
        Tale
    }

    public enum Window
    {
        LogIn,
        SignUp,
        DisplayName,
        ResetPassword,
        EmailVerification,
        AddAccountData,
        Profile,
        MainMenu,
        TitleScreen,
        Settings
    }
    
    
}