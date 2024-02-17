using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PlayFab;
using PlayFab.ClientModels;
using Services;
using TMPro;
using UI;
using UnityEngine;

public class SignUpCanvas : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField email;
    public TMP_InputField password;
    public TMP_InputField confirmPassword;
    
    private string _emailAddress;
    
    public void SignUp()
    {
        if (!ValidateInputs()) return;
        Debug.Log("Inputs are valid. Signing up...");
        _emailAddress = email.text.Trim();
        var request = new RegisterPlayFabUserRequest
        {
            Username = username.text.Trim(),
            Email = _emailAddress,
            Password = password.text.Trim()
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnSignUpSuccess, OnSignUpFailure);
    }

    private void OnSignUpFailure(PlayFabError obj)
    {
        UIManager ui = UIManager.Instance;
        Debug.Log("Sign-up failed: " + obj.ErrorMessage);
        ui.DisplayError(obj);
    }

    private void OnSignUpSuccess(RegisterPlayFabUserResult obj)
    {
        Debug.Log("Sign-up successful " + obj.Username);
        var contactEmailRequest = new AddOrUpdateContactEmailRequest
        {
            EmailAddress = _emailAddress,
        };
        PlayFabClientAPI.AddOrUpdateContactEmail(contactEmailRequest, OnContactEmailSuccess, OnSignUpFailure);
    }

    private void OnContactEmailSuccess(AddOrUpdateContactEmailResult obj)
    {
        //Switch to the email verification canvas
        Debug.Log("Contact email added successfully");
        UIManager ui = UIManager.Instance;
        ui.EmailCanvas.SetEmailCanvas(EmailTypes.Verification, _emailAddress);
        ui.Switch(ui.CurrentPanel.gameObject, ui.DefaultExitingAnimation, 
            ui.emailVerificationPanel.gameObject, ui.DefaultEnteringAnimation);
    }

    private bool ValidateInputs()
    {
        UIManager ui = UIManager.Instance;
        return ui.ValidateUsername(username.text) && ui.ValidateEmail(email.text) && ValidatePassword();
    }
    
    private bool ValidatePassword()
    {
        UIManager ui = UIManager.Instance;
        string passwordText = password.text.Trim();
        if (passwordText != confirmPassword.text.Trim())
        {
            PopupBox.Instance.DisplayError("Passwords do not match");
            return false;
        }
        return ui.ValidatePassword(passwordText);
    }
}
