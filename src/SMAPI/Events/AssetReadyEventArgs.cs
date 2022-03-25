using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for an <see cref="IContentEvents.AssetReady"/> event.</summary>
    public class AssetReadyEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The name of the asset being requested.</summary>
        public IAssetName Name { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The name of the asset being requested.</param>
        internal AssetReadyEventArgs(IAssetName name)
        {
            this.Name = name;
        }
    }
}
