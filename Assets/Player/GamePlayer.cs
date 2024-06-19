using System;
using System.Collections;
using System.Collections.Generic;
using Collectables;
using Inventories;
using JetBrains.Annotations;
using Managers;
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
    public static int MaxHealth { get; set; } = 100;
    public static int MaxHungerAndThirstLevel { get; set; } = 10;
    public static int MaxHungerAndThirstPoints => MaxHungerAndThirstLevel * Mathf.FloorToInt(HungerAndThirstPointsPerLevel);
    public static float HungerAndThirstPointsPerLevel { get; set; } = 100f;
    public static float HungerPointsGainPerSecond { get; set; } = 0.1f;
    public static float ThirstPointsGainPerSecond { get; set; } = 0.2f;
    public static float HungerPointsGainPerSecondWhileMoving { get; set; } = 0.5f;
    public static float ThirstPointsGainPerSecondWhileMoving { get; set; } = 0.75f;
    public static float HungerPointsGainPerSecondWhileSprinting { get; set; } = 1f;
    public static float ThirstPointsGainPerSecondWhileSprinting { get; set; } = 1.5f;
    public static float HealthLossPerSecond { get; set; } = 0.5f;
    public static float HealthGainPerSecond { get; set; } = 1f;
    public static int NoRegenerationOnLevel { get; set; } = 3;
    public static int NoSprintingOnLevel { get; set; } = 7;

    public static int WaterSipWorthOfPoints { get; set; } = MaxHungerAndThirstPoints / 8;
    
    [SerializeField] private TextMeshProUGUI playerNickname;
    private PlayerController _playerController;
    [SerializeField] private Material playerMaterial;
    [SerializeField] private SkinnedMeshRenderer _playerRenderer;
    
    [SerializeField] private Transform holdingPosition;

    [CanBeNull] public static GamePlayer ThisPlayer { get; private set; }

    public NetworkVariable<FixedString32Bytes> Nickname { get; } = new();
    public NetworkVariable<FixedString32Bytes> ColorInHex { get; } = new();
    public NetworkVariable<FixedString64Bytes> HandItem { get; } = new();
    public NetworkVariable<float> Health { get; } = new(2);

    [SerializeField] private float healthPreview;

    [SerializeField] private float _hungerPoints;
    /// <summary>
    /// Hunger level of the player from 0 to 10
    /// </summary>
    public int HungerLevel => Mathf.FloorToInt(_hungerPoints / HungerAndThirstPointsPerLevel);
    [SerializeField] private float _thirstPoints;
    public int ThirstLevel => Mathf.FloorToInt(_thirstPoints / HungerAndThirstPointsPerLevel);
    
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
    
    private double _oneSecondTimer = 1f;
    
    public override void OnNetworkSpawn()
    {
        Nickname.OnValueChanged += OnNicknameChanged;
        ColorInHex.OnValueChanged += OnColorChanged;
        if(ColorInHex.Value != default)
            OnColorChanged(default ,ColorInHex.Value);
        if(Nickname.Value != default)
            OnNicknameChanged(default, Nickname.Value);
        Inventory = new Inventory(33);
        if (GameManager.Instance != null && IsOwner)
        {
            GameManager.Instance.RegisterThisPlayer(transform);
            ThisPlayer = this;
            _inventoryVm = InventoryVm.Instance;
            _inventoryVm.Inventory = Inventory;
            _inventoryVm.HandSlot.Slot = Inventory.HandSlot;
            _inventoryVm.HandSlot.HandChanged += OnHandChanged;
            _inventoryCanvas = _inventoryVm.transform.parent.GetComponent<Canvas>();
        }
        HandItem.OnValueChanged += OnHandItemChanged;
        Health.OnValueChanged += OnHealthChanged;
        OnHandItemChanged("", HandItem.Value);
    }
    
    public void OnPlayerDeath()
    {
        gameObject.SetActive(false);
        if (IsOwner)
        {
            HudManager.Instance.DisplayGameOver();
        }
    }
    
    [ServerRpc]
    public void RespawnServerRpc()
    {
        Health.Value = MaxHealth;
        transform.position = GameManager.Instance.SpawnPoint!.Value;
        RespawnClientRpc();
    }
    
    [ClientRpc]
    private void RespawnClientRpc()
    {
        _hungerPoints = 0;
        _thirstPoints = 0;
        gameObject.SetActive(true);
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        healthPreview = newValue;
        if (newValue <= 0)
        {
            OnPlayerDeath();
        }
    }

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _playerController.Drink += OnDrink;
        _hungerPoints = 990;
        _thirstPoints = 990;
    }

    private void OnDrink()
    {
        _thirstPoints = Mathf.Clamp(_thirstPoints - WaterSipWorthOfPoints, 0, MaxHungerAndThirstPoints);
        HudManager.Instance.UpdateHud(Health.Value, _hungerPoints, _thirstPoints);
    }

    private void Update()
    {
        if (!IsOwner) return;
        _oneSecondTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E))
        {
            IsInventoryOpen = !IsInventoryOpen;
        }
        if (IsInventoryOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            IsInventoryOpen = false;
        }
        if (_oneSecondTimer <= 0)
        {
            _oneSecondTimer = 1f;
            UpdateHungerAndThirst();
            UpdateHealth();
            HudManager.Instance.UpdateHud(Health.Value, _hungerPoints, _thirstPoints);
        }
    }

    private void UpdateHungerAndThirst()
    {
        bool isSprinting = _playerController.IsSprinting;
        bool isWalking = _playerController.IsWalking;
        _hungerPoints += isSprinting 
            ? HungerPointsGainPerSecondWhileSprinting 
            : isWalking 
                ? HungerPointsGainPerSecondWhileMoving 
                : HungerPointsGainPerSecond;
        _thirstPoints += isSprinting
            ? ThirstPointsGainPerSecondWhileSprinting
            : isWalking
                ? ThirstPointsGainPerSecondWhileMoving
                : ThirstPointsGainPerSecond;
    }

    private void UpdateHealth()
    {
        if (ThirstLevel == MaxHungerAndThirstLevel || HungerLevel == MaxHungerAndThirstLevel)
        {
            SetHealthServerRpc(Health.Value - HealthLossPerSecond);
        }
        else if (ThirstLevel <= NoRegenerationOnLevel 
                 && HungerLevel < NoRegenerationOnLevel 
                 && Health.Value < MaxHealth)
        {
            SetHealthServerRpc(Health.Value + HealthGainPerSecond);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRpc(float health)
    {
        Health.Value = health;
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
        //playerMaterial.color = ColorUtility.TryParseHtmlString(newValue.Value, out var color) ? color : Color.white;
        _playerRenderer.material.color = ColorUtility.TryParseHtmlString(newValue.Value, out var color) ? color : Color.white;
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
