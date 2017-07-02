using StardewValley;

namespace TrainerMod.Framework.ItemData
{
    /// <summary>An object that can be searched and added to the player's inventory through the console.</summary>
    internal class SearchableObject : ISearchItem
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying item.</summary>
        private readonly Item Item;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the item is valid.</summary>
        public bool IsValid => this.Item != null && this.Item.Name != "Broken Item";

        /// <summary>The item ID.</summary>
        public int ID => this.Item.parentSheetIndex;

        /// <summary>The item name.</summary>
        public string Name => this.Item.Name;

        /// <summary>The item type.</summary>
        public ItemType Type => ItemType.Object;


        /*********
        ** Accessors
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The item ID.</param>
        public SearchableObject(int id)
        {
            try
            {
                this.Item = new Object(id, 1);
            }
            catch
            {
                // invalid
            }
        }
    }
}