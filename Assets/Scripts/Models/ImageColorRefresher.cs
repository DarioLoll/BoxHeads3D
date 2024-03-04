using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Models
{
    public class ImageColorRefresher : RefresherBase
    {
        private Image _image;
        [SerializeField] private ColorType colorType;
        [SerializeField] private float alpha = 1.0f;
        [SerializeField] private bool hasAlpha = false;

        public ColorType Color
        {
            get => colorType;
            set
            {
                colorType = value;
                Refresh();
            }
        }

        protected override void Start()
        {
            _image = GetComponent<Image>();
            base.Start();
        }

        public override void Refresh(float animationDuration = 0f)
        {
            Color newColor = UIManager.Instance.GetColor(colorType);
            if(hasAlpha) newColor.a = alpha;
            if(animationDuration == 0f)
                _image.color = newColor;
            else if(_image.color != newColor)
                UIManager.Instance.Animator.FadeColor(_image, _image.color, newColor, animationDuration);
        }
    }
}
