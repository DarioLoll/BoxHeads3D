using System;
using Managers;
using Models;
using UnityEngine;

namespace Windows
{
    public class MainWindow : AnimatableWindow
    {
        
        public override void Enter(Action onComplete = null)
        {
            base.Enter(onComplete);
            UIManager.Instance.EnterWindow(Window.TitleScreen);
        }

        public override void Exit(Action onComplete = null)
        {
            base.Exit(onComplete);
            UIManager.Instance.ExitWindow(Window.TitleScreen);
        }
    }
}
