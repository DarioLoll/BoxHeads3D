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

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            UIManager.Instance.ThemeChanged += _ => Refresh();
            Refresh();
        }
        
        public void Refresh()
        {
            if(_text == null)
                _text = GetComponent<TextMeshProUGUI>();
            Color newColor = UIManager.Instance.GetColor(colorType);
            if(_text.color != newColor)
                UIManager.Instance.Animator.FadeTextColor(_text, _text.color, newColor, UIManager.Instance.FadeBaseDuration);
        }
    }
}
