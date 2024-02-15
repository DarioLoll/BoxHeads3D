using System;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
    
        public UIAnimator Animator { get; private set; }

        public UIPanelElement signUpPanel;
        public UIPanelElement logInPanel;
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
        public float EnteringAnimationDuration = 1f;
        public float ButtonOnClickSizeMultiplier = 0.9f;
        
        public const float HorizontalSlideDistance = 2000f;
        public const float VerticalSlideDistance = 1100f;

        public EnteringAnimation LogInSignUpSwitchEnteringAnimation = EnteringAnimation.RotateIn;
        public ExitingAnimation LogInSignUpSwitchExitingAnimation = ExitingAnimation.RotateOut;
    
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
            go.SetActive(true);
            if(Animator.IsBeingAnimated(go))
                return;
            if (enteringAnimation.ToString().StartsWith("SlideIn"))
            {
                Animator.SlideIn(go, enteringAnimation, callback);
            }
            else if(enteringAnimation == EnteringAnimation.RotateIn)
                Animator.Rotate(go, true, callback);
        }
        
        public void Exit(GameObject go, ExitingAnimation exitingAnimation, Action callback = null)
        {
            Vector3 pos = go.transform.localPosition;
            Action callbackAction = () =>
            {
                go.SetActive(false);
                go.transform.localPosition = pos;
                callback?.Invoke();
            };
            if(Animator.IsBeingAnimated(go))
                return;
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
            Switch(signUpPanel.gameObject, LogInSignUpSwitchExitingAnimation, logInPanel.gameObject,
                LogInSignUpSwitchEnteringAnimation);
        }

        public void SwitchToSignUp()
        {
            Switch(logInPanel.gameObject, LogInSignUpSwitchExitingAnimation, signUpPanel.gameObject,
                LogInSignUpSwitchEnteringAnimation);
        }

        public void CloseApplication()
        {
            Application.Quit();
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