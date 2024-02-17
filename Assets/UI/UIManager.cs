using System;
using System.Text.RegularExpressions;
using PlayFab;
using Services;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
    
        public UIAnimator Animator { get; private set; }

        public GameObject CurrentPanel { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public UIPanelElement signUpPanel;
        public UIPanelElement logInPanel;
        public UIPanelElement displayNamePanel;
        public UIPanelElement resetPasswordPanel;
        public UIPanelElement emailVerificationPanel;
        public GameObject mainMenu;
        public EmailCanvas EmailCanvas { get; private set; }
        public GameObject mainMenuTitleSection;
        public GameObject mainMenuButtonsSection;

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
            Instance = this;
            Animator = new UIAnimator(this);
            EmailCanvas = emailVerificationPanel.gameObject.GetComponent<EmailCanvas>(); 
        }

        private void Start()
        {
            Enter(logInPanel.gameObject, EnteringAnimation.SlideInFromLeft);
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
            Exit(CurrentPanel, DefaultExitingAnimation, () =>
            {
                IsLoggedIn = true;
                mainMenu.SetActive(true);
                //enter main menu
                Enter(mainMenuTitleSection, EnteringAnimation.SlideInFromTop);
                Enter(mainMenuButtonsSection, EnteringAnimation.SlideInFromBottom);
            });
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

        public void CloseApplication()
        {
            Application.Quit();
        }

        public bool ValidateUsername(string username)
        {
            username = username.Trim();
            if (username.Length is < 3 or > 20)
            {
                PopupBox.Instance.DisplayError("Username must be between 3 and 20 characters");
                return false;
            }
            if (!Regex.IsMatch(username, "^[a-zA-Z0-9_.-]+$"))
            {
                PopupBox.Instance.DisplayError("Username may only contain numbers, letters, hyphens, underscores, and periods");
                return false;
            }
            if(username.Contains("..") || username.Contains("--") || username.Contains("__"))
            {
                PopupBox.Instance.DisplayError("Username may not contain consecutive periods, hyphens, or underscores");
                return false;
            }
            if(username.StartsWith('.') || username.StartsWith('-') || username.StartsWith('_'))
            {
                PopupBox.Instance.DisplayError("Username may not start with a period, hyphen, or underscore");
                return false;
            }
            if(username.EndsWith('.') || username.EndsWith('-') || username.EndsWith('_'))
            {
                PopupBox.Instance.DisplayError("Username may not end with a period, hyphen, or underscore");
                return false;
            }
            return true;
        }

        public void DisplayError(PlayFabError message)
        {
            PopupBox.Instance.DisplayPlayFabError(message);
        }

        public bool ValidateEmail(string email)
        {
            string emailText = email.Trim();
            if (!Regex.IsMatch(emailText, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {
                PopupBox.Instance.DisplayError("Invalid email address");
                return false;
            }
            return true;
        }

        public bool ValidatePassword(string passwordText)
        {
            passwordText = passwordText.Trim();
            if (passwordText.Length < 8)
            {
                PopupBox.Instance.DisplayError("Password must be at least 8 characters long");
                return false;
            }
            return true;
        }

        public bool ValidateDisplayName(string displayNameText)
        {
            displayNameText = displayNameText.Trim();
            if (displayNameText.Length is < 3 or > 20)
            {
                PopupBox.Instance.DisplayError("Display name must be between 3 and 20 characters");
                return false;
            }
            return true;
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