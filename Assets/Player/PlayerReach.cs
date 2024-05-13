using System;
using System.Collections;
using System.Collections.Generic;
using Collectables;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerReach : MonoBehaviour
{
    [CanBeNull]
    public ICollectable CollectableInReach { get; private set; } = null;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")) return;
        Debug.Log("Collision no player");
        if(other.transform.parent == null) return;
        if (other.transform.parent.TryGetComponent(out LODGroup collectable))
        {
            Debug.Log("Collectable in reach");
            //CollectableInReach = collectable;
        } 
        else CollectableInReach = null;
    }

    
}
