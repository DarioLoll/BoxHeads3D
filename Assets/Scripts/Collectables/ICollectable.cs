namespace Collectables
{
    public interface ICollectable
    {
        CollectableType Type { get; set; }
        
        CollectableStats Stats { get; }

        int Durability { get; set; }
        
        int ItemDropCount { get; set; }
        
        void OnHit(string itemName);
        
        void OnCollect();
    }
}
