using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerTest : NetworkBehaviour
{
    public static MultiplayerTest Instance { get; private set; }
    
    [CanBeNull] public event Action<Transform> OnThisPlayerSpawned;

    public Transform ThisPlayer { get; private set; }
    
    [SerializeField] private Transform playerPrefab;
    
    private Vector3? _spawnPoint;
    
    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        TerrainGenerator.Instance.GenerateSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnClientConnectedServerRpc(ulong clientId)
    {
        if (_spawnPoint == null)
        {
            var rayStart = new Vector3(0, 250, 0);

            if (!Physics.Raycast(rayStart, Vector3.down, out var hit, Mathf.Infinity))
                throw new Exception("Failed to find spawn point");
            _spawnPoint = hit.point + Vector3.up * 5;
        }
        var player = Instantiate(playerPrefab, _spawnPoint.Value, Quaternion.identity);
        Debug.Log($"Player with clientId {clientId} spawned");
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
