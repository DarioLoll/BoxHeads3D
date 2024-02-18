using System;
using System.Text.RegularExpressions;
using Managers;
using Models;
using PlayFab;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
    
        public UIAnimator Animator { get; private set; }

        public GameObject CurrentPanel { get; private set; }
        public UIPanelElement signUpPanel;
        public UIPanelElement addAccountDataPanel;
        public UIPanelElement logInPanel;
        public UIPanelElement displayNamePanel;
        public UIPanelElement resetPasswordPanel;
        public UIPanelElement emailVerificationPanel;
        public UIPanelElement profilePanel;
        public GameObject mainMenu;
        public EmailCanvas EmailCanvas { get; private set; }
        public bool IsOnMainMenu { get; private set; }
        public GameObject mainMenuTitleSection;
        public GameObject mainMenuButtonsSection;
        public GameObject mainMenuObscure;
        
        public GameObject multiplayerButton;
        private ButtonBase _multiplayerButton;

        public Color baseBackground;
        public Color baseForeground;
        public Color highlightedForeground;
        public Color highlightedBackground;
        public Color disabledForeground;
        public Color placeholderForeground;
        public Color elementBackground;
        public Color elementBackgroundOnHover;
        public Color disabledElementBackground;
        public Color darkForeground;
        public Color primaryBackgroundGreen;
        public Color primaryBackgroundHoverGreen;
        public Color primaryBackgroundTale;
        public Color primaryBackgroundHoverTale;
        public Color primaryBackgroundRed;
        public Color primaryBackgroundHoverRed;
        public Color transparent;
    
        public float FadeBaseDuration = 0.5f;
        public float HoverBaseDuration = 0.2f;
        public float EnteringAnimationDuration = 0.5f;
        public float ButtonOnClickSizeMultiplier = 0.9f;
        
        public const float HorizontalSlideDistance = 2000f;
        public const float VerticalSlideDistance = 1100f;

        public EnteringAnimation LogInSignUpSwitchEnteringAnimation = EnteringAnimation.RotateIn;
        public ExitingAnimation LogInSignUpSwitchExitingAnimation = ExitingAnimation.RotateOut;
        
        public EnteringAnimation DefaultEnteringAnimation = EnteringAnimation.SlideInFromTop;
        public ExitingAnimation DefaultExitingAnimation = ExitingAnimation.SlideOutToBottom;
    
        public PrimaryColor primaryColor = PrimaryColor.Green;
    
        public Color PrimaryBackgroundColor => primaryColor switch
        {
            PrimaryColor.Green => primaryBackgroundGreen,
            PrimaryColor.Tale => primaryBackgroundTale,
            _ => primaryBackgroundGreen
        };
        public Color PrimaryBackgroundHoverColor => primaryColor switch
        {
            PrimaryColor.Green => primaryBackgroundHoverGreen,
            PrimaryColor.Tale => primaryBackgroundHoverTale,
            _ => primaryBackgroundHoverGreen
        };

        private void Awake()
        {
            if(Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                Animator = new UIAnimator(this);
                EmailCanvas = emailVerificationPanel.gameObject.GetComponent<EmailCanvas>(); 
            }
        }

        private void Start()
        {
            PlayFabManager manager = PlayFabManager.Instance;
            manager.LoggedIn += OnLoggedIn;
            manager.EmailVerificationRequested += OnEmailVerificationRequested;
            manager.PasswordResetRequested += OnPasswordResetRequested;
            manager.LoggedOut += BackToLogin;
            manager.DisplayNameUpdated += OnDisplayNameUpdated;
            if (manager.IsOffline)
            {
                //Show offline message
                EnterMainMenu();
                _multiplayerButton = multiplayerButton.GetComponent<ButtonBase>();
                _multiplayerButton.Disable();
            }
            else if (manager.IsLoggedIn) 
                EnterMainMenu();
            else
                Enter(logInPanel.gameObject, EnteringAnimation.SlideInFromLeft);
        }

        private void OnDisplayNameUpdated(PlayFabPlayer obj)
        {
            if(!IsOnMainMenu)
                SwitchToMainMenu();
        }

        public void BackToLogin()
        {
            Exit(CurrentPanel.gameObject, DefaultExitingAnimation, () =>
            {
                ExitMainMenu(() =>
                {
                    Enter(logInPanel.gameObject, DefaultEnteringAnimation);
                });
            });
        }

        private void OnPasswordResetRequested(string email) => SwitchToEmailCanvas(EmailTypes.PasswordReset, email);

        private void OnEmailVerificationRequested(string email) => SwitchToEmailCanvas(EmailTypes.Verification, email);

        private void SwitchToEmailCanvas(EmailTypes type, string email)
        {
            EmailCanvas.SetEmailCanvas(type, email);
            Switch(CurrentPanel.gameObject, DefaultExitingAnimation, 
                emailVerificationPanel.gameObject, DefaultEnteringAnimation);
        }

        private void OnLoggedIn(PlayFabPlayer obj)
        {
            if (string.IsNullOrEmpty(PlayFabManager.Instance.Player?.DisplayName) && !IsOnMainMenu)
                SwitchToDisplayName();
            else
                SwitchToMainMenu();
        }

        public Color GetColor(ThemeColors themeColor)
        {
            return themeColor switch
            {
                ThemeColors.BaseBackground => baseBackground,
                ThemeColors.BaseForeground => baseForeground,
                ThemeColors.HighlightedForeground => highlightedForeground,
                ThemeColors.HighlightedBackground => highlightedBackground,
                ThemeColors.DisabledForeground => disabledForeground,
                ThemeColors.PlaceholderForeground => placeholderForeground,
                ThemeColors.ElementBackground => elementBackground,
                ThemeColors.ElementBackgroundOnHover => elementBackgroundOnHover,
                ThemeColors.DisabledElementBackground => disabledElementBackground,
                ThemeColors.DarkForeground => darkForeground,
                ThemeColors.PrimaryBackgroundGreen => primaryBackgroundGreen,
                ThemeColors.PrimaryBackgroundHoverGreen => primaryBackgroundHoverGreen,
                ThemeColors.PrimaryBackgroundTale => primaryBackgroundTale,
                ThemeColors.PrimaryBackgroundHoverTale => primaryBackgroundHoverTale,
                ThemeColors.Transparent => transparent,
                ThemeColors.PrimaryBackground => PrimaryBackgroundColor,
                ThemeColors.PrimaryBackgroundHover => PrimaryBackgroundHoverColor,
                ThemeColors.PrimaryBackgroundRed => primaryBackgroundRed,
                ThemeColors.PrimaryBackgroundHoverRed => primaryBackgroundHoverRed,
                _ => baseBackground
            };
        }

        public void Enter(GameObject go, EnteringAnimation enteringAnimation, Action callback = null)
        {
            if(Animator.IsBeingAnimated(go))
                return;
            if (IsOnMainMenu)
            {
                mainMenuObscure.SetActive(true);
                Animator.FadeIn(mainMenuObscure.gameObject);
            }
            CurrentPanel = go;
            go.SetActive(true);
            
            if (enteringAnimation.ToString().StartsWith("SlideIn"))
            {
                Animator.SlideIn(go, enteringAnimation, callback);
            }
            else if(enteringAnimation == EnteringAnimation.RotateIn)
                Animator.Rotate(go, true, callback);
        }
        
        public void Exit(GameObject go, ExitingAnimation exitingAnimation, Action callback = null)
        {
            if(Animator.IsBeingAnimated(go))
                return;
            CurrentPanel = null;
            if (IsOnMainMenu) 
                Animator.FadeOut(mainMenuObscure.gameObject, () => mainMenuObscure.SetActive(false));
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

        public void Switch(GameObject from, ExitingAnimation exitingAnimation, GameObject to,
            EnteringAnimation enteringAnimation)
        {
            if(Animator.IsBeingAnimated(from) || Animator.IsBeingAnimated(to))
                return;
            Exit(from, exitingAnimation, () => Enter(to, enteringAnimation));
        }
        
        public void EnterProfile() => Enter(profilePanel.gameObject, EnteringAnimation.SlideInFromLeft);

        public void SwitchToLogIn()
        {
            ExitingAnimation exitingAnimation = CurrentPanel == signUpPanel.gameObject
                ? LogInSignUpSwitchExitingAnimation
                : DefaultExitingAnimation;
            EnteringAnimation enteringAnimation = CurrentPanel == signUpPanel.gameObject
                ? LogInSignUpSwitchEnteringAnimation
                : DefaultEnteringAnimation;
            Switch(CurrentPanel, exitingAnimation, logInPanel.gameObject, enteringAnimation);
        }
        
        public void SwitchToMainMenu()
        {
            if (IsOnMainMenu)
            {
                Exit(CurrentPanel, DefaultExitingAnimation);
            }
            else
            {
                Exit(CurrentPanel, DefaultExitingAnimation, EnterMainMenu);
            }
        }

        private void EnterMainMenu()
        {
            mainMenu.SetActive(true);
            Enter(mainMenuTitleSection, EnteringAnimation.SlideInFromTop);
            Enter(mainMenuButtonsSection, EnteringAnimation.SlideInFromBottom, () => IsOnMainMenu = true);
        }
        
        public void ExitMainMenu(Action callback)
        {
            Animator.FadeOut(mainMenuObscure.gameObject, () => mainMenuObscure.SetActive(false));
            Exit(mainMenuTitleSection, ExitingAnimation.SlideOutToTop);
            Exit(mainMenuButtonsSection, ExitingAnimation.SlideOutToBottom, callback);
            IsOnMainMenu = false;
        }

        public void SwitchToSignUp()
        {
            ExitingAnimation exitingAnimation = CurrentPanel == logInPanel.gameObject
                ? LogInSignUpSwitchExitingAnimation
                : DefaultExitingAnimation;
            EnteringAnimation enteringAnimation = CurrentPanel == logInPanel.gameObject
                ? LogInSignUpSwitchEnteringAnimation
                : DefaultEnteringAnimation;
            Switch(CurrentPanel, exitingAnimation, signUpPanel.gameObject, enteringAnimation);
        }
        
        public void SwitchToResetPassword()
        {
            Switch(CurrentPanel, DefaultExitingAnimation, resetPasswordPanel.gameObject,
                DefaultEnteringAnimation);
        }
        
        public void SwitchToDisplayName()
        {
            Switch(CurrentPanel, DefaultExitingAnimation, displayNamePanel.gameObject,
                DefaultEnteringAnimation);
        }

        public void CloseWindow()
        {
            if (IsOnMainMenu)
            {
                if(CurrentPanel != null)
                    Exit(CurrentPanel, DefaultExitingAnimation);
            }
            else if (CurrentPanel != logInPanel.gameObject)
                SwitchToLogIn();
            else
                Application.Quit();
        }
        
        public void DisplayError(PlayFabError message)
        {
            PopupBox.Instance.DisplayPlayFabError(message);
        }


        
    }

    public enum PrimaryColor
    {
        Green,
        Tale
    }

    public enum ThemeColors
    {
        BaseBackground,
        BaseForeground,
        HighlightedForeground,
        HighlightedBackground,
        DisabledForeground,
        PlaceholderForeground,
        ElementBackground,
        ElementBackgroundOnHover,
        DisabledElementBackground,
        DarkForeground,
        PrimaryBackgroundGreen,
        PrimaryBackgroundHoverGreen,
        PrimaryBackgroundTale,
        PrimaryBackgroundHoverTale,
        PrimaryBackground,
        PrimaryBackgroundHover,
        PrimaryBackgroundRed,
        PrimaryBackgroundHoverRed,
        Transparent
    }
    
    
}