using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventories
{
    public class Items : MonoBehaviour
    {
        public static Items Singleton { get; private set; }
        
        public const string WoodName = "wood";
        public const string StoneName = "stone";
        public const string AxeName = "axe";
        public const string PickaxeName = "pickaxe";
    
        private const string IconSuffix = "-icon";
        private const string ItemSuffix = "-item";
    
        private Dictionary<string, Item> _items;
    
        public Item Get(string itemName)
        {
            return Singleton._items[itemName];
        }
    
        public Item Wood => Get(WoodName);
    
        public Item Stone => Get(StoneName);
    
        public Item Axe => Get(AxeName);
    
        public Item Pickaxe => Get(PickaxeName);

        private void Awake()
        {
            Singleton = this;
            _items = new()
            {
                {WoodName, new Item(WoodName, 
                    Resources.Load<Sprite>(WoodName + IconSuffix), 
                    Resources.Load<GameObject>(WoodName + ItemSuffix))},
        
                {StoneName, new Item(StoneName,
                    Resources.Load<Sprite>(StoneName + IconSuffix),
                    Resources.Load<GameObject>(StoneName + ItemSuffix))},
        
                {AxeName, new Item(AxeName,
                    Resources.Load<Sprite>(AxeName + IconSuffix),
                    Resources.Load<GameObject>(AxeName + ItemSuffix))},
        
                {PickaxeName, new Item(PickaxeName,
                    Resources.Load<Sprite>(PickaxeName + IconSuffix),
                    Resources.Load<GameObject>(PickaxeName + ItemSuffix))}
            };
        }
    }
    
    public enum ItemType
    {
        Wood,
        Stone,
        Axe,
        Pickaxe
    }
}
