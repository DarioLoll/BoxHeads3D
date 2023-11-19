using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    public static ErrorDisplay Instance;

    public GameObject errorPopupPrefab;
    
    public GameObject mainCanvas;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void DisplayError(string errorMessage)
    {
        var errorPopup = Instantiate(errorPopupPrefab.transform, mainCanvas.transform);
        var errorPopupVm = errorPopup.GetComponent<ErrorPopupVm>();
        errorPopupVm.ErrorMessage = errorMessage;
    }
}
