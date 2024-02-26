using System;
using Managers;
using Models;
using TMPro;

namespace Windows
{
    public class LoginWindow : AnimatableWindow
    {
        public TMP_InputField username;
        public TMP_InputField password;

        public void Login() => PlayFabManager.Instance.Login(username.text, password.text);
    
        public void ContinueAsGuest() => PlayFabManager.Instance.ContinueAsGuest();
    }
}
