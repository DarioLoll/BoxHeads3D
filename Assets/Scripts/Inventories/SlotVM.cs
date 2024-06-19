using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventories
{
    public class SlotVM : MonoBehaviour
    {
        [SerializeField] private Image icon;
        
        [SerializeField] private TextMeshProUGUI itemCount;
        
        [SerializeField] private bool isHandSlot;
        
        public event Action<Slot> HandChanged;
        
        public event Action<Slot, int> SlotChanged; 
        
        private Slot _slot;
        
        public Slot Slot 
        {
            get => _slot;
            set
            {
                _slot = value;
                if (_slot.IsEmpty || _slot.Item == null)
                {
                    icon.color = Color.clear;
                    itemCount.text = string.Empty;
                }
                else
                {
                    icon.sprite = _slot.Item.Value.Icon;
                    icon.color = Color.white;
                    itemCount.text = _slot.AmountText;
                }
                OnSlotChanged(_slot, Index);
                if(isHandSlot) OnHandChanged(_slot);
            }
        }
        
        public int Index { get; set; }

        public void OnClick()
        {
            var inventoryVm = InventoryVm.Instance;
            if (inventoryVm.ItemOnCursor != null)
            {
                if (!Slot.IsEmpty)
                {
                    if (Slot.Item != inventoryVm.ItemOnCursor.Item) return;
                    Slot = new Slot(Slot.Item!.Value, Slot.Amount + inventoryVm.ItemOnCursor.Amount);
                    inventoryVm.ItemOnCursor = null;
                }
                else
                {
                    Slot = new Slot(inventoryVm.ItemOnCursor.Item!.Value, inventoryVm.ItemOnCursor.Amount);
                    inventoryVm.ItemOnCursor = null;
                }
            }
            else
            {
                if(Slot.IsEmpty) return;
                inventoryVm.ItemOnCursor = new Slot(Slot.Item!.Value, Slot.Amount);
                Slot = new Slot(null, 0);
            }
        }

        public void OnRightClick()
        {
            if (isHandSlot) return;
            if(Slot.IsEmpty) return;
            var inventoryVm = InventoryVm.Instance;
            if (inventoryVm.ItemOnCursor != null) return;
            var previousHand = inventoryVm.HandSlot.Slot;
            inventoryVm.HandSlot.Slot = Slot;
            Slot = previousHand!;
        }

        protected virtual void OnHandChanged(Slot newHand)
        {
            HandChanged?.Invoke(newHand);
        }

        protected virtual void OnSlotChanged(Slot obj, int index)
        {
            SlotChanged?.Invoke(obj, index);
        }
    }
}
