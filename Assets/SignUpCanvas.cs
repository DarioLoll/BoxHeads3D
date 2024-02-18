using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Managers;
using PlayFab;
using PlayFab.ClientModels;
using Services;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class SignUpCanvas : MonoBehaviour
{
    public TMP_InputField email;
    public GameObject switchToLoginButton;

    private void OnEnable()
    {
        switchToLoginButton.SetActive(PlayFabManager.Instance.Player?.IsGuest is null or false);
    }

    public void SignUp() => PlayFabManager.Instance.SignUp(email.text);
}
