using System;
using Inventories;
using Unity.Netcode;
using UnityEngine;

namespace Collectables
{
    public class ItemVm : MonoBehaviour
    {
        [SerializeField] private ItemType type;
        
        public Item Item { get; private set; }

        public bool IsDropped { get; set; } = false;

        private void Start()
        {
            Item = Items.Singleton.Get(type.ToString().ToLower());
        }
    }
}
