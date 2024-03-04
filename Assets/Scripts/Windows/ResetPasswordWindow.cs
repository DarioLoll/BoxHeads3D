using System;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;

namespace Windows
{
    public class ResetPasswordWindow : AnimatableWindow
    {
        [SerializeField] private ButtonBase backToLoginButton;
        [SerializeField] private TMP_InputField email;
        [SerializeField] private LoadingButton submitButtonContainer;

        private void OnEnable()
        {
            if (UIManager.Instance.IsOnMainMenu)
                backToLoginButton.gameObject.SetActive(false);
            else
                backToLoginButton.gameObject.SetActive(true);
        }

        public void ResetPassword()
        {
            if (IsBusy) return;
            PlayFabManager.Instance.RequestFailed += OnRequestFailed;
            PlayFabManager.Instance.PasswordResetRequested += OnPasswordResetRequested;
            bool success = PlayFabManager.Instance.ResetPassword(email.text);
            if (!success)
            {
                PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
                PlayFabManager.Instance.PasswordResetRequested -= OnPasswordResetRequested;
            }
            else OnRequestSent(submitButtonContainer);
        }

        private void OnPasswordResetRequested(string mail)
        {
            OnRequestProcessed();
            UIManager.Instance.OnPasswordResetRequested(mail);
        }
        
        protected override void OnRequestProcessed()
        {
            PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
            PlayFabManager.Instance.PasswordResetRequested -= OnPasswordResetRequested;
            base.OnRequestProcessed();
        }
        
        public void ToLogin()
        {
            if(IsBusy) return;
            UIManager.Instance.CloseWindow();
        }
    }
}
