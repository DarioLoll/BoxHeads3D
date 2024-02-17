using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UI;
using UnityEngine;

public class DisplayNameCanvas : MonoBehaviour
{
    public TMP_InputField displayName;
    
    public void SetDisplayName()
    {
        if (!UIManager.Instance.ValidateDisplayName(displayName.text)) return;
        Debug.Log("Inputs are valid. Setting display name...");
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName.text
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnSetDisplayNameSuccess, OnSetDisplayNameFailure);
    }

    private void OnSetDisplayNameFailure(PlayFabError obj)
    {
        Debug.Log("Failed to set display name: " + obj.ErrorMessage);
        UIManager.Instance.DisplayError(obj);
    }

    private void OnSetDisplayNameSuccess(UpdateUserTitleDisplayNameResult obj)
    {
        Debug.Log("Display name set successfully " + obj.DisplayName);
    }
}
