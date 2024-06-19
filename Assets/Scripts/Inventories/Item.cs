using System;
using UnityEngine;

namespace Inventories
{
    [System.Serializable]
    public struct Item : IEquatable<Item>
    {
        public string Name { get; }

        public Sprite Icon { get; }

        public GameObject Model { get; }
    
        public Item(string name, Sprite icon, GameObject model)
        {
            Name = name;
            Icon = icon;
            Model = model;
        }

        public bool Equals(Item other)
        {
            return Name == other.Name;
        }
        
        public static bool operator ==(Item left, Item right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(Item left, Item right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is Item other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
