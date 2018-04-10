// ReSharper disable CheckNamespace, InconsistentNaming -- matches Stardew Valley's code
using Netcode;

namespace StardewValley
{
    /// <summary>A simplified version of Stardew Valley's <c>StardewValley.Item</c> class for unit testing.</summary>
    public class Item
    {
        /// <summary>A net int field with an equivalent non-net <c>Category</c> property.</summary>
        public readonly NetInt category = new NetInt { Value = 42 };

        /// <summary>A generic net int field with no equivalent non-net property.</summary>
        public readonly NetInt netIntField = new NetInt { Value = 42 };

        /// <summary>A generic net ref field with no equivalent non-net property.</summary>
        public readonly NetRef<object> netRefField = new NetRef<object>();

        /// <summary>A generic net int property with no equivalent non-net property.</summary>
        public NetInt netIntProperty = new NetInt { Value = 42 };

        /// <summary>A generic net ref property with no equivalent non-net property.</summary>
        public NetRef<object> netRefProperty { get; } = new NetRef<object>();
    }
}
