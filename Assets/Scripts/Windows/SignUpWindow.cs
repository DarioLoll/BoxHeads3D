using System;
using Managers;
using Models;
using TMPro;
using UnityEngine;

namespace Windows
{
    public class SignUpWindow : AnimatableWindow
    {
        public TMP_InputField email;
        public GameObject switchToLoginButton;

        private void OnEnable()
        {
            switchToLoginButton.SetActive(PlayFabManager.Instance.Player?.IsGuest is null or false);
        }

        public void SignUp() => PlayFabManager.Instance.SignUp(email.text);
    }
}
