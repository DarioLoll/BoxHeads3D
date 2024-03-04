using System;
using JetBrains.Annotations;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;

namespace Windows
{
    public class LoginWindow : AnimatableWindow
    {
        [SerializeField] private TMP_InputField username;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private LoadingButton loginButtonContainer;
        [SerializeField] private LoadingButton guestButtonContainer;
        

        public void Login()
        {
            if (IsBusy) return;
            PlayFabManager.Instance.LoggedIn += OnLoggedIn;
            PlayFabManager.Instance.RequestFailed += OnRequestFailed;
            bool success = PlayFabManager.Instance.Login(username.text, password.text);
            if (!success)
            {
                PlayFabManager.Instance.LoggedIn -= OnLoggedIn;
                PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
                return;
            }
            OnRequestSent(loginButtonContainer);
        }

        private void OnLoggedIn(PlayFabPlayer player)
        {
            OnRequestProcessed();
            UIManager.Instance.OnLoggedIn(player);
        }
        
        public void ContinueAsGuest()
        {
            if (IsBusy) return;
            PlayFabManager.Instance.LoggedIn += OnLoggedIn;
            PlayFabManager.Instance.RequestFailed += OnRequestFailed;
            bool success = PlayFabManager.Instance.ContinueAsGuest();
            if (!success)
            {
                PlayFabManager.Instance.LoggedIn -= OnLoggedIn;
                PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
                return;
            }
            OnRequestSent(guestButtonContainer);
        }

        protected override void OnRequestProcessed()
        {
            PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
            PlayFabManager.Instance.LoggedIn -= OnLoggedIn;
            base.OnRequestProcessed();
        }
        
        public void SwitchToSignUp()
        {
            if (IsBusy) return;
            UIManager.Instance.SwitchToWindow(Window.SignUp);
        }

        public void ForgotPassword()
        {
            if (IsBusy) return;
            UIManager.Instance.SwitchToWindow(Window.ResetPassword);
        }
    }
}
