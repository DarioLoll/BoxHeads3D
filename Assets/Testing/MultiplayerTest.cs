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
    
    void Awake()
    {
        Instance = this;
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
