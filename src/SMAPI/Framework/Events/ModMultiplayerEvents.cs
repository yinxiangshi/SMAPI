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

        /// <summary>Raised after the mod context for a player is received. This happens before the game approves the connection, so the player does not yet exist in the game. This is the earliest point where messages can be sent to the player via SMAPI.</summary>
        public event EventHandler<ContextReceivedEventArgs> ContextReceived
        {
            add => this.EventManager.ContextReceived.Add(value);
            remove => this.EventManager.ContextReceived.Remove(value);
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
