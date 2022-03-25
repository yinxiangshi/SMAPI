using System;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <inheritdoc cref="IContentEvents" />
    internal class ModContentEvents : ModEventsBase, IContentEvents
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public event EventHandler<AssetRequestedEventArgs> AssetRequested
        {
            add => this.EventManager.AssetRequested.Add(value, this.Mod);
            remove => this.EventManager.AssetRequested.Remove(value);
        }

        /// <inheritdoc />
        public event EventHandler<AssetsInvalidatedEventArgs> AssetsInvalidated
        {
            add => this.EventManager.AssetsInvalidated.Add(value, this.Mod);
            remove => this.EventManager.AssetsInvalidated.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod which uses this instance.</param>
        /// <param name="eventManager">The underlying event manager.</param>
        internal ModContentEvents(IModMetadata mod, EventManager eventManager)
            : base(mod, eventManager) { }
    }
}
