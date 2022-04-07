#nullable disable

// ReSharper disable CheckNamespace, InconsistentNaming -- matches Stardew Valley's code
#pragma warning disable 649 // (never assigned) -- only used to test type conversions
using System.Collections.Generic;

namespace StardewValley
{
    /// <summary>A simplified version of Stardew Valley's <c>StardewValley.Farmer</c> class for unit testing.</summary>
    internal class Farmer
    {
        /// <summary>A sample field which should be replaced with a different property.</summary>
        public readonly IDictionary<string, int[]> friendships;
    }
}
