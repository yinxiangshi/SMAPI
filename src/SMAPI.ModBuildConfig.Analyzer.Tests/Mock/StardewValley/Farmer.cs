// ReSharper disable CheckNamespace, InconsistentNaming -- matches Stardew Valley's code
#pragma warning disable 649 // (never assigned) -- only used to test type conversions
using System.Collections.Generic;
using Netcode;

namespace StardewValley
{
    /// <summary>A simplified version of Stardew Valley's <c>StardewValley.Farmer</c> class for unit testing.</summary>
    internal class Farmer
    {
        /// <summary>A sample field which should be replaced with a different property.</summary>
        public readonly IDictionary<string, int[]> friendships;

        /// <summary>A sample net list.</summary>
        public readonly NetList<int> eventsSeen;

        /// <summary>A sample net object list.</summary>
        public readonly NetObjectList<int> netObjectList;
    }
}
