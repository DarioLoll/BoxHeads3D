using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIAnimator
    {
        private UIManager _ui;
        private List<GameObject> _animatedObjects = new List<GameObject>();

        public bool IsBeingAnimated(GameObject gameObject)
        {
            return _animatedObjects.Contains(gameObject);
        }
        public UIAnimator(UIManager ui)
        {
            _ui = ui;
        }
        
        public void SlideIn(GameObject gameObject, float fromX, float fromY, float toX, float toY, Action callback = null)
        {
            _animatedObjects.Add(gameObject);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(fromX, fromY);
            LeanTween.move(rectTransform, new Vector2(toX, toY), _ui.EnteringAnimationDuration)
                .setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
                {
                    _animatedObjects.Remove(gameObject);
                    callback?.Invoke();
                });
        }
        
        public void SlideOut(GameObject gameObject, float toX, float toY, Action callback = null)
        {
            _animatedObjects.Add(gameObject);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            LeanTween.move(rectTransform, new Vector2(toX, toY), _ui.EnteringAnimationDuration)
                .setEase(LeanTweenType.easeInCubic).setOnComplete(() =>
                {
                    _animatedObjects.Remove(gameObject);
                    callback?.Invoke();
                });
        }
        
        public void Rotate(GameObject gameObject, bool rotateIn, Action callback = null)
        {
            //how do I return a task that completes when the animation is done?
            float from = rotateIn ? 90 : 0;
            float to = rotateIn ? 0 : 90;
            _animatedObjects.Add(gameObject);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.localRotation = Quaternion.Euler(0, from, 0);
            LeanTween.value(gameObject, from, to, _ui.EnteringAnimationDuration)
                //.setEase(LeanTweenType.easeOutCubic)
                .setOnUpdate((float v) =>
                {
                    rectTransform.localRotation = Quaternion.Euler(0, v, 0);
                }).setOnComplete(() =>
                {
                    _animatedObjects.Remove(gameObject);
                    rectTransform.localRotation = Quaternion.Euler(0, to, 0);
                    callback?.Invoke();
                });
        }
        
        public void SlideIn(GameObject go, EnteringAnimation enteringAnimation, Action callback = null)
        {
            Vector3 pos = go.transform.localPosition;
            switch (enteringAnimation)
            {
                case EnteringAnimation.SlideInFromLeft:
                    SlideIn(go, -UIManager.HorizontalSlideDistance, pos.y, pos.x, pos.y, callback);
                    break;
                case EnteringAnimation.SlideInFromRight:
                    SlideIn(go, UIManager.HorizontalSlideDistance, pos.y, pos.x, pos.y, callback);
                    break;
                case EnteringAnimation.SlideInFromTop:
                    SlideIn(go, pos.x, UIManager.VerticalSlideDistance, pos.x, pos.y, callback);
                    break;
                case EnteringAnimation.SlideInFromBottom:
                    SlideIn(go, pos.x, -UIManager.VerticalSlideDistance, pos.x, pos.y, callback);
                    break;
            }
        }
        public void SlideOut(GameObject go, ExitingAnimation exitingAnimation, Action callback = null)
        {
            Vector3 pos = go.transform.localPosition;
            switch (exitingAnimation)
            {
                case ExitingAnimation.SlideOutToLeft:
                    SlideOut(go, -UIManager.HorizontalSlideDistance, pos.y, callback);
                    break;
                case ExitingAnimation.SlideOutToRight:
                    SlideOut(go, UIManager.HorizontalSlideDistance, pos.y, callback);
                    break;
                case ExitingAnimation.SlideOutToTop:
                    SlideOut(go, pos.x, UIManager.VerticalSlideDistance, callback);
                    break;
                case ExitingAnimation.SlideOutToBottom:
                    SlideOut(go, pos.x, -UIManager.VerticalSlideDistance, callback);
                    break;
            }
        }
        
    }
    
    public enum EnteringAnimation
    {
        SlideInFromLeft,
        SlideInFromRight,
        SlideInFromTop,
        SlideInFromBottom,
        RotateIn
    }
    
    public enum ExitingAnimation
    {
        SlideOutToLeft,
        SlideOutToRight,
        SlideOutToTop,
        SlideOutToBottom,
        RotateOut
    }
}