using StardewModdingAPI.Framework.Events;
using StardewValley;

namespace StardewModdingAPI.Framework
{
    /// <summary>SMAPI's implementation of the game's core multiplayer logic.</summary>
    internal class SMultiplayer : Multiplayer
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Manages SMAPI events.</summary>
        private readonly EventManager EventManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="eventManager">Manages SMAPI events.</param>
        public SMultiplayer(IMonitor monitor, EventManager eventManager)
        {
            this.Monitor = monitor;
            this.EventManager = eventManager;
        }

        /// <summary>Handle sync messages from other players and perform other initial sync logic.</summary>
        public override void UpdateEarly()
        {
            this.EventManager.Multiplayer_BeforeMainSync.Raise();
            base.UpdateEarly();
            this.EventManager.Multiplayer_AfterMainSync.Raise();
        }

        /// <summary>Broadcast sync messages to other players and perform other final sync logic.</summary>
        public override void UpdateLate(bool forceSync = false)
        {
            this.EventManager.Multiplayer_BeforeMainBroadcast.Raise();
            base.UpdateLate(forceSync);
            this.EventManager.Multiplayer_AfterMainBroadcast.Raise();
        }
    }
}
