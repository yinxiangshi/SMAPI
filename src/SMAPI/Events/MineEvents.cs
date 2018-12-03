using System;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when something happens in the mines.</summary>
    [Obsolete("Use " + nameof(Mod.Helper) + "." + nameof(IModHelper.Events) + " instead. See https://smapi.io/3.0 for more info.")]
    public static class MineEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;

        /// <summary>Manages deprecation warnings.</summary>
        private static DeprecationManager DeprecationManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the player warps to a new level of the mine.</summary>
        public static event EventHandler<EventArgsMineLevelChanged> MineLevelChanged
        {
            add
            {
                MineEvents.DeprecationManager.WarnForOldEvents();
                MineEvents.EventManager.Legacy_MineLevelChanged.Add(value);
            }
            remove => MineEvents.EventManager.Legacy_MineLevelChanged.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        /// <param name="deprecationManager">Manages deprecation warnings.</param>
        internal static void Init(EventManager eventManager, DeprecationManager deprecationManager)
        {
            MineEvents.EventManager = eventManager;
            MineEvents.DeprecationManager = deprecationManager;
        }
    }
}
