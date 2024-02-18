using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UI;
using UnityEngine;

public class ResetPasswordCanvas : MonoBehaviour
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
