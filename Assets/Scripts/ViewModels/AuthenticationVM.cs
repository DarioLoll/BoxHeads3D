using System;
using System.Text.RegularExpressions;
using Services;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace ViewModels
{
    public class AuthenticationVm : MonoBehaviour
    {
        #region fields

        [SerializeField] private GameObject mainCanvas;
        

        private const string NotAlphaNumerical = "The name may only contain numbers and letters.";
        private const string NetworkError = "Couldn't log in. Check your internet connection and try again.";
    
        #endregion
    

        #region methods
    
        public void Login(TextMeshProUGUI playerNameField) => Login(playerNameField.text.Trim('\u200b'));

        private async void Login(string playerName)
        {
            //The name may only contain numbers and letters
            if (!Regex.IsMatch(playerName, "^[a-zA-Z0-9]+$"))
            {
                PopupBox.Instance.DisplayError(NotAlphaNumerical);
                return; 
            }
        
            LoadingScreen.Instance.DisplayLoadingScreen("Logging in");
            try
            {
                InitializationOptions initializationOptions = new InitializationOptions();
                initializationOptions.SetProfile(playerName);
                await UnityServices.InitializeAsync(initializationOptions);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                Debug.Log(AuthenticationService.Instance.PlayerId + " " + AuthenticationService.Instance.PlayerName);
                //Loading the next scene (main menu)
                SceneLoader.LoadScene(Scenes.MainMenu);
                LoadingScreen.Instance.CloseLoadingScreen();
            }
            catch (Exception e)
            {
                PopupBox.Instance.DisplayError(NetworkError, LoadingScreen.Instance.CloseLoadingScreen);
                Debug.LogException(e);
            }
        }
        #endregion
    }
}
