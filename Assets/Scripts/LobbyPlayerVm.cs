using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

public class LobbyPlayerVm : MonoBehaviour
{
    [SerializeField] private Transform icon;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private GameObject readyIcon;
    [SerializeField] private GameObject kickButton;

    [SerializeField] private Sprite readyImage;
    [SerializeField] private Sprite hostImage;
    
    private Image _iconImage;
    private Image _readyIconImage;
    
    private readonly Color _checkmarkColor = new Color(0.0f, 0.8f, 0.0f);
    private readonly Color _crownColor = new Color(1.0f, 0.8f, 0.0f);

    private Player _player;

    public bool IsReady
    {
        get => readyIcon.activeInHierarchy || LobbyManager.Instance.IsHost;
        set
        {
            if(LobbyManager.Instance.JoinedLobby!.HostId != _player.Id)
                readyIcon.SetActive(value);
        }
    }

    public Color Color 
    {
        get => _iconImage.color;
        set => _iconImage.color = value;
    }

    private void Awake()
    {
        _iconImage = icon.GetComponent<Image>();
        _readyIconImage = readyIcon.GetComponent<Image>();
    }
    
    public void SetPlayer(Player player)
    {
        _player = player;
        if (LobbyManager.Instance.JoinedLobby == null) return;
        playerName.text = player.Data[LobbyManager.PlayerNameProperty].Value;
        Color = ColorUtility.TryParseHtmlString
            (player.Data[LobbyManager.PlayerColorProperty].Value, out Color color)
            ? color
            : Color.white;
        kickButton.SetActive(false);
        if (player.Id == LobbyManager.Instance.HostPlayer!.Id)
        {
            readyIcon.SetActive(true);
            _readyIconImage.sprite = hostImage;
            _readyIconImage.color = _crownColor;
            return;
        }
        
        IsReady = player.Data[LobbyManager.PlayerIsReadyProperty].Value == true.ToString();
        _readyIconImage.sprite = readyImage;
        _readyIconImage.color = _checkmarkColor;
        if(LobbyManager.Instance.IsHost)
            kickButton.SetActive(true);
    } 

    public void KickPlayer()
    {
        if (LobbyManager.Instance.JoinedLobby == null) return;
        LobbyManager.Instance.KickPlayer(_player.Id);
    }
    
}
