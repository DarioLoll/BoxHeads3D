using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ProfileEditorCanvas : MonoBehaviour
{
    public TextMeshProUGUI username;
    public TMP_InputField displayName;
    public GameObject changePassword;
    public GameObject linkAccount;
    public GameObject logIn;
    public GameObject logOut;
    
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
        if(player.DisplayName != displayName.text && PlayFabManager.Instance.IsLoggedIn)
        {
            PlayFabManager.Instance.UpdateDisplayName(displayName.text);
        }
    }

    public void BackToLogin() => UIManager.Instance.BackToLogin();

    public void LogOut() => PlayFabManager.Instance.LogOut();
}
