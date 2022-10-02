using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Toolkit.Framework.UpdateData;

namespace StardewModdingAPI.Web.Framework
{
    /// <summary>Generic metadata about a mod page.</summary>
    internal interface IModPage
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod site containing the mod.</summary>
        ModSiteKey Site { get; }

        /// <summary>The mod's unique ID within the site.</summary>
        string Id { get; }

        /// <summary>The mod name.</summary>
        string? Name { get; }

        /// <summary>The mod's semantic version number.</summary>
        string? Version { get; }

        /// <summary>The mod's web URL.</summary>
        string? Url { get; }

        /// <summary>The mod downloads.</summary>
        IModDownload[] Downloads { get; }

        /// <summary>The mod page status.</summary>
        RemoteModStatus Status { get; }

        /// <summary>A user-friendly error which indicates why fetching the mod info failed (if applicable).</summary>
        string? Error { get; }

        /// <summary>Whether the mod data is valid.</summary>
        [MemberNotNullWhen(true, nameof(IModPage.Name), nameof(IModPage.Url))]
        [MemberNotNullWhen(false, nameof(IModPage.Error))]
        bool IsValid { get; }

        /// <summary>
        ///   Does this page use strict subkey matching.  Pages that use string subkey matching do not fall back
        ///   to searching for versions without a subkey if there are no versions found when given a subkey.
        ///   Additionally, the leading <c>@</c> is stripped from the subkey value before searching for matches.
        /// </summary>
        bool IsSubkeyStrict { get; }

        /*********
        ** Methods
        *********/

        /// <summary>Get the mod name associated with the given subkey, if any.</summary>
        /// <param name="subkey">The subkey.</param>
        /// <returns>The mod name associated with the given subkey (if any)</returns>
        string? GetName(string? subkey);

        /// <summary>Get the URL for the mod associated with the given subkey, if any.</summary>
        /// <param name="subkey">The subkey.</param>
        /// <returns>The URL for the mod associated with the given subkey (if any)</returns>
        string? GetUrl(string? subkey);

        /// <summary>Set the fetched mod info.</summary>
        /// <param name="name">The mod name.</param>
        /// <param name="version">The mod's semantic version number.</param>
        /// <param name="url">The mod's web URL.</param>
        /// <param name="downloads">The mod downloads.</param>
        IModPage SetInfo(string name, string? version, string url, IEnumerable<IModDownload> downloads);

        /// <summary>Set a mod fetch error.</summary>
        /// <param name="status">The mod availability status on the remote site.</param>
        /// <param name="error">A user-friendly error which indicates why fetching the mod info failed (if applicable).</param>
        IModPage SetError(RemoteModStatus status, string error);
    }
}
