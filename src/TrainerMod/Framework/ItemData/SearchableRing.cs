using StardewValley.Objects;

namespace TrainerMod.Framework.ItemData
{
    /// <summary>A ring that can be searched and added to the player's inventory through the console.</summary>
    internal class SearchableRing : ISearchItem
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying item.</summary>
        private readonly Ring Ring;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the item is valid.</summary>
        public bool IsValid => this.Ring != null;

        /// <summary>The item ID.</summary>
        public int ID => this.Ring.parentSheetIndex;

        /// <summary>The item name.</summary>
        public string Name => this.Ring.Name;

        /// <summary>The item type.</summary>
        public ItemType Type => ItemType.Ring;


        /*********
        ** Accessors
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The ring ID.</param>
        public SearchableRing(int id)
        {
            try
            {
                this.Ring = new Ring(id);
            }
            catch
            {
                // invalid
            }
        }
    }
}