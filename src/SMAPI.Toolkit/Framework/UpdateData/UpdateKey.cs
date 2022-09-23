using System;
using System.Diagnostics.CodeAnalysis;

namespace StardewModdingAPI.Toolkit.Framework.UpdateData
{
    /// <summary>A namespaced mod ID which uniquely identifies a mod within a mod repository.</summary>
    public class UpdateKey : IEquatable<UpdateKey>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw update key text.</summary>
        public string RawText { get; }

        /// <summary>The mod site containing the mod.</summary>
        public ModSiteKey Site { get; }

        /// <summary>The mod ID within the repository.</summary>
        public string? ID { get; }

        /// <summary>If specified, a substring in download names/descriptions to match.</summary>
        public string? Subkey { get; }

        /// <summary>Whether the update key seems to be valid.</summary>
#if NET5_0_OR_GREATER
        [MemberNotNullWhen(true, nameof(UpdateKey.ID))]
#endif
        public bool LooksValid { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="rawText">The raw update key text.</param>
        /// <param name="site">The mod site containing the mod.</param>
        /// <param name="id">The mod ID within the site.</param>
        /// <param name="subkey">If specified, a substring in download names/descriptions to match.</param>
        public UpdateKey(string? rawText, ModSiteKey site, string? id, string? subkey)
        {
            this.RawText = rawText?.Trim() ?? string.Empty;
            this.Site = site;
            this.ID = id?.Trim();
            this.Subkey = subkey?.Trim();
            this.LooksValid =
                site != ModSiteKey.Unknown
                && !string.IsNullOrWhiteSpace(id);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="site">The mod site containing the mod.</param>
        /// <param name="id">The mod ID within the site.</param>
        /// <param name="subkey">If specified, a substring in download names/descriptions to match.</param>
        public UpdateKey(ModSiteKey site, string? id, string? subkey)
            : this(UpdateKey.GetString(site, id, subkey), site, id, subkey) { }

        /// <summary>
        ///   Split a string into two at a delimiter.  If the delimiter does not appear in the string then the second
        ///   value of the returned tuple is null.  Both returned strings are trimmed of whitespace.
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <param name="delimiter">The character on which to split.</param>
        /// <param name="keepDelimiter">
        ///   If <c>true</c> then the second string returned will include the delimiter character
        ///   (provided that the string is not <c>null</c>)
        /// </param>
        /// <returns>
        ///   A pair containing the string consisting of all characters in <paramref name="s"/> before the first
        ///   occurrence of <paramref name="delimiter"/>, and a string consisting of all characters in <paramref name="s"/>
        ///   after the first occurrence of <paramref name="delimiter"/> or <c>null</c> if the delimiter does not
        ///   exist in s.  Both strings are trimmed of whitespace.
        /// </returns>
        private static (string, string?) Bifurcate(string s, char delimiter, bool keepDelimiter = false) {
            int pos = s.IndexOf(delimiter);
            if (pos < 0)
                return (s.Trim(), null);
            return (s.Substring(0, pos).Trim(), s.Substring(pos + (keepDelimiter ? 0 : 1)).Trim());
        }

        /// <summary>Parse a raw update key.</summary>
        /// <param name="raw">The raw update key to parse.</param>
        public static UpdateKey Parse(string? raw)
        {
            if (raw is null)
                return new UpdateKey(raw, ModSiteKey.Unknown, null, null);
            // extract site + ID
            (string rawSite, string? id) = Bifurcate(raw, ':');
            if (string.IsNullOrWhiteSpace(id))
                id = null;

            // extract subkey
            string? subkey = null;
            if (id != null)
                (id, subkey) = Bifurcate(id, '@', true);

            // parse
            if (!Enum.TryParse(rawSite, true, out ModSiteKey site))
                return new UpdateKey(raw, ModSiteKey.Unknown, id, subkey);
            if (id == null)
                return new UpdateKey(raw, site, null, subkey);

            return new UpdateKey(raw, site, id, subkey);
        }

        /// <summary>Parse a raw update key if it's valid.</summary>
        /// <param name="raw">The raw update key to parse.</param>
        /// <param name="parsed">The parsed update key, if valid.</param>
        /// <returns>Returns whether the update key was successfully parsed.</returns>
        public static bool TryParse(string raw, out UpdateKey parsed)
        {
            parsed = UpdateKey.Parse(raw);
            return parsed.LooksValid;
        }

        /// <summary>Get a string that represents the current object.</summary>
        public override string ToString()
        {
            return this.LooksValid
                ? UpdateKey.GetString(this.Site, this.ID, this.Subkey)
                : this.RawText;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(UpdateKey? other)
        {
            if (!this.LooksValid)
            {
                return
                    other?.LooksValid == false
                    && this.RawText.Equals(other.RawText, StringComparison.OrdinalIgnoreCase);
            }

            return
                other != null
                && this.Site == other.Site
                && string.Equals(this.ID, other.ID, StringComparison.OrdinalIgnoreCase)
                && string.Equals(this.Subkey, other.Subkey, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object? obj)
        {
            return obj is UpdateKey other && this.Equals(other);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return this.ToString().ToLower().GetHashCode();
        }

        /// <summary>Get the string representation of an update key.</summary>
        /// <param name="site">The mod site containing the mod.</param>
        /// <param name="id">The mod ID within the repository.</param>
        /// <param name="subkey">If specified, a substring in download names/descriptions to match.</param>
        public static string GetString(ModSiteKey site, string? id, string? subkey = null)
        {
            return $"{site}:{id}{subkey}".Trim();
        }
    }
}
