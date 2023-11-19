using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ErrorPopupVm : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private GameObject icon;
    [SerializeField] private Sprite errorIcon;
    [SerializeField] private Sprite infoIcon;
    
    public Color ErrorIconColor { get; set; } = Color.red;
    
    public Color InfoIconColor { get; set; } = Color.yellow;

    private Image _iconImage;

    public string ErrorMessage
    {
        get => errorText.text;
        set => errorText.text = value;
    }

    public string Title
    {
        get => title.text;
        set => title.text = value;
    }

    public Action OnClose { get; set; } = null;

    private void Awake()
    { 
        _iconImage = icon.GetComponent<Image>();
    }
    
    public void DisplayError(string message)
    {
        Title = "Error";
        ErrorMessage = message;
        _iconImage.sprite = errorIcon;
        _iconImage.color = ErrorIconColor;
    }
    
    public void DisplayInfo(string message)
    {
        Title = "Info";
        ErrorMessage = message;
        _iconImage.sprite = infoIcon;
        _iconImage.color = InfoIconColor;
    }
    
    public void Close()
    {
        Destroy(gameObject);
        OnClose?.Invoke();
    }
}
