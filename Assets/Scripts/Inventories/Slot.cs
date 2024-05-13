namespace Inventories
{
    public class Slot
    {
        public Item? Item { get; set; }
    
        private int _amount;

        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                if (_amount <= 0)
                {
                    Item = null;
                }
            }
        }
        
        public string AmountText => Amount >= 1000 ? "999+" : Amount.ToString();

        public bool IsEmpty => Item == null;

        public Slot(Item? item, int amount)
        {
            Item = item;
            _amount = amount;
        }
    }
}
