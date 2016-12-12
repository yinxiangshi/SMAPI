namespace TrainerMod.ItemData
{
    /// <summary>An item type that can be searched and added to the player through the console.</summary>
    internal enum ItemType
    {
        /// <summary>Any object in <see cref="StardewValley.Game1.objectInformation"/> (except rings).</summary>
        Object,

        /// <summary>A ring in <see cref="StardewValley.Game1.objectInformation"/>.</summary>
        Ring,

        /// <summary>A weapon from <c>Data\weapons</c>.</summary>
        Weapon
    }
}
