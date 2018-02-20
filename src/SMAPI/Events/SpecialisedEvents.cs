using System;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events serving specialised edge cases that shouldn't be used by most mod.</summary>
    public static class SpecialisedEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised when the game updates its state (≈60 times per second), regardless of normal SMAPI validation. This event is not thread-safe and may be invoked while game logic is running asynchronously. Changes to game state in this method may crash the game or corrupt an in-progress save. Do not use this event unless you're fully aware of the context in which your code will be run. Mods using this method will trigger a stability warning in the SMAPI console.</summary>
        public static event EventHandler UnvalidatedUpdateTick;


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise an <see cref="UnvalidatedUpdateTick"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        internal static void InvokeUnvalidatedUpdateTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(SpecialisedEvents)}.{nameof(SpecialisedEvents.UnvalidatedUpdateTick)}", SpecialisedEvents.UnvalidatedUpdateTick?.GetInvocationList());
        }
    }
}
