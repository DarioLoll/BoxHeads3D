using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Managers;
using TMPro;
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
        
        public void Slide(GameObject gameObject, float fromX, float fromY, float toX, float toY, Action callback = null, float delay = 0f)
        {
            _animatedObjects.Add(gameObject);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(fromX, fromY);
            Vector3 startPos = rectTransform.anchoredPosition;
            Vector3 endPos = new Vector2(toX, toY);
            LeanTween.value(gameObject,startPos, endPos, _ui.EnteringAnimationDuration).setEase(LeanTweenType.easeOutCubic)
                .setDelay(delay).setOnUpdate((Vector3 pos) =>
                {
                    rectTransform.anchoredPosition = pos;
                }).setOnComplete(() =>
                {
                    _animatedObjects.Remove(gameObject);
                    callback?.Invoke();
                });
        }
        
        public void Slide(GameObject gameObject, float toX, float toY, Action callback = null, float delay = 0f)
        {
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            var anchoredPosition = rectTransform.anchoredPosition;
            Slide(gameObject, anchoredPosition.x, anchoredPosition.y, toX, toY, callback, delay);
        }
        
        public void FadeIn(GameObject gameObject, Action callback = null)
        {
            _animatedObjects.Add(gameObject);
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            LeanTween.alphaCanvas(canvasGroup, 1, _ui.EnteringAnimationDuration)
                .setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
                {
                    _animatedObjects.Remove(gameObject);
                    callback?.Invoke();
                });
        }
        
        public void FadeOut(GameObject gameObject, Action callback = null)
        {
            _animatedObjects.Add(gameObject);
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            LeanTween.alphaCanvas(canvasGroup, 0, _ui.EnteringAnimationDuration)
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
        
        public void Slide(GameObject go, EnteringAnimation enteringAnimation, Action callback = null)
        {
            var rectTransform = go.GetComponent<RectTransform>();
            Vector3 pos = rectTransform.anchoredPosition;
            switch (enteringAnimation)
            {
                case EnteringAnimation.SlideInFromLeft:
                    Slide(go, -UIManager.HorizontalSlideDistance, pos.y, pos.x, pos.y, callback);
                    break;
                case EnteringAnimation.SlideInFromRight:
                    Slide(go, UIManager.HorizontalSlideDistance, pos.y, pos.x, pos.y, callback);
                    break;
                case EnteringAnimation.SlideInFromTop:
                    Slide(go, pos.x, UIManager.VerticalSlideDistance, pos.x, pos.y, callback);
                    break;
                case EnteringAnimation.SlideInFromBottom:
                    Slide(go, pos.x, -UIManager.VerticalSlideDistance, pos.x, pos.y, callback);
                    break;
            }
        }
        public void SlideOut(GameObject go, ExitingAnimation exitingAnimation, Action callback = null)
        {
            Vector3 pos = go.transform.localPosition;
            switch (exitingAnimation)
            {
                case ExitingAnimation.SlideOutToLeft:
                    Slide(go, -UIManager.HorizontalSlideDistance, pos.y, callback);
                    break;
                case ExitingAnimation.SlideOutToRight:
                    Slide(go, UIManager.HorizontalSlideDistance, pos.y, callback);
                    break;
                case ExitingAnimation.SlideOutToTop:
                    Slide(go, pos.x, UIManager.VerticalSlideDistance, callback);
                    break;
                case ExitingAnimation.SlideOutToBottom:
                    Slide(go, pos.x, -UIManager.VerticalSlideDistance, callback);
                    break;
            }
        }
        
        public void Move(GameObject go, Vector3 to, float duration, Action callback = null)
        {
            LeanTween.moveLocal(go, to, duration).setEaseOutQuad().setOnComplete(() =>
            {
                callback?.Invoke();
            });
        }
        
        public void FadeColor(Image image, ColorType from, ColorType to, float duration, Action callback = null)
        {
            FadeColor(image, ThemeManager.GetColor(from, _ui.currentTheme), 
                ThemeManager.GetColor(to, _ui.currentTheme), duration, callback);
        }

        public void FadeTextColor(TextMeshProUGUI text, Color from, Color to, float duration, Action callback = null)
        {
            LeanTween.value(text.gameObject, from, to, duration)
                .setOnUpdateColor(color => text.color = color).setOnComplete(() =>
                {
                    callback?.Invoke();
                });
        }

        public void FadeColor(Image image, Color from, Color to, float duration, Action callback = null)
        {
            LeanTween.value(image.gameObject, from, to, duration)
                .setOnUpdateColor(color => image.color = color).setOnComplete(() =>
                {
                    callback?.Invoke();
                });
        }
        
    }
    
    public enum EnteringAnimation
    {
        SlideInFromLeft,
        SlideInFromRight,
        SlideInFromTop,
        SlideInFromBottom,
        RotateIn,
        FadeIn
    }
    
    public enum ExitingAnimation
    {
        SlideOutToLeft,
        SlideOutToRight,
        SlideOutToTop,
        SlideOutToBottom,
        RotateOut,
        FadeOut
    }
}