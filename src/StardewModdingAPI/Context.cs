using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewModdingAPI
{
    /// <summary>Provides information about the current game state.</summary>
    public static class Context
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Public
        ****/
        /// <summary>Whether the player has loaded a save and the world has finished initialising.</summary>
        public static bool IsWorldReady { get; internal set; }

        /// <summary>Whether the player is free to move around (e.g. save is loaded, no menu is displayed, no cutscene is in progress, etc).</summary>
        public static bool IsPlayerFree => Context.IsWorldReady && Game1.activeClickableMenu == null && Game1.player.CanMove && !Game1.dialogueUp && !Game1.eventUp;

        /// <summary>Whether the game is currently running the draw loop. This isn't relevant to most mods, since you should use <see cref="GraphicsEvents.OnPostRenderEvent"/> to draw to the screen.</summary>
        public static bool IsInDrawLoop { get; internal set; }

        /****
        ** Internal
        ****/
        /// <summary>Whether a player save has been loaded.</summary>
        internal static bool IsSaveLoaded => Game1.hasLoadedGame && !string.IsNullOrEmpty(Game1.player.name);

        /// <summary>Whether the game is currently writing to the save file.</summary>
        internal static bool IsSaving => Game1.activeClickableMenu is SaveGameMenu || Game1.activeClickableMenu is ShippingMenu; // saving is performed by SaveGameMenu, but it's wrapped by ShippingMenu on days when the player shipping something
    }
}
