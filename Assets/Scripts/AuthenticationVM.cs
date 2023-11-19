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
    
    /// <summary>
    /// The text field displaying errors
    /// </summary>
    [SerializeField] private TextMeshProUGUI errorDisplay;

    /// <summary>
    /// The animated text that is displayed when the game is logging in the user
    /// </summary>
    [SerializeField] private GameObject loggingInText;
    private TextAnimator _loggingInAnimator;

    private const string NotAlphaNumerical = "The name may only contain numbers and letters";
    
    #endregion

    #region property
    public TextMeshProUGUI ErrorDisplay => errorDisplay;
    
    #endregion

    #region methods
    
    private void Start()
    {
        _loggingInAnimator = loggingInText.GetComponent<TextAnimator>();
    }
    
    public void Login(TextMeshProUGUI playerNameField) => Login(playerNameField.text.Trim('\u200b'));

    private async void Login(string playerName)
    {
        ErrorDisplay.text = string.Empty;
        //The name may only contain numbers and letters
        if (!Regex.IsMatch(playerName, "^[a-zA-Z0-9]+$"))
        { 
            ErrorDisplay.text = NotAlphaNumerical; 
            return; 
        }
        
        _loggingInAnimator.StartAnimation();
        try
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(playerName);
            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
            Debug.Log(AuthenticationService.Instance.PlayerId + " " + AuthenticationService.Instance.PlayerName);
            //Loading the next scene (main menu)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        catch (Exception e)
        {
            ErrorDisplay.text = e.Message;
            Debug.LogException(e);
        }
        finally
        {
            _loggingInAnimator.StopAnimation();
        }
    }
    #endregion
}
