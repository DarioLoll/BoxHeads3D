using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GamePlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private TextMeshProUGUI playerNickname;
    private PlayerController _playerController;
    private Material _playerMaterial;

    public NetworkVariable<FixedString32Bytes> Nickname { get; } = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<FixedString32Bytes> ColorInHex { get; } = new NetworkVariable<FixedString32Bytes>();
    
    private void Awake()
    {
        _playerMaterial = playerObject.GetComponentInChildren<MeshRenderer>().material;
        
    }
    
    
    
    public override void OnNetworkSpawn()
    {
        Nickname.OnValueChanged += OnNicknameChanged;
        ColorInHex.OnValueChanged += OnColorChanged;
        if(ColorInHex.Value != default)
            OnColorChanged(default ,ColorInHex.Value);
        if(Nickname.Value != default)
            OnNicknameChanged(default, Nickname.Value);
        LoadingScreen.Instance.CloseLoadingScreen();
    }

    private void OnColorChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        _playerMaterial.color = ColorUtility.TryParseHtmlString(newValue.Value, out var color) ? color : Color.white;
        
    }

    private void OnNicknameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        playerNickname.text = newValue.Value;
        playerNickname.gameObject.SetActive(!IsOwner);
    }


    [ServerRpc(RequireOwnership = false)]
    public void AssignNickAndColorOnNetworkServerRpc(string nick, string colorInHex)
    {
        Nickname.Value = nick;
        ColorInHex.Value =colorInHex;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Nickname.OnValueChanged -= OnNicknameChanged;
        ColorInHex.OnValueChanged -= OnColorChanged;
    }
}
