using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using PlayFab;
using PlayFab.ClientModels;
using Services;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class LoginCanvas : MonoBehaviour
{
    public ButtonBase continueAsGuestButton;
    public TMP_InputField username;
    public TMP_InputField password;

    public void Login() => PlayFabManager.Instance.Login(username.text, password.text);
    
    public void ContinueAsGuest() => PlayFabManager.Instance.ContinueAsGuest();
}
