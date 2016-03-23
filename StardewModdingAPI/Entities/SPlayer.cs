using System.Collections.Generic;
using StardewModdingAPI.Inheritance;
using StardewValley;

namespace StardewModdingAPI.Entities
{
    public static class SPlayer
    {
        public static List<Farmer> AllFarmers
        {
            get
            {
                return SGame.getAllFarmers();
            }
        }

        public static Farmer CurrentFarmer
        {
            get
            {
                return SGame.player;
            }
        }

        public static GameLocation CurrentFarmerLocation
        {
            get
            {
                return SGame.player.currentLocation;
            }
        }
    }
}
