using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Services;
using TMPro;
using UI;
using UnityEngine;

public class LoginCanvas : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField password;

    public void Login()
    {
        if(!ValidateInputs()) return;
        Debug.Log("Inputs are valid. Logging in...");
        var request = new LoginWithPlayFabRequest
        {
            Username = username.text,
            Password = password.text,
            TitleId = PlayFabSettings.TitleId,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
            {
                GetPlayerProfile = true,
                ProfileConstraints = new PlayerProfileViewConstraints()
                {
                    ShowContactEmailAddresses = true,
                    ShowDisplayName = true
                }
            }
        };
        //show loading screen
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
    }
    
    public void ContinueAsGuest()
    {
        //show loading screen
        var request = new LoginWithCustomIDRequest
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
            {
                GetPlayerProfile = true,
                ProfileConstraints = new PlayerProfileViewConstraints()
                {
                    ShowDisplayName = true
                }
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginAsGuestSuccess, OnLoginFailure);
    }

    private void OnLoginAsGuestSuccess(LoginResult obj)
    {
        string displayName = obj.InfoResultPayload.PlayerProfile.DisplayName;
        Debug.Log("Logged in as guest " + displayName);
        //close loading screen
        if(string.IsNullOrEmpty(displayName))
            UIManager.Instance.SwitchToDisplayName();
        else
            UIManager.Instance.SwitchToMainMenu();
    }

    private void OnLoginFailure(PlayFabError obj)
    {
        Debug.Log("Login failed: " + obj.ErrorMessage);
        //close loading screen
        UIManager.Instance.DisplayError(obj);
    }

    private void OnLoginSuccess(LoginResult obj)
    {
        //close loading screen
        var verificationStatus = obj.InfoResultPayload.PlayerProfile.ContactEmailAddresses[0].VerificationStatus;
        if (verificationStatus != EmailVerificationStatus.Confirmed)
        {
            Debug.Log("Email not verified");
            PopupBox.Instance.DisplayError("You must verify your email before logging in");
            return;
        }
        string displayName = obj.InfoResultPayload.PlayerProfile.DisplayName;
        Debug.Log("Login successful");
        if(string.IsNullOrEmpty(displayName))
            UIManager.Instance.SwitchToDisplayName();
        else
            UIManager.Instance.SwitchToMainMenu();
    }

    private bool ValidateInputs()
    {
        return UIManager.Instance.ValidateUsername(username.text) 
               && UIManager.Instance.ValidatePassword(password.text);
    }
}
