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
            Action callbackAction = () =>
            {
                go.SetActive(false);
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

        
    }

    public enum PrimaryColor
    {
        Green,
        Tale
    }
    
    
}