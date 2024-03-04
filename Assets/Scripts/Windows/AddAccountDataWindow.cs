using System;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;

namespace Windows
{
    public class AddAccountDataWindow : AnimatableWindow
    {
        [SerializeField] private TMP_InputField username;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private TMP_InputField confirmPassword;
        [SerializeField] private LoadingButton submitButtonContainer;

    
        public void AddAccountData()
        {
            if (IsBusy) return;
            PlayFabManager.Instance.LoggedIn += OnLoggedIn;
            PlayFabManager.Instance.RequestFailed += OnRequestFailed;
            bool requestSent = PlayFabManager.Instance.AddAccountData(username.text, password.text, confirmPassword.text);
            if (!requestSent)
            {
                PlayFabManager.Instance.LoggedIn -= OnLoggedIn;
                PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
                return;
            }
            OnRequestSent(submitButtonContainer);
        }

        protected override void OnRequestProcessed()
        {
            PlayFabManager.Instance.LoggedIn -= OnLoggedIn;
            PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
            base.OnRequestProcessed();
        }

        private void OnLoggedIn(PlayFabPlayer obj)
        {
            OnRequestProcessed();
            UIManager.Instance.OnLoggedIn(obj);
        }
    }
}
