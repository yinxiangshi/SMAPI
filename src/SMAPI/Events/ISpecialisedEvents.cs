using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Events serving specialised edge cases that shouldn't be used by most mods.</summary>
    public interface ISpecialisedEvents
    {
        /// <summary>Raised immediately after the player loads a save slot, but before the world is fully initialised. The save and game data are available at this point, but some in-game content (like location maps) haven't been initialised yet.</summary>
        event EventHandler<SavePreloadedEventArgs> SavePreloaded;

        /// <summary>Raised before the game state is updated (≈60 times per second), regardless of normal SMAPI validation. This event is not thread-safe and may be invoked while game logic is running asynchronously. Changes to game state in this method may crash the game or corrupt an in-progress save. Do not use this event unless you're fully aware of the context in which your code will be run. Mods using this event will trigger a stability warning in the SMAPI console.</summary>
        event EventHandler<UnvalidatedUpdateTickingEventArgs> UnvalidatedUpdateTicking;

        /// <summary>Raised after the game state is updated (≈60 times per second), regardless of normal SMAPI validation. This event is not thread-safe and may be invoked while game logic is running asynchronously. Changes to game state in this method may crash the game or corrupt an in-progress save. Do not use this event unless you're fully aware of the context in which your code will be run. Mods using this event will trigger a stability warning in the SMAPI console.</summary>
        event EventHandler<UnvalidatedUpdateTickedEventArgs> UnvalidatedUpdateTicked;
    }
}
