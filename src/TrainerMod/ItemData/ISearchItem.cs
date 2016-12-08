namespace TrainerMod.ItemData
{
    /// <summary>An item that can be searched and added to the player's inventory through the console.</summary>
    internal interface ISearchItem
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the item is valid.</summary>
        bool IsValid { get; }

        /// <summary>The item ID.</summary>
        int ID { get; }

        /// <summary>The item name.</summary>
        string Name { get; }

        /// <summary>The item type.</summary>
        ItemType Type { get; }
    }
}