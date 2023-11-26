using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AuthenticationVm : MonoBehaviour
{
    #region fields
    [SerializeField]
    private TextMeshProUGUI playerNameField;

    [SerializeField]
    private TextMeshProUGUI playerPasswordField;

    [SerializeField] 
    private TextMeshProUGUI errorDisplay;

    private const string NotAlphaNumerical = "The name may only contain numbers and letters";

    private const string MinimumPasswordLength = "The password must be at least 8 characters long";
    #endregion

    #region property
    public TextMeshProUGUI ErrorDisplay => errorDisplay;
    #endregion

    #region methods
    public void Login()
    {
        Login(playerNameField.text.Trim('\u200b'), playerPasswordField.text.Trim('\u200b'));
    }

    private async void Login(string playerName, string playerPassword)
    {
        ErrorDisplay.text = string.Empty;

        //The name may only contain numbers and letters
        if (!Regex.IsMatch(playerName, "^[a-zA-Z0-9]+$"))
        { 
            ErrorDisplay.text = NotAlphaNumerical; 
            return;
        }

        //Password should be at least 8 characters long
        if (playerPassword.Length < 8)
        {
            ErrorDisplay.text = MinimumPasswordLength;
            return;
        }
        
        try
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(playerName);
            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(playerName, playerPassword);
            Debug.Log(AuthenticationService.Instance.PlayerId + " " + AuthenticationService.Instance.PlayerName);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        catch (Exception e)
        {
            ErrorDisplay.text = e.Message;
            Debug.LogException(e);
        }
    }

    private void SignInWithGoogle()
    {
        
    }
    #endregion
}
