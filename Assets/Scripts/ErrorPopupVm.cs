using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ErrorPopupVm : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorText;

    public string ErrorMessage
    {
        get => errorText.text;
        set => errorText.text = value;
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
