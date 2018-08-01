using System;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Events linked to the game's update loop. The update loop runs roughly ≈60 times/second to run game logic like state changes, action handling, etc. These can be useful, but you should consider more semantic events like <see cref="IInputEvents"/> if possible.</summary>
    internal class ModGameLoopEvents : ModEventsBase, IGameLoopEvents
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Raised after the game is launched, right before the first update tick.</summary>
        public event EventHandler<GameLoopLaunchedEventArgs> Launched
        {
            add => this.EventManager.GameLoop_Launched.Add(value);
            remove => this.EventManager.GameLoop_Launched.Remove(value);
        }

        /// <summary>Raised before the game performs its overall update tick (≈60 times per second).</summary>
        public event EventHandler<GameLoopUpdatingEventArgs> Updating
        {
            add => this.EventManager.GameLoop_Updating.Add(value);
            remove => this.EventManager.GameLoop_Updating.Remove(value);
        }

        /// <summary>Raised after the game performs its overall update tick (≈60 times per second).</summary>
        public event EventHandler<GameLoopUpdatedEventArgs> Updated
        {
            add => this.EventManager.GameLoop_Updated.Add(value);
            remove => this.EventManager.GameLoop_Updated.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod which uses this instance.</param>
        /// <param name="eventManager">The underlying event manager.</param>
        internal ModGameLoopEvents(IModMetadata mod, EventManager eventManager)
            : base(mod, eventManager) { }
    }
}
