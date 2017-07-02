using StardewValley.Tools;

namespace TrainerMod.Framework.ItemData
{
    /// <summary>A weapon that can be searched and added to the player's inventory through the console.</summary>
    internal class SearchableWeapon : ISearchItem
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying item.</summary>
        private readonly MeleeWeapon Weapon;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the item is valid.</summary>
        public bool IsValid => this.Weapon != null;

        /// <summary>The item ID.</summary>
        public int ID => this.Weapon.initialParentTileIndex;

        /// <summary>The item name.</summary>
        public string Name => this.Weapon.Name;

        /// <summary>The item type.</summary>
        public ItemType Type => ItemType.Weapon;


        /*********
        ** Accessors
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The weapon ID.</param>
        public SearchableWeapon(int id)
        {
            try
            {
                this.Weapon = new MeleeWeapon(id);
            }
            catch
            {
                // invalid
            }
        }
    }
}