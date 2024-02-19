using System;
using System.Collections.Generic;
using Managers;
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

        public virtual void Enter(Action onComplete = null)
        {
            if (IsBeingAnimated) return;
            gameObject.SetActive(true);
            if (enteringAnimation.ToString().StartsWith("SlideIn"))
            {
                Animator.SlideIn(gameObject, enteringAnimation, onComplete);
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

        public void SetTheme()
        {
            List<IRefreshable> refreshables = new List<IRefreshable>();
            GetComponentsInChildren(refreshables);
            if(TryGetComponent(typeof(IRefreshable), out var refreshable))
                refreshables.Add((IRefreshable) refreshable);
            refreshables.ForEach(element => element.Refresh());
        }

        
        
    }
}