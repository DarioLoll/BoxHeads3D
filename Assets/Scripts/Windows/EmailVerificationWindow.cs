using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Models;
using PlayFab;
using PlayFab.ClientModels;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Windows
{
    public class EmailVerificationWindow : AnimatableWindow
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI subtitle;
        public TextMeshProUGUI subtitle2;
        public TextMeshProUGUI instruction;
        public TextMeshProUGUI email;
        public TextMeshProUGUI cooldown;
        public Button resendButton;

        private EmailTypes _emailType;
        private string _email;
        
        public float cooldownDuration = 30.0f;
        
        public void SetEmailCanvas(EmailTypes emailType, string mail)
        {
            _emailType = emailType;
            _email = mail;
            switch (_emailType)
            {
                case EmailTypes.Verification:
                    title.text = "Verify Email";
                    subtitle.text = "Account created successfully!";
                    subtitle2.text = "A verification link has been sent to";
                    instruction.gameObject.SetActive(true);
                    break;
                case EmailTypes.PasswordReset:
                    title.text = "Password Reset";
                    subtitle.text = "Password reset requested!";
                    subtitle2.text = "A password reset link has been sent to";
                    instruction.gameObject.SetActive(false);
                    break;
            }
            email.text = mail;
        }

        public void Continue()
        {
            UIManager ui = UIManager.Instance;
             if(_emailType == EmailTypes.Verification)
             {
                 PlayFabManager.Instance.CheckVerificationStatus(_email, result =>
                 {
                    if (result)
                        ui.SwitchToWindow(Window.AddAccountData);
                    else
                        PopupBox.Instance.DisplayError("Email not verified yet");
                 });
             }
             else
             {
                 ui.CloseWindow();
             }
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
}
