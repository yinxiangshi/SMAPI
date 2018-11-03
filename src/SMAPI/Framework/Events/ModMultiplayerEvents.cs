using System;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Events raised for multiplayer messages and connections.</summary>
    internal class ModMultiplayerEvents : ModEventsBase, IMultiplayerEvents
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Raised after a mod message is received over the network.</summary>
        public event EventHandler<ModMessageReceivedEventArgs> ModMessageReceived
        {
            add => this.EventManager.ModMessageReceived.Add(value);
            remove => this.EventManager.ModMessageReceived.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod which uses this instance.</param>
        /// <param name="eventManager">The underlying event manager.</param>
        internal ModMultiplayerEvents(IModMetadata mod, EventManager eventManager)
            : base(mod, eventManager) { }
    }
}
