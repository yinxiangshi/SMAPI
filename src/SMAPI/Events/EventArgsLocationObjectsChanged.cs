using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using SObject = StardewValley.Object;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="LocationEvents.LocationObjectsChanged"/> or <see cref="LocationEvents.ObjectsChanged"/> event.</summary>
    public class EventArgsLocationObjectsChanged : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location which changed.</summary>
        public GameLocation Location { get; }

        /// <summary>The objects added to the list.</summary>
        public IEnumerable<KeyValuePair<Vector2, SObject>> Added { get; }

        /// <summary>The objects removed from the list.</summary>
        public IEnumerable<KeyValuePair<Vector2, SObject>> Removed { get; }

        /// <summary>The current list of objects in the current location.</summary>
        [Obsolete("Use " + nameof(EventArgsLocationObjectsChanged.Added))]
        public IDictionary<Vector2, NetRef<SObject>> NewObjects { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location which changed.</param>
        /// <param name="added">The objects added to the list.</param>
        /// <param name="removed">The objects removed from the list.</param>
        /// <param name="newObjects">The current list of objects in the current location.</param>
        public EventArgsLocationObjectsChanged(GameLocation location, IEnumerable<KeyValuePair<Vector2, SObject>> added, IEnumerable<KeyValuePair<Vector2, SObject>> removed, IDictionary<Vector2, NetRef<SObject>> newObjects)
        {
            this.Location = location;
            this.Added = added.ToArray();
            this.Removed = removed.ToArray();
            this.NewObjects = newObjects;
        }
    }
}
