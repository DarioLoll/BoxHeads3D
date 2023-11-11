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
    [SerializeField] private TextMeshProUGUI errorDisplay;
    private const string NotAlphaNumerical = "The name may only contain numbers and letters";
    public TextMeshProUGUI ErrorDisplay => errorDisplay;
    
    // Start is called before the first frame update
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void Login(TMPro.TextMeshProUGUI playerNameField) => Login(playerNameField.text.Trim('\u200b'));

    private async void Login(string playerName)
    {
        ErrorDisplay.text = string.Empty;
        //The name may only contain numbers and letters
        if (!Regex.IsMatch(playerName, "^[a-zA-Z0-9]+$"))
        { ErrorDisplay.text = NotAlphaNumerical; return; }
        
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);

            //Loading the next scene (main menu)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
