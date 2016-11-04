using StardewValley;

namespace StardewModdingAPI.Inheritance
{
    public class ItemStackChange
    {
        public Item Item { get; set; }
        public int StackChange { get; set; }
        public ChangeType ChangeType { get; set; }
    }
}