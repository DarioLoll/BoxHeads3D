using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Inventories
{
    public class InventoryVm : MonoBehaviour
    {
        [SerializeField] private Transform itemStackPrefab;
        
        public static InventoryVm Instance { get; private set; }
        
        private List<SlotVM> _slots = new List<SlotVM>();
        
        private SlotVM _handSlot;

        public SlotVM HandSlot
        {
            get => _handSlot;
            set
            {
                _handSlot = value;
                _handSlot.Index = -1;
            }
        }

        private Inventory _inventory;

        public Inventory Inventory
        {
            get => _inventory;
            set
            {
                _inventory = value;
                _inventory.SlotUpdated += OnSlotUpdated;
                for (int i = 0; i < _inventory.Slots.Count; i++)
                {
                    _slots[i].Slot = _inventory.Slots[i];
                    _slots[i].Index = i;
                }
            }
        
        }
        

        private void OnSlotUpdated(int index)
        { 
            _slots[index].Slot = _inventory.Slots[index];  
        }

        [CanBeNull] private Slot _itemOnCursor;

        [CanBeNull]
        public Slot ItemOnCursor
        {
            get => _itemOnCursor;
            set
            {
                _itemOnCursor = value;
                if (_itemOnCursorIsInstantiated)
                {
                    Destroy(_itemOnCursorInstance!.gameObject);
                    _itemOnCursorInstance = null;
                    _itemOnCursorIsInstantiated = false;
                }

                if (_itemOnCursor == null) return;
                _itemOnCursorInstance = Instantiate(itemStackPrefab, transform).GetComponent<SlotVM>();
                _itemOnCursorInstance!.Slot = _itemOnCursor;
                _itemOnCursorIsInstantiated = true;
            }
        }
        [CanBeNull] private SlotVM _itemOnCursorInstance;
        private bool _itemOnCursorIsInstantiated;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
        }

        void Start()
        {
            Transform slotParent = transform;
            for (int i = 0; i < slotParent.childCount - 1; i++)
            {
                SlotVM slot = slotParent.GetChild(i).GetComponent<SlotVM>();
                if (slot != null)
                {
                    _slots.Add(slot);
                    slot.SlotChanged += OnSlotChanged;
                }
            }
            HandSlot = slotParent.GetChild(slotParent.childCount - 1).GetComponent<SlotVM>();
        }

        private void OnSlotChanged(Slot slot, int index)
        {
            _inventory.Slots[index] = slot;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_itemOnCursorIsInstantiated) return;
            _itemOnCursorInstance!.transform.position = Input.mousePosition;
        }
    }
}
