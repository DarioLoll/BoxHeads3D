using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using UI;
using UnityEngine;

public class AddAccountDataCanvas : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField password;
    public TMP_InputField confirmPassword;
    
    public void AddAccountData()
    {
        PlayFabManager.Instance.AddAccountData(username.text, password.text, confirmPassword.text);
    }
}
