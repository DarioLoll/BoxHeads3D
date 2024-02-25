using System;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Windows
{
    public class SettingsWindow : AnimatableWindow
    {
        [SerializeField] private Image _themeButtonIcon;
        [SerializeField] private Sprite _darkModeIcon;
        [SerializeField] private Sprite _lightModeIcon;

        public void SwitchTheme()
        {
            UIManager.Instance.ChangeTheme();
            _themeButtonIcon.sprite = UIManager.Instance.currentTheme == Theme.Light 
                ? _lightModeIcon : _darkModeIcon;
        }

        public void SwitchColorCheme()
        {
            UIManager.Instance.ChangeColorScheme();
        }
        
        public override void Enter(Action onComplete = null)
        {
            UIManager.Instance.ChangeMainTitle("Settings");
            UIManager.Instance.ToggleProfileButton();
            base.Enter(onComplete);
        }

        public override void Exit(Action onComplete = null)
        {
            UIManager.Instance.ChangeMainTitle("Celestial Echo");
            UIManager.Instance.ToggleProfileButton();
            base.Exit(onComplete);
        }
    }
}
