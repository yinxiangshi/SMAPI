#nullable disable

// ReSharper disable CheckNamespace -- matches Stardew Valley's code
using System.Collections;
using System.Collections.Generic;

namespace Netcode
{
    /// <summary>A simplified version of Stardew Valley's <c>Netcode.NetObjectList</c> for unit testing.</summary>
    public class NetList<T> : List<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable { }
}
