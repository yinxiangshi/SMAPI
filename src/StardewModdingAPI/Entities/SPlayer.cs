using System;
using System.Collections.Generic;
using StardewModdingAPI.Framework;
using StardewValley;

namespace StardewModdingAPI.Entities
{
    /// <summary>Static class for integrating with the player.</summary>
    [Obsolete("This API was never officially documented and will be removed soon.")]
    public class SPlayer
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Obsolete.</summary>
        [Obsolete("Use " + nameof(Game1) + "." + nameof(Game1.getAllFarmers) + " instead")]
        public static List<Farmer> AllFarmers
        {
            get
            {
                Program.DeprecationManager.Warn(nameof(SPlayer), "1.0", DeprecationLevel.Info);
                return Game1.getAllFarmers();
            }
        }

        /// <summary>Obsolete.</summary>
        [Obsolete("Use " + nameof(Game1) + "." + nameof(Game1.player) + " instead")]
        public static Farmer CurrentFarmer
        {
            get
            {
                Program.DeprecationManager.Warn(nameof(SPlayer), "1.0", DeprecationLevel.Info);
                return Game1.player;
            }
        }

        /// <summary>Obsolete.</summary>
        [Obsolete("Use " + nameof(Game1) + "." + nameof(Game1.player) + " instead")]
        public static Farmer Player
        {
            get
            {
                Program.DeprecationManager.Warn(nameof(SPlayer), "1.0", DeprecationLevel.Info);
                return Game1.player;
            }
        }

        /// <summary>Obsolete.</summary>
        [Obsolete("Use " + nameof(Game1) + "." + nameof(Game1.player) + "." + nameof(Farmer.currentLocation) + " instead")]
        public static GameLocation CurrentFarmerLocation
        {
            get
            {
                Program.DeprecationManager.Warn(nameof(SPlayer), "1.0", DeprecationLevel.Info);
                return Game1.player.currentLocation;
            }
        }
    }
}