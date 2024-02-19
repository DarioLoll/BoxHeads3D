using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Models
{
    public class ImageColorRefresher : MonoBehaviour, IRefreshable
    {
        private Image _image;
        [SerializeField] private ColorType colorType;
        [SerializeField] private float alpha = 1.0f;
        [SerializeField] private bool hasAlpha = false;

        private void Awake()
        {
            _image = GetComponent<Image>();
            UIManager.Instance.ThemeChanged += _ => Refresh();
            Refresh();
        }
        
        public void Refresh()
        {
            if(_image == null)
                _image = GetComponent<Image>();
            Color newColor = UIManager.Instance.GetColor(colorType);
            if(hasAlpha) newColor.a = alpha;
            if(_image.color != newColor)
                UIManager.Instance.Animator.FadeColor(_image, _image.color, newColor, UIManager.Instance.FadeBaseDuration);
        }
    }
}
