using Unity.Netcode;
using UnityEngine;

namespace Collectables
{
    public abstract class CollectableBase : NetworkBehaviour, ICollectable
    {
        [SerializeField] private int durability = 50;
    
        [SerializeField] private int itemDropCount = 3;
        
        public abstract CollectableType Type { get; set; }

        public CollectableStats Stats => CollectableTypes.Stats[Type];

        public int Durability
        {
            get => durability;
            set => durability = value;
        }

        public int ItemDropCount
        {
            get => itemDropCount;
            set => itemDropCount = value;
        }
        
        
        public virtual void OnHit(string itemName)
        {
            int damage;
            if(itemName != Stats.FittingItem) 
                damage = 1;
            else
                damage = Stats.BaseDamagePerHit;
            Durability -= damage;
            Debug.Log($"Type {Type} took {damage} damage. Durability: {Durability}");
            if (Durability <= 0)
            {
                OnCollect();
            }
        }

        public virtual void OnCollect()
        {
            Debug.Log($"Type {Type} collected");
            MultiplayerTest.Instance.OnObjectDestroyedServerRpc(GetComponent<NetworkObject>());
        }
    }
}
