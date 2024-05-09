using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerTest : NetworkBehaviour
{
    public static MultiplayerTest Instance { get; private set; }
    
    public event Action<Transform> OnThisPlayerSpawned;

    public Transform ThisPlayer { get; private set; }
    
    [SerializeField] private Transform playerPrefab;
    
    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer && !IsHost) return;
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        var spawnPoint = new Vector3(0, 20, 0);
        var player = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    public void RegisterThisPlayer(Transform thisPlayer)
    {
        ThisPlayer = thisPlayer;
        OnOnThisPlayerSpawned(thisPlayer);
    }
    
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    protected virtual void OnOnThisPlayerSpawned(Transform thisPlayer)
    {
        OnThisPlayerSpawned?.Invoke(thisPlayer);
    }
}
