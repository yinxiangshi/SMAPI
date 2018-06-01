using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Manages access to events raised by SMAPI.</summary>
    internal class ModEvents : IModEvents
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Events raised when something changes in the world.</summary>
        public IWorldEvents World { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod which uses this instance.</param>
        /// <param name="eventManager">The underlying event manager.</param>
        public ModEvents(IModMetadata mod, EventManager eventManager)
        {
            this.World = new ModWorldEvents(mod, eventManager);
        }
    }
}
