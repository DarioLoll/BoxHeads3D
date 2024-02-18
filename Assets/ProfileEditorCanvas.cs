using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class ProfileEditorCanvas : MonoBehaviour
{
    public TextMeshProUGUI username;
    public TMP_InputField displayName;
    public Button changePasswordButton;
    public Button linkAccountButton;
    public Button logInButton;
    
    private void OnEnable()
    {
        logInButton.gameObject.SetActive(!PlayFabManager.Instance.IsLoggedIn);
        displayName.gameObject.SetActive(PlayFabManager.Instance.IsLoggedIn);
        if (!PlayFabManager.Instance.IsLoggedIn)
        {
            changePasswordButton.gameObject.SetActive(false);
            linkAccountButton.gameObject.SetActive(false);
            username.text = "Not logged in";
        }
        else
        {
            PlayFabPlayer player = PlayFabManager.Instance.Player;
            username.text = player.Username;
            displayName.text = player.DisplayName;
            changePasswordButton.gameObject.SetActive(!player.IsGuest);
            linkAccountButton.gameObject.SetActive(player.IsGuest);
        }
    }

    public void Save()
    {
        PlayFabPlayer player = PlayFabManager.Instance.Player;
        if(player.DisplayName != displayName.text && PlayFabManager.Instance.IsLoggedIn)
        {
            PlayFabManager.Instance.UpdateDisplayName(displayName.text);
        }
        UIManager.Instance.Exit(UIManager.Instance.CurrentPanel, UIManager.Instance.DefaultExitingAnimation);
    }

    public void BackToLogin() => UIManager.Instance.BackToLogin();

    public void LogOut() => PlayFabManager.Instance.LogOut();
}
