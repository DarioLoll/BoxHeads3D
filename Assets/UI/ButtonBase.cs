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
        public ButtonTypeName buttonTypeName;
        private ButtonType _buttonType;
        private void Start()
        {
            _button = GetComponent<Button>();
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
            ButtonTypes types = new ButtonTypes();
            _buttonType = types.GetButtonType(buttonTypeName);
            _button.image.color = _buttonType.backgroundColor;
            _buttonText.color = _buttonType.textColor;
        
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
            if (_buttonType.AnimateBackground)
            {
                LeanTween.value(gameObject, _buttonType.backgroundColor, _buttonType.backgroundColorOnHover, ui.HoverBaseDuration)
                    .setOnUpdateColor(color => _button.image.color = color);
            }
            if (_buttonType.AnimateText)
            {
                LeanTween.value(gameObject, _buttonType.textColor, _buttonType.textColorOnHover, ui.HoverBaseDuration)
                    .setOnUpdateColor(color => _buttonText.color = color);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager ui = UIManager.Instance;
            if (_buttonType.AnimateBackground)
            {
                LeanTween.value(gameObject, _buttonType.backgroundColorOnHover, _buttonType.backgroundColor, ui.HoverBaseDuration)
                    .setOnUpdateColor(color => _button.image.color = color);
            }
            if (_buttonType.AnimateText)
            {
                LeanTween.value(gameObject, _buttonType.textColorOnHover, _buttonType.textColor, ui.HoverBaseDuration)
                    .setOnUpdateColor(color => _buttonText.color = color);
            }
        }
    }

    public class ButtonTypes
    {
        public ButtonTypes()
        {
            TextButton = new ButtonType
            {
                backgroundColor = UIManager.Instance.transparent,
                backgroundColorOnHover = UIManager.Instance.transparent,
                textColor = UIManager.Instance.PrimaryBackgroundColor,
                textColorOnHover = UIManager.Instance.PrimaryBackgroundHoverColor
            };
            StandardButton = new ButtonType
            {
                backgroundColor = UIManager.Instance.elementBackground,
                backgroundColorOnHover = UIManager.Instance.elementBackgroundOnHover,
                textColor = UIManager.Instance.baseForeground,
                textColorOnHover = UIManager.Instance.baseForeground
            };
            DisabledButton = new ButtonType
            {
                backgroundColor = UIManager.Instance.disabledElementBackground,
                backgroundColorOnHover = UIManager.Instance.disabledElementBackground,
                textColor = UIManager.Instance.disabledForeground,
                textColorOnHover = UIManager.Instance.disabledForeground
            };
            PrimaryButton = new ButtonType
            {
                backgroundColor = UIManager.Instance.PrimaryBackgroundColor,
                backgroundColorOnHover = UIManager.Instance.PrimaryBackgroundHoverColor,
                textColor = UIManager.Instance.darkForeground,
                textColorOnHover = UIManager.Instance.darkForeground
            };
        }
        public ButtonType PrimaryButton;
        public ButtonType TextButton;
        public ButtonType StandardButton;
        public ButtonType DisabledButton;
        public ButtonType GetButtonType(ButtonTypeName buttonTypeName)
        {
            return buttonTypeName switch
            {
                ButtonTypeName.PrimaryButton => PrimaryButton,
                ButtonTypeName.TextButton => TextButton,
                ButtonTypeName.StandardButton => StandardButton,
                ButtonTypeName.DisabledButton => DisabledButton,
                _ => StandardButton
            };
        }
    }

    public enum ButtonTypeName
    {
        PrimaryButton,
        TextButton,
        StandardButton,
        DisabledButton
    }

    [Serializable]
    public struct ButtonType
    {
        public Color backgroundColor;
        public Color backgroundColorOnHover;
        public Color textColor;
        public Color textColorOnHover;
        public bool AnimateBackground => backgroundColor != backgroundColorOnHover;
        public bool AnimateText => textColor != textColorOnHover;
    }
}