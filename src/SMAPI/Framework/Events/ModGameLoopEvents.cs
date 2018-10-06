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
        public event EventHandler<GameLaunchedEventArgs> GameLaunched
        {
            add => this.EventManager.GameLaunched.Add(value);
            remove => this.EventManager.GameLaunched.Remove(value);
        }

        /// <summary>Raised before the game performs its overall update tick (≈60 times per second).</summary>
        public event EventHandler<UpdateTickingEventArgs> UpdateTicking
        {
            add => this.EventManager.UpdateTicking.Add(value);
            remove => this.EventManager.UpdateTicking.Remove(value);
        }

        /// <summary>Raised after the game performs its overall update tick (≈60 times per second).</summary>
        public event EventHandler<UpdateTickedEventArgs> UpdateTicked
        {
            add => this.EventManager.UpdateTicked.Add(value);
            remove => this.EventManager.UpdateTicked.Remove(value);
        }

        /// <summary>Raised before the game creates a new save file.</summary>
        public event EventHandler<SaveCreatingEventArgs> SaveCreating
        {
            add => this.EventManager.SaveCreating.Add(value);
            remove => this.EventManager.SaveCreating.Remove(value);
        }

        /// <summary>Raised after the game finishes creating the save file.</summary>
        public event EventHandler<SaveCreatedEventArgs> SaveCreated
        {
            add => this.EventManager.SaveCreated.Add(value);
            remove => this.EventManager.SaveCreated.Remove(value);
        }

        /// <summary>Raised before the game begins writes data to the save file.</summary>
        public event EventHandler<SavingEventArgs> Saving
        {
            add => this.EventManager.Saving.Add(value);
            remove => this.EventManager.Saving.Remove(value);
        }

        /// <summary>Raised after the game finishes writing data to the save file.</summary>
        public event EventHandler<SavedEventArgs> Saved
        {
            add => this.EventManager.Saved.Add(value);
            remove => this.EventManager.Saved.Remove(value);
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        public event EventHandler<SaveLoadedEventArgs> SaveLoaded
        {
            add => this.EventManager.SaveLoaded.Add(value);
            remove => this.EventManager.SaveLoaded.Remove(value);
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        public event EventHandler<DayStartedEventArgs> DayStarted
        {
            add => this.EventManager.DayStarted.Add(value);
            remove => this.EventManager.DayStarted.Remove(value);
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
