using System;
using Managers;
using TMPro;
using UnityEngine;

namespace Models
{
    public class TextColorRefresher : RefresherBase
    {
        private TextMeshProUGUI _text;
        [SerializeField] private ColorType colorType;

        protected override void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();
            base.Start();
        }

        public override void Refresh(float animationDuration = 0f)
        {
            Color newColor = UIManager.Instance.GetColor(colorType);
            if(animationDuration == 0f)
                _text.color = newColor;
            else if(_text.color != newColor)
                UIManager.Instance.Animator.FadeTextColor(_text, _text.color, newColor, animationDuration);
        }
    }
}
