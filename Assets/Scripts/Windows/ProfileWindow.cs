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
            PlayFabPlayer player = PlayFabManager.Instance.Player;
            if(player is null) return;
            if(player.DisplayName != displayName.text && PlayFabManager.Instance.IsLoggedIn)
            {
                PlayFabManager.Instance.UpdateDisplayName(displayName.text);
            }
        }

        public void BackToLogin() => UIManager.Instance.BackToLogin();

        public void LogOut() => PlayFabManager.Instance.LogOut();
        
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
