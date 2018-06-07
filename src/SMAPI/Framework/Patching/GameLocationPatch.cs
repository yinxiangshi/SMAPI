using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Harmony;
using StardewValley;
using xTile.Tiles;

namespace StardewModdingAPI.Framework.Patching
{
    /// <summary>A Harmony patch for the <see cref="GameLocation.updateSeasonalTileSheets"/> method.</summary>
    internal class GameLocationPatch : IHarmonyPatch
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A unique name for this patch.</summary>
        public string Name => $"{nameof(GameLocation)}.{nameof(GameLocation.updateSeasonalTileSheets)}";


        /*********
        ** Public methods
        *********/
        /// <summary>Apply the Harmony patch.</summary>
        /// <param name="harmony">The Harmony instance.</param>
        public void Apply(HarmonyInstance harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.updateSeasonalTileSheets));
            MethodInfo prefix = AccessTools.Method(this.GetType(), nameof(GameLocationPatch.Prefix));

            harmony.Patch(method, new HarmonyMethod(prefix), null);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>An implementation of <see cref="GameLocation.updateSeasonalTileSheets"/> which correctly handles custom map tilesheets.</summary>
        /// <param name="__instance">The location instance being patched.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument name is defined by Harmony.")]
        private static bool Prefix(ref GameLocation __instance)
        {
            if (!__instance.IsOutdoors || __instance.Name.Equals("Desert"))
                return false;
            foreach (TileSheet tilesheet in __instance.Map.TileSheets)
            {
                string imageSource = tilesheet.ImageSource;
                string imageFile = Path.GetFileName(imageSource);
                if (imageFile.StartsWith("spring_") || imageFile.StartsWith("summer_") || imageFile.StartsWith("fall_") || imageFile.StartsWith("winter_"))
                {
                    string imageDir = Path.GetDirectoryName(imageSource);
                    if (string.IsNullOrWhiteSpace(imageDir))
                        imageDir = "Maps";
                    tilesheet.ImageSource = Path.Combine(imageDir, Game1.currentSeason + "_" + imageFile.Split('_')[1]);
                }
            }

            return false;
        }
    }
}
