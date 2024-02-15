using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ButtonBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Button _button;
        private TextMeshProUGUI _buttonText;
        public ThemeColors backgroundColor;
        public ThemeColors textColor;
        public ThemeColors backgroundColorOnHover;
        public ThemeColors textColorOnHover;
        public Image icon;
        private bool _hasIcon;
        public float onHoverSizeMultiplier = 1.0f;

        private Color _backgroundColor;
        private Color _textColor;
        private Color _backgroundColorOnHover;
        private Color _textColorOnHover;
        
        private bool _animateBackground => backgroundColor != backgroundColorOnHover;
        private bool _animateText => textColor != textColorOnHover;
        
        private void Start()
        {
            _button = GetComponent<Button>();
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
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
        }

        public static Color DarkenColor(Color color, float multiplier)
        {
            return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UIManager ui = UIManager.Instance;
            LeanTween.scale(gameObject, Vector3.one * ui.ButtonOnClickSizeMultiplier, ui.HoverBaseDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            UIManager ui = UIManager.Instance;
            LeanTween.scale(gameObject, Vector3.one, ui.HoverBaseDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UIManager ui = UIManager.Instance;
            if (_animateBackground)
            {
                LeanTween.value(gameObject, _backgroundColor, _backgroundColorOnHover, ui.HoverBaseDuration)
                    .setOnUpdateColor(color => _button.image.color = color);
            }
            if (_animateText)
            {
                LeanTween.value(gameObject, _textColor, _textColorOnHover, ui.HoverBaseDuration)
                    .setOnUpdateColor(color =>
                    {
                        _buttonText.color = color;
                        if(_hasIcon)
                            icon.color = color;
                    });
            }
            LeanTween.scale(gameObject, Vector3.one * onHoverSizeMultiplier, ui.HoverBaseDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager ui = UIManager.Instance;
            if (_animateBackground)
            {
                LeanTween.value(gameObject, _backgroundColorOnHover, _backgroundColor, ui.HoverBaseDuration)
                    .setOnUpdateColor(color => _button.image.color = color);
            }
            if (_animateText)
            {
                LeanTween.value(gameObject, _textColorOnHover, _textColor, ui.HoverBaseDuration)
                    .setOnUpdateColor(color =>
                    {
                        _buttonText.color = color;
                        if(_hasIcon)
                            icon.color = color;
                    });
            }
            LeanTween.scale(gameObject, Vector3.one, ui.HoverBaseDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }
    }
    
}