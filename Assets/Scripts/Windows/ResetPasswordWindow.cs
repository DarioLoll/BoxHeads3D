using System;
using Managers;
using Models;
using TMPro;
using UI;

namespace Windows
{
    public class ResetPasswordWindow : AnimatableWindow
    {
        public ButtonBase backToLoginButton;
        public TMP_InputField email;

        private void OnEnable()
        {
            if (UIManager.Instance.IsOnMainMenu)
                backToLoginButton.gameObject.SetActive(false);
            else
                backToLoginButton.gameObject.SetActive(true);
        }

        public void ResetPassword() => PlayFabManager.Instance.ResetPassword(email.text);
    }
}
