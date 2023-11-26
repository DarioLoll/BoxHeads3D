using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PasswordInput : MonoBehaviour
{
    #region field
    [SerializeField]
    private TMP_InputField password;
    #endregion

    #region methods
    void Start()
    {
        password.onValueChanged.AddListener(OnPasswordValueChanged);
    }

    private void OnPasswordValueChanged(string newPassword)
    {
        password.text = new string('*', newPassword.Length);
    }
    #endregion
}
