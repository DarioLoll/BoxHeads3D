using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Services;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class EmailCanvas : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI subtitle;
    public TextMeshProUGUI subtitle2;
    public TextMeshProUGUI email;
    public TextMeshProUGUI cooldown;
    public Button resendButton;
    
    public float cooldownDuration = 30.0f;
    
    public void SetEmailCanvas(EmailTypes emailType, string mail)
    {
        switch (emailType)
        {
            case EmailTypes.Verification:
                title.text = "Verify Email";
                subtitle.text = "Account created successfully!";
                subtitle2.text = "A verification link has been sent to";
                break;
            case EmailTypes.PasswordReset:
                title.text = "Password Reset";
                subtitle.text = "Password reset requested!";
                subtitle2.text = "A password reset link has been sent to";
                break;
        }
        email.text = mail;
    }

    public void ResendEmail()
    {
        string mail = email.text;
        var request = new AddOrUpdateContactEmailRequest()
        {
            EmailAddress = mail
        };
        PlayFabClientAPI.AddOrUpdateContactEmail(request, OnResendSuccess, OnResendFailure);
    }

    private void OnResendFailure(PlayFabError obj)
    {
        Debug.Log("Failed to resend email: " + obj.ErrorMessage);
        UIManager.Instance.DisplayError(obj);
    }

    private void OnResendSuccess(AddOrUpdateContactEmailResult obj)
    {
        Debug.Log("Email resent successfully");
        StartTimer();
    }

    private void OnEnable()
    {
        //Resending emails is not yet implemented correctly
        //StartTimer();
    }

    private void StartTimer()
    {
        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        resendButton.interactable = false;
        float time = cooldownDuration;
        while (time > 0)
        {
            cooldown.text = "Cooldown: " + time.ToString("F0") + "s";
            yield return new WaitForSeconds(1.0f);
            time--;
        }
        cooldown.text = "";
        resendButton.interactable = true;
    }
}

public enum EmailTypes
{
    Verification,
    PasswordReset
}
