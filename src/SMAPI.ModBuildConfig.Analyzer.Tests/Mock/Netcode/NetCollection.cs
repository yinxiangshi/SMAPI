// ReSharper disable CheckNamespace -- matches Stardew Valley's code
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Netcode
{
    /// <summary>A simplified version of Stardew Valley's <c>Netcode.NetCollection</c> for unit testing.</summary>
    public class NetCollection<T> : Collection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable { }
}
