using System;
using System.Collections;
using System.Collections.Generic;
using Collectables;
using Inventories;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerTest : NetworkBehaviour
{
    
    [CanBeNull] public event Action<Transform> OnThisPlayerSpawned;

    public Transform ThisPlayer { get; private set; }
    
    [SerializeField] private Transform playerPrefab;

    public Vector3? SpawnPoint { get; private set; }
    
    void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
        return;
        TerrainGenerator.Instance.GenerateSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnClientConnectedServerRpc(ulong clientId)
    {
        return;
        if (SpawnPoint == null)
        {
            var rayStart = new Vector3(0, 250, 0);

            if (!Physics.Raycast(rayStart, Vector3.down, out var hit, Mathf.Infinity))
                throw new Exception("Failed to find spawn point");
            SpawnPoint = hit.point + Vector3.up * 5;
        }
        var player = Instantiate(playerPrefab, SpawnPoint.Value, Quaternion.identity);
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
    
    [ServerRpc(RequireOwnership = false)]
    public void OnObjectDestroyedServerRpc(NetworkObjectReference collectable)
    {
        //get network object from reference
        if (collectable.TryGet(out var networkObject))
        {
            ICollectable collectableComponent = networkObject.GetComponent<ICollectable>();
            var itemToDrop = Items.Singleton.Get(collectableComponent.Stats.DroppedItem);
            var itemDropCount = collectableComponent.ItemDropCount;
            var position = networkObject.transform.position;
            networkObject.Despawn();
            for (int i = 0; i < itemDropCount; i++)
            {
                var layerMask = LayerMask.GetMask("Default");
                var randomOffset = new Vector3(UnityEngine.Random.Range(-2f, 2f), 20, UnityEngine.Random.Range(-2f, 2f));
                if(!Physics.Raycast(position + randomOffset, Vector3.down, out var hit, Mathf.Infinity, layerMask)) continue;
                var item = Instantiate(itemToDrop.Model, hit.point, Quaternion.identity);
                item.layer = LayerMask.NameToLayer("Collectables");
                item.GetComponent<ItemVm>().IsDropped = true;
                item.GetComponent<NetworkObject>().Spawn();
            }
        }
        else throw new Exception("Failed to get network object from reference");
    }
}
