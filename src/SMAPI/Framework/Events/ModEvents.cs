using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Manages access to events raised by SMAPI.</summary>
    internal class ModEvents : IModEvents
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Events linked to the game's update loop. The update loop runs roughly ≈60 times/second to run game logic like state changes, action handling, etc. These can be useful, but you should consider more semantic events like <see cref="IModEvents.Input"/> if possible.</summary>
        public IGameLoopEvents GameLoop { get; }

        /// <summary>Events raised when the player provides input using a controller, keyboard, or mouse.</summary>
        public IInputEvents Input { get; }

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
            this.GameLoop = new ModGameLoopEvents(mod, eventManager);
            this.Input = new ModInputEvents(mod, eventManager);
            this.World = new ModWorldEvents(mod, eventManager);
        }
    }
}
