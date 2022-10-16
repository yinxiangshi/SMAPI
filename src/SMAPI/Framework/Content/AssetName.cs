using System;
using StardewModdingAPI.Toolkit.Utilities;
using StardewModdingAPI.Utilities.AssetPathUtilities;
using StardewValley;
using ToolkitPathUtilities = StardewModdingAPI.Toolkit.Utilities.PathUtilities;

namespace StardewModdingAPI.Framework.Content
{
    /// <summary>An asset name that can be loaded through the content pipeline.</summary>
    internal class AssetName : IAssetName
    {
        /*********
        ** Fields
        *********/
        /// <summary>A lowercase version of <see cref="Name"/> used for consistent hash codes and equality checks.</summary>
        private readonly string ComparableName;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string BaseName { get; }

        /// <inheritdoc />
        public string? LocaleCode { get; }

        /// <inheritdoc />
        public LocalizedContentManager.LanguageCode? LanguageCode { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="baseName">The base asset name without the locale code.</param>
        /// <param name="localeCode">The locale code specified in the <see cref="Name"/>, if it's a valid code recognized by the game content.</param>
        /// <param name="languageCode">The language code matching the <see cref="LocaleCode"/>, if applicable.</param>
        public AssetName(string baseName, string? localeCode, LocalizedContentManager.LanguageCode? languageCode)
        {
            // validate
            if (string.IsNullOrWhiteSpace(baseName))
                throw new ArgumentException("The asset name can't be null or empty.", nameof(baseName));
            if (string.IsNullOrWhiteSpace(localeCode))
                localeCode = null;

            // set base values
            this.BaseName = PathUtilities.NormalizeAssetName(baseName);
            this.LocaleCode = localeCode;
            this.LanguageCode = languageCode;

            // set derived values
            this.Name = localeCode != null
                ? string.Concat(this.BaseName, '.', this.LocaleCode)
                : this.BaseName;
            this.ComparableName = this.Name.ToLowerInvariant();
        }

        /// <summary>Parse a raw asset name into an instance.</summary>
        /// <param name="rawName">The raw asset name to parse.</param>
        /// <param name="parseLocale">Get the language code for a given locale, if it's valid.</param>
        /// <exception cref="ArgumentException">The <paramref name="rawName"/> is null or empty.</exception>
        public static AssetName Parse(string rawName, Func<string, LocalizedContentManager.LanguageCode?> parseLocale)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ArgumentException("The asset name can't be null or empty.", nameof(rawName));

            string baseName = rawName;
            string? localeCode = null;
            LocalizedContentManager.LanguageCode? languageCode = null;

            int lastPeriodIndex = rawName.LastIndexOf('.');
            if (lastPeriodIndex > 0 && rawName.Length > lastPeriodIndex + 1)
            {
                string possibleLocaleCode = rawName[(lastPeriodIndex + 1)..];
                LocalizedContentManager.LanguageCode? possibleLanguageCode = parseLocale(possibleLocaleCode);

                if (possibleLanguageCode != null)
                {
                    baseName = rawName[..lastPeriodIndex];
                    localeCode = possibleLocaleCode;
                    languageCode = possibleLanguageCode;
                }
            }

            return new AssetName(baseName, localeCode, languageCode);
        }

        /// <inheritdoc />
        public bool IsEquivalentTo(string? assetName, bool useBaseName = false)
        {
            // empty asset key is never equivalent
            if (string.IsNullOrWhiteSpace(assetName))
                return false;

            AssetNamePartEnumerator curParts = new(useBaseName ? this.BaseName : this.Name);
            AssetNamePartEnumerator otherParts = new(assetName);

            while (true)
            {
                bool otherHasMore = otherParts.MoveNext();
                bool curHasMore = curParts.MoveNext();

                // neither of us have any more to yield, I'm done.
                if (!otherHasMore && !curHasMore)
                    return true;

                // One of us has more but the other doesn't, this isn't a match.
                if (otherHasMore ^ curHasMore)
                    return false;

                // My next bit doesn't match their next bit, this isn't a match.
                if (!curParts.Current.Equals(otherParts.Current, StringComparison.OrdinalIgnoreCase))
                    return false;

                // continue checking.
            }
        }

        /// <inheritdoc />
        public bool IsEquivalentTo(IAssetName? assetName, bool useBaseName = false)
        {
            if (useBaseName)
                return this.BaseName.Equals(assetName?.BaseName, StringComparison.OrdinalIgnoreCase);

            if (assetName is AssetName impl)
                return this.ComparableName == impl.ComparableName;

            return this.Name.Equals(assetName?.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public bool StartsWith(string? prefix, bool allowPartialWord = true, bool allowSubfolder = true)
        {
            // asset keys never start with null
            if (prefix is null)
                return false;

            ReadOnlySpan<char> trimmed = prefix.AsSpan().Trim();

            // just because most ReadOnlySpan/Span APIs expect a ReadOnlySpan/Span, easier to read.
            ReadOnlySpan<char> pathSeparators = new(ToolkitPathUtilities.PossiblePathSeparators);

            // asset keys can't have a leading slash, but AssetPathYielder won't yield that.
            if (pathSeparators.Contains(trimmed[0]))
                return false;

            if (trimmed.Length == 0)
                return true;

            AssetNamePartEnumerator curParts = new(this.Name);
            AssetNamePartEnumerator prefixParts = new(trimmed);

            while (true)
            {
                bool prefixHasMore = prefixParts.MoveNext();
                bool curHasMore = curParts.MoveNext();

                // Neither of us have any more to yield, I'm done.
                if (!prefixHasMore && !curHasMore)
                    return true;

                // the prefix is actually longer than the asset name, this can't be true.
                if (prefixHasMore && !curHasMore)
                    return false;

                // they're done, I have more. (These are going to be word boundaries, I don't need to check that).
                if (!prefixHasMore && curHasMore)
                {
                    return allowSubfolder || !curParts.Remainder.Contains(pathSeparators, StringComparison.Ordinal);
                }

                // check my next segment against theirs.
                if (prefixHasMore && curHasMore)
                {
                    // my next segment doesn't match theirs.
                    if (!curParts.Current.StartsWith(prefixParts.Current, StringComparison.OrdinalIgnoreCase))
                        return false;

                    // my next segment starts with theirs but isn't an exact match.
                    if (curParts.Current.Length != prefixParts.Current.Length)
                    {
                        // something like "Maps/" would require an exact match.
                        if (pathSeparators.Contains(trimmed[^1]))
                            return false;

                        // check for partial word.
                        if (!allowPartialWord
                            && char.IsLetterOrDigit(prefixParts.Current[^1]) // last character in suffix is not word separator
                            && char.IsLetterOrDigit(curParts.Current[prefixParts.Current.Length]) // and the first character after it isn't either.
                            )
                            return false;

                        return allowSubfolder || !curParts.Remainder.Contains(pathSeparators, StringComparison.Ordinal);
                    }

                    // exact matches should continue checking.
                }
            }
        }

        /// <inheritdoc />
        public bool IsDirectlyUnderPath(string? assetFolder)
        {
            if (assetFolder is null)
                return false;

            return this.StartsWith(assetFolder + "/", allowPartialWord: false, allowSubfolder: false);
        }

        /// <inheritdoc />
        IAssetName IAssetName.GetBaseAssetName()
        {
            return this.LocaleCode == null
                ? this
                : new AssetName(this.BaseName, null, null);
        }

        /// <inheritdoc />
        public bool Equals(IAssetName? other)
        {
            return other switch
            {
                null => false,
                AssetName otherImpl => this.ComparableName == otherImpl.ComparableName,
                _ => StringComparer.OrdinalIgnoreCase.Equals(this.Name, other.Name)
            };
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.ComparableName.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Name;
        }
    }
}
