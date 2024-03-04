using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Managers;
using Services;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Models
{
    public class AnimatableWindow : MonoBehaviour
    {
        public EnteringAnimation enteringAnimation;
        public ExitingAnimation exitingAnimation;
        public bool isModal;
        protected UIAnimator Animator => UIManager.Instance.Animator;
        public bool IsBeingAnimated => Animator.IsBeingAnimated(gameObject);

        public bool IsBusy { get; set; }

        private Action _closeLoading;
        [CanBeNull] protected LoadingButton CurrentLoadingButton { get; set; }

        public virtual void Enter(Action onComplete = null)
        {
            if (IsBeingAnimated) return;
            gameObject.SetActive(true);
            if (enteringAnimation.ToString().StartsWith("SlideIn"))
            {
                Animator.Slide(gameObject, enteringAnimation, onComplete);
            }
            else if(enteringAnimation == EnteringAnimation.RotateIn)
                Animator.Rotate(gameObject, true, onComplete);
        }

        public virtual void Exit(Action onComplete = null)
        {
            if(IsBeingAnimated) return;
            var obj = gameObject;
            Vector3 startPos = obj.transform.localPosition;
            Vector3 startRotation = obj.transform.localEulerAngles;
            Action callbackAction = () =>
            {
                obj.SetActive(false);
                obj.transform.localPosition = startPos;
                obj.transform.localEulerAngles = startRotation;
                onComplete?.Invoke();
            };
            
            if (exitingAnimation.ToString().StartsWith("SlideOut"))
            {
                Animator.SlideOut(gameObject, exitingAnimation, callbackAction);
            }
            else if(exitingAnimation == ExitingAnimation.RotateOut)
                Animator.Rotate(gameObject, false, callbackAction);
        }

        public virtual void DisplayLoading(string loadingText = "Loading", Action callback = null)
        {
            LoadingScreen.Instance.DisplayLoadingScreen(loadingText, callback);
            _closeLoading = () => LoadingScreen.Instance.CloseLoadingScreen();
        }

        public virtual void DisplayLoading(LoadingButton nextToButton)
        {
            CurrentLoadingButton = nextToButton;
            nextToButton.StartLoading();
            _closeLoading = () => CloseLoading(nextToButton);
        }

        protected virtual void OnRequestSent(LoadingButton loadingToDisplay = default)
        {
            IsBusy = true;
            if (loadingToDisplay != default)
            {
                DisplayLoading(loadingToDisplay);
            }
            else DisplayLoading();
        }
        
        public virtual void CloseLoading()
        {
            _closeLoading?.Invoke();
            _closeLoading = null;
        }
        
        protected virtual void CloseLoading(LoadingButton nextToButton)
        {
            CurrentLoadingButton = null;
            nextToButton.StopLoading();
        }
        
        protected void CloseCurrentLoading() => CloseLoading(CurrentLoadingButton);

        protected virtual void OnRequestProcessed()
        {
            CloseCurrentLoading();
            IsBusy = false;
        }
        protected virtual void OnRequestFailed(string errorMessage)
        {
            OnRequestProcessed();
            UIManager.Instance.DisplayError(errorMessage);
        }
    }
}