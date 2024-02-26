using System;
using Managers;
using Models;
using TMPro;

namespace Windows
{
    public class AddAccountDataWindow : AnimatableWindow
    {
        public TMP_InputField username;
        public TMP_InputField password;
        public TMP_InputField confirmPassword;
    
        public void AddAccountData()
        {
            PlayFabManager.Instance.AddAccountData(username.text, password.text, confirmPassword.text);
        }
    }
}
