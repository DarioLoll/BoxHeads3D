using System;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;

namespace Windows
{
    public class SignUpWindow : AnimatableWindow
    {
        [SerializeField] private TMP_InputField email;
        [SerializeField] private GameObject switchToLoginButton;
        [SerializeField] private LoadingButton signUpButtonContainer;

        private void OnEnable()
        {
            switchToLoginButton.SetActive(PlayFabManager.Instance.Player?.IsGuest is null or false);
        }

        public void SignUp()
        {
            if (IsBusy) return;
            PlayFabManager.Instance.EmailVerificationRequested += OnEmailVerificationRequested;
            PlayFabManager.Instance.RequestFailed += OnRequestFailed;
            bool success = PlayFabManager.Instance.SignUp(email.text);
            if (!success)
            {
                PlayFabManager.Instance.EmailVerificationRequested -= OnEmailVerificationRequested;
                return;
            }
            OnRequestSent(signUpButtonContainer);
        }
        
        protected override void OnRequestProcessed()
        {
            PlayFabManager.Instance.EmailVerificationRequested -= OnEmailVerificationRequested;
            PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
            base.OnRequestProcessed();
        }

        public void ToLogin()
        {
            if(IsBusy) return;
            UIManager.Instance.CloseWindow();
        }

        private void OnEmailVerificationRequested(string mail)
        {
            OnRequestProcessed();
            UIManager.Instance.OnEmailVerificationRequested(mail);
        }
    }
}
