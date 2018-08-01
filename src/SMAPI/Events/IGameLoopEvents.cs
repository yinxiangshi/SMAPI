using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Events linked to the game's update loop. The update loop runs roughly ≈60 times/second to run game logic like state changes, action handling, etc. These can be useful, but you should consider more semantic events like <see cref="IInputEvents"/> if possible.</summary>
    public interface IGameLoopEvents
    {
        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        event EventHandler<GameLoopLaunchedEventArgs> Launched;

        /// <summary>Raised before the game performs its overall update tick (≈60 times per second).</summary>
        event EventHandler<GameLoopUpdatingEventArgs> Updating;

        /// <summary>Raised after the game performs its overall update tick (≈60 times per second).</summary>
        event EventHandler<GameLoopUpdatedEventArgs> Updated;
    }
}
