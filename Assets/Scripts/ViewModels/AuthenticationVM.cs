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

    [SerializeField] private GameObject mainCanvas;

    /// <summary>
    /// The animated text that is displayed when the game is logging in the user
    /// </summary>
    [SerializeField] private GameObject loggingInText;
    private TextAnimator _loggingInAnimator;

    private const string NotAlphaNumerical = "The name may only contain numbers and letters.";
    private const string NetworkError = "Couldn't log in. Check your internet connection and try again.";
    
    #endregion
    

    #region methods
    
    private void Start()
    {
        _loggingInAnimator = loggingInText.GetComponent<TextAnimator>();
        ErrorDisplay.Instance.mainCanvas = mainCanvas;
    }
    
    public void Login(TextMeshProUGUI playerNameField) => Login(playerNameField.text.Trim('\u200b'));

    private async void Login(string playerName)
    {
        //The name may only contain numbers and letters
        if (!Regex.IsMatch(playerName, "^[a-zA-Z0-9]+$"))
        {
            ErrorDisplay.Instance.DisplayError(NotAlphaNumerical);
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
            ErrorDisplay.Instance.DisplayError(NetworkError);
            Debug.LogException(e);
        }
        finally
        {
            _loggingInAnimator.StopAnimation();
        }
    }
    #endregion
}
