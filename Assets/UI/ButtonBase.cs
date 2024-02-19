using System;
using Managers;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ButtonBase : MonoBehaviour, IPointerDownHandler, 
        IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IRefreshable
    {
        private Button _button;
        private TextMeshProUGUI _buttonText;
        public ColorType backgroundColor;
        public ColorType textColor;
        public ColorType backgroundColorOnHover;
        public ColorType textColorOnHover;
        public Image icon;
        private bool _hasIcon;
        public float onHoverSizeMultiplier = 1.0f;

        private Color _backgroundColor;
        private Color _textColor;
        private Color _backgroundColorOnHover;
        private Color _textColorOnHover;
        
        private bool AnimateBackground => backgroundColor != backgroundColorOnHover;
        private bool AnimateText => textColor != textColorOnHover;
        
        private void Start()
        {
            _button = gameObject.GetComponent<Button>();
            _buttonText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            UIManager ui = UIManager.Instance;
            _backgroundColor = ui.GetColor(backgroundColor);
            _textColor = ui.GetColor(textColor);
            _backgroundColorOnHover = ui.GetColor(backgroundColorOnHover);
            _textColorOnHover = ui.GetColor(textColorOnHover);
            _button.image.color = _backgroundColor;
            _buttonText.color = _textColor;
            _hasIcon = icon != null;
            if(_hasIcon)
                icon.color = _textColor;
            Refresh();
        }
        
        public void Refresh()
        {
            if(_button == null)
                _button = gameObject.GetComponent<Button>();
            if(_buttonText == null)
                _buttonText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _hasIcon = icon != null;
            UIManager ui = UIManager.Instance;
            _backgroundColor = ui.GetColor(backgroundColor);
            _textColor = ui.GetColor(textColor);
            _backgroundColorOnHover = ui.GetColor(backgroundColorOnHover);
            _textColorOnHover = ui.GetColor(textColorOnHover);
            if(_button.image.color != _backgroundColor)
                UIManager.Instance.Animator.FadeColor(_button.image, _button.image.color, _backgroundColor, ui.FadeBaseDuration);
            if (_buttonText.color != _textColor)
            {
                if(_hasIcon)
                    UIManager.Instance.Animator.FadeColor(icon, icon.color, _textColor, ui.FadeBaseDuration);
                UIManager.Instance.Animator.FadeTextColor(_buttonText, _buttonText.color, _textColor, ui.FadeBaseDuration);
            }
        }

        public void Disable()
        {
            if (_button == null)
                _button = gameObject.GetComponent<Button>();
            _button.interactable = false;
            var disabledBg = ThemeManager.GetColor(ColorType.DisabledElementBackground, UIManager.Instance.currentTheme);
            var disabledText = ThemeManager.GetColor(ColorType.DisabledForeground, UIManager.Instance.currentTheme);
            AnimateBackgroundColor(_backgroundColor, disabledBg, UIManager.Instance.HoverBaseDuration);
            AnimateTextColor(_textColor, disabledText, UIManager.Instance.HoverBaseDuration);
        }

        public void Enable()
        {
            if (_button == null)
                _button = gameObject.GetComponent<Button>();
            _button.interactable = true;
            AnimateBackgroundColor(ThemeManager.GetColor(ColorType.DisabledElementBackground, UIManager.Instance.currentTheme), 
                _backgroundColor, UIManager.Instance.HoverBaseDuration);
            AnimateTextColor(ThemeManager.GetColor(ColorType.DisabledForeground, UIManager.Instance.currentTheme), 
                _textColor, UIManager.Instance.HoverBaseDuration);
        }

        public static Color DarkenColor(Color color, float multiplier)
        {
            return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            UIManager ui = UIManager.Instance;
            LeanTween.scale(gameObject, Vector3.one * ui.ButtonOnClickSizeMultiplier, ui.HoverBaseDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            UIManager ui = UIManager.Instance;
            LeanTween.scale(gameObject, Vector3.one, ui.HoverBaseDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            UIManager ui = UIManager.Instance;
            if (AnimateBackground)
            {
                AnimateBackgroundColor(_backgroundColor, _backgroundColorOnHover, ui.HoverBaseDuration);
            }
            if (AnimateText)
            {
                AnimateTextColor(_textColor, _textColorOnHover, ui.HoverBaseDuration);
            }
            LeanTween.scale(gameObject, Vector3.one * onHoverSizeMultiplier, ui.HoverBaseDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            UIManager ui = UIManager.Instance;
            if (AnimateBackground)
            {
                AnimateBackgroundColor(_backgroundColorOnHover, _backgroundColor, ui.HoverBaseDuration);
            }
            if (AnimateText)
            {
                AnimateTextColor(_textColorOnHover, _textColor, ui.HoverBaseDuration);
            }
            LeanTween.scale(gameObject, Vector3.one, ui.HoverBaseDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }
        
        public void AnimateBackgroundColor(Color from, Color to, float duration)
        {
            LeanTween.value(gameObject, from, to, duration)
                .setOnUpdateColor(color => _button.image.color = color);
        }
        
        public void AnimateTextColor(Color from, Color to, float duration)
        {
            LeanTween.value(gameObject, from, to, duration)
                .setOnUpdateColor(color =>
                {
                    _buttonText.color = color;
                    if(_hasIcon)
                        icon.color = color;
                });
        }
    }
    
}