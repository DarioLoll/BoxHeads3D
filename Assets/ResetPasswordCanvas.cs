using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UI;
using UnityEngine;

public class ResetPasswordCanvas : MonoBehaviour
{
    public TMP_InputField email;
    private string _emailAddress;
    
    public void ResetPassword()
    {
        if (!UIManager.Instance.ValidateEmail(email.text)) return;
        Debug.Log("Inputs are valid. Sending email...");
        _emailAddress = email.text.Trim();
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = _emailAddress,
            TitleId = PlayFabSettings.TitleId
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnResetPasswordSuccess, OnResetPasswordFailure);
    }

    private void OnResetPasswordFailure(PlayFabError obj)
    {
        Debug.Log("Failed to reset password: " + obj.ErrorMessage);
        UIManager.Instance.DisplayError(obj);
    }

    private void OnResetPasswordSuccess(SendAccountRecoveryEmailResult obj)
    {
        Debug.Log("Password reset email sent successfully");
        UIManager ui = UIManager.Instance;
        ui.EmailCanvas.SetEmailCanvas(EmailTypes.PasswordReset, _emailAddress);
        ui.Switch(ui.CurrentPanel.gameObject, ui.DefaultExitingAnimation, 
            ui.emailVerificationPanel.gameObject, ui.DefaultEnteringAnimation);
    }
}
