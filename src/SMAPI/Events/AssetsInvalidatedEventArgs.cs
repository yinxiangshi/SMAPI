using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for an <see cref="IContentEvents.AssetsInvalidated"/> event.</summary>
    public class AssetsInvalidatedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The asset names that were invalidated.</summary>
        public IEnumerable<IAssetName> Names { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="names">The asset names that were invalidated.</param>
        internal AssetsInvalidatedEventArgs(IEnumerable<IAssetName> names)
        {
            this.Names = names.ToArray();
        }
    }
}
