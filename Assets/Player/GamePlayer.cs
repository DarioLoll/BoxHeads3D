using System;
using System.Collections;
using System.Collections.Generic;
using Collectables;
using Inventories;
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
    [SerializeField] private TextMeshProUGUI playerNickname;
    private PlayerController _playerController;
    [SerializeField] private Material playerMaterial;
    
    [SerializeField] private Transform holdingPosition;

    public NetworkVariable<FixedString32Bytes> Nickname { get; } = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<FixedString32Bytes> ColorInHex { get; } = new NetworkVariable<FixedString32Bytes>();
    
    public NetworkVariable<FixedString64Bytes> HandItem { get; } = new NetworkVariable<FixedString64Bytes>();

    public Inventory Inventory { get; private set; }
    
    private InventoryVm _inventoryVm;
    private Canvas _inventoryCanvas;
    
    private bool _isInventoryOpen;
    
    public bool IsInventoryOpen
    {
        get => _isInventoryOpen;
        set
        {
            _isInventoryOpen = value;
            if (value)
            {
                _inventoryCanvas.enabled = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                _inventoryCanvas.enabled = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
    
    public override void OnNetworkSpawn()
    {
        Nickname.OnValueChanged += OnNicknameChanged;
        ColorInHex.OnValueChanged += OnColorChanged;
        if(ColorInHex.Value != default)
            OnColorChanged(default ,ColorInHex.Value);
        if(Nickname.Value != default)
            OnNicknameChanged(default, Nickname.Value);
        Inventory = new Inventory(33);
        if (MultiplayerTest.Instance != null && IsOwner)
        {
            MultiplayerTest.Instance.RegisterThisPlayer(transform);
            _inventoryVm = InventoryVm.Instance;
            _inventoryVm.Inventory = Inventory;
            _inventoryVm.HandSlot.Slot = Inventory.HandSlot;
            _inventoryVm.HandSlot.HandChanged += OnHandChanged;
            _inventoryCanvas = _inventoryVm.transform.parent.GetComponent<Canvas>();
        }
        HandItem.OnValueChanged += OnHandItemChanged;
        OnHandItemChanged("", HandItem.Value);
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                IsInventoryOpen = !IsInventoryOpen;
            }
            if (IsInventoryOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                IsInventoryOpen = false;
            }
        }
    }

    private void OnHandChanged(Slot newHand)
    {
        var itemName = newHand.IsEmpty ? "empty" : newHand.Item!.Value.Name;
        var newHandString = new FixedString64Bytes($"{itemName}|{newHand.Amount}");
        OnHandChangedServerRpc(newHandString);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void OnHandChangedServerRpc(FixedString64Bytes newHandString)
    {
        HandItem.Value = newHandString;
    }

    private void OnHandItemChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        var previousHandObj = holdingPosition.childCount > 0 ? holdingPosition.GetChild(0).gameObject : null;
        if (previousHandObj != null)
        {
            Destroy(previousHandObj);
        }
        var sArray = newValue.Value.Split('|');
        if (newValue == default || sArray[0] == "empty")
        {
            Inventory.HandSlot = new Slot(null, 0);
            return;
        }
        var item = Items.Singleton.Get(sArray[0]);
        Inventory.HandSlot = new Slot(item, int.Parse(sArray[1]));
        Instantiate(item.Model, holdingPosition);
    }

    private void OnColorChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        playerMaterial.color = ColorUtility.TryParseHtmlString(newValue.Value, out var color) ? color : Color.white;
        
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
    

    [ServerRpc (RequireOwnership = false)]
    public void OnItemPickedUpServerRpc(NetworkObjectReference itemReference)
    {
        if (itemReference.TryGet(out var networkObject))
        {
            var itemVm = networkObject.GetComponent<ItemVm>();
            if (itemVm.IsDropped)
            {
                var item = itemVm.Item;
                if (Inventory.AddItem(item))
                {
                    itemVm.IsDropped = false;
                    networkObject.Despawn();
                }
            }
        }
    }
}
