using System;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;

namespace Windows
{
    public class ProfileWindow : AnimatableWindow
    {
        [SerializeField] private TextMeshProUGUI username;
        [SerializeField] private TMP_InputField displayName;
        [SerializeField] private GameObject changePassword;
        [SerializeField] private GameObject linkAccount;
        [SerializeField] private GameObject logIn;
        [SerializeField] private GameObject logOut;
        [SerializeField] private ButtonBase profileButton;
        [SerializeField] private LoadingButton logOutContainer;

        private void OnEnable()
        {
            logIn.SetActive(!PlayFabManager.Instance.IsLoggedIn);
            logOut.SetActive(PlayFabManager.Instance.IsLoggedIn);
            displayName.transform.parent.gameObject.SetActive(PlayFabManager.Instance.IsLoggedIn);
            if (!PlayFabManager.Instance.IsLoggedIn)
            {
                changePassword.gameObject.SetActive(false);
                linkAccount.gameObject.SetActive(false);
                username.text = "Not logged in";
            }
            else
            {
                PlayFabPlayer player = PlayFabManager.Instance.Player;
                username.text = player.Username;
                displayName.text = player.DisplayName;
                changePassword.gameObject.SetActive(!player.IsGuest);
                linkAccount.gameObject.SetActive(player.IsGuest);
            }
        }

        public void Save()
        {
            if (IsBusy) return;
            PlayFabPlayer player = PlayFabManager.Instance.Player;
            if(player is null) return;
            if(player.DisplayName != displayName.text && PlayFabManager.Instance.IsLoggedIn)
            {
                PlayFabManager.Instance.DisplayNameUpdated += OnDisplayNameUpdated;
                PlayFabManager.Instance.RequestFailed += OnRequestFailed;
                bool success = PlayFabManager.Instance.UpdateDisplayName(displayName.text);
                if (!success)
                {
                    PlayFabManager.Instance.DisplayNameUpdated -= OnDisplayNameUpdated;
                    PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
                    return;
                }
                IsBusy = true;
            }
        }

        protected override void OnRequestProcessed()
        {
            PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
            base.OnRequestProcessed();
        }

        private void OnDisplayNameUpdated(PlayFabPlayer player)
        {
            OnRequestProcessed();
            PlayFabManager.Instance.DisplayNameUpdated -= OnDisplayNameUpdated;
            UIManager.Instance.OnDisplayNameUpdated(player);
        }

        public void BackToLogin()
        {
            if (IsBusy) return;
            UIManager.Instance.BackToLogin();
        }

        public void LogOut()
        {
            if (IsBusy) return;
            PlayFabManager.Instance.LoggedOut += OnLoggedOut;
            PlayFabManager.Instance.RequestFailed += OnRequestFailed;
            PlayFabManager.Instance.LogOut();
            OnRequestSent(logOutContainer);
        }

        private void OnLoggedOut()
        {
            OnRequestProcessed();
            PlayFabManager.Instance.LoggedOut -= OnLoggedOut;
            UIManager.Instance.BackToLogin();
        }

        public override void Enter(Action onComplete = null)
        {
            profileButton.Disable();
            UIManager.Instance.ChangeMainTitle("Profile");
            base.Enter(onComplete);
        }

        public override void Exit(Action onComplete = null)
        {
            Save();
            profileButton.Enable();
            UIManager.Instance.ChangeMainTitle("Celestial Echo");
            base.Exit(onComplete);
        }
    }
}
