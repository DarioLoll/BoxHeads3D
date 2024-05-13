using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventories
{
    public class Inventory
    {
        private List<Slot> _slots = new List<Slot>();
        
        public List<Slot> Slots => _slots;
        
        public Slot HandSlot { get; set; } = new Slot(null, 0);
        
        public event Action<int> SlotUpdated;

        public Inventory(int size)
        {
            for (int i = 0; i < size; i++)
            {
                Slot slot = new Slot(null, 0);
                if (i == 0)
                {
                    slot = new Slot(Items.Singleton.Axe, 1);
                }
                if(i == 1)
                {
                    slot = new Slot(Items.Singleton.Pickaxe, 1);
                }
                if(i == 2)
                {
                    slot = new Slot(Items.Singleton.Wood, 10);
                }
                if(i == 3)
                {
                    slot = new Slot(Items.Singleton.Stone, 10);
                }
                _slots.Add(slot);
            }
        }

        public bool AddItem(Item item)
        {
            Slot slot = _slots.FirstOrDefault(s => s.Item == item);
            if (slot == null)
            {
                slot = _slots.FirstOrDefault(s => s.IsEmpty);
                if (slot == null) return false;
                slot.Item = item;
                slot.Amount = 1;
                OnSlotUpdated(_slots.IndexOf(slot));
                return true;
            }
            slot.Amount++;
            OnSlotUpdated(_slots.IndexOf(slot));
            return true;
        }

        protected virtual void OnSlotUpdated(int index)
        {
            SlotUpdated?.Invoke(index);
        }
    }
}
