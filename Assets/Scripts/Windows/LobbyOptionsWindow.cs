using System;
using Managers;
using Models;
using UnityEngine;

namespace Windows
{
    public class LobbyOptionsWindow : AnimatableWindow
    {
        [SerializeField] private AnimatableWindow leftWindow;
        [SerializeField] private AnimatableWindow rightWindow;
        
        public override void Enter(Action onComplete = null)
        {
            gameObject.SetActive(true);
            UIManager.Instance.ChangeMainTitle("Lobby Options");
            leftWindow.Enter();
            rightWindow.Enter(onComplete);
        }
        
        public override void Exit(Action onComplete = null)
        {
            Action callback = () =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            };
            UIManager.Instance.ChangeMainTitle("Celestial Echo");
            leftWindow.Exit();
            rightWindow.Exit(callback);
            
        }
    }
}
