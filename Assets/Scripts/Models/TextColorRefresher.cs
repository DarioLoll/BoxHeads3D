using System;
using Managers;
using TMPro;
using UnityEngine;

namespace Models
{
    public class TextColorRefresher : MonoBehaviour, IRefreshable
    {
        private TextMeshProUGUI _text;
        [SerializeField] private ColorType colorType;
        private bool _initialized;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            UIManager.Instance.ThemeChanged += _ => Refresh();
            Refresh();
            _initialized = true;
        }
        
        void OnEnable()
        {
            if (!_initialized) return;
            Refresh();
        }

        public void Refresh(float animationDuration = 0f)
        {
            Color newColor = UIManager.Instance.GetColor(colorType);
            if(animationDuration == 0f)
                _text.color = newColor;
            else if(_text.color != newColor)
                UIManager.Instance.Animator.FadeTextColor(_text, _text.color, newColor, animationDuration);
        }
    }
}
