using System.Collections.Generic;
using Inventories;
using UnityEngine;

namespace Collectables
{
    public static class CollectableTypes
    {
        public static readonly Dictionary<CollectableType, CollectableStats> Stats = new()
        {
            {CollectableType.Tree, new CollectableStats(Items.AxeName, 10, Items.WoodName)},
            {CollectableType.Rock, new CollectableStats(Items.PickaxeName, 5, Items.StoneName)}
        };
    }
    
    public enum CollectableType
    {
        Tree,
        Rock,
    }

    public struct CollectableStats
    {
        public string FittingItem { get; }

        public int BaseDamagePerHit { get; }

        public string DroppedItem { get;  }
        
        public CollectableStats(string fittingItem, int baseDamagePerHit, string droppedItem)
        {
            FittingItem = fittingItem;
            BaseDamagePerHit = baseDamagePerHit;
            DroppedItem = droppedItem;
        }
    }
}
