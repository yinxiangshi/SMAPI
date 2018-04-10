// ReSharper disable CheckNamespace, InconsistentNaming -- matches Stardew Valley's code
using Netcode;

namespace StardewValley
{
    /// <summary>A simplified version of Stardew Valley's <c>StardewValley.Item</c> class for unit testing.</summary>
    public class Item
    {
        /// <summary>A net int field with an equivalent non-net <c>Category</c> property.</summary>
        public NetInt category { get; } = new NetInt { Value = 42 };

        /// <summary>A net int field with no equivalent non-net property.</summary>
        public NetInt type { get; } = new NetInt { Value = 42 };

        /// <summary>A net reference field.</summary>
        public NetRef refField { get; } = null;
    }
}
