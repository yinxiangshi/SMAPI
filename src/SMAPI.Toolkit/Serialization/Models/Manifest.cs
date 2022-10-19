using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Serialization.Converters;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingAPI.Toolkit.Serialization.Models
{
    /// <summary>A manifest which describes a mod for SMAPI.</summary>
    public class Manifest : IManifest
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public string Name { get; }

        /// <summary>A brief description of the mod.</summary>
        public string Description { get; }

        /// <summary>The mod author's name.</summary>
        public string Author { get; }

        /// <summary>The mod version.</summary>
        public ISemanticVersion Version { get; }

        /// <summary>The minimum SMAPI version required by this mod, if any.</summary>
        public ISemanticVersion? MinimumApiVersion { get; }

        /// <summary>The name of the DLL in the directory that has the <c>Entry</c> method. Mutually exclusive with <see cref="ContentPackFor"/>.</summary>
        public string? EntryDll { get; }

        /// <summary>The mod which will read this as a content pack. Mutually exclusive with <see cref="Manifest.EntryDll"/>.</summary>
        [JsonConverter(typeof(ManifestContentPackForConverter))]
        public IManifestContentPackFor? ContentPackFor { get; }

        /// <summary>The other mods that must be loaded before this mod.</summary>
        [JsonConverter(typeof(ManifestDependencyArrayConverter))]
        public IManifestDependency[] Dependencies { get; }

        /// <summary>The namespaced mod IDs to query for updates (like <c>Nexus:541</c>).</summary>
        public string[] UpdateKeys { get; private set; }

        /// <summary>The unique mod ID.</summary>
        public string UniqueID { get; }

        /// <summary>Any manifest fields which didn't match a valid field.</summary>
        [JsonExtensionData]
        public IDictionary<string, object> ExtraFields { get; } = new Dictionary<string, object>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance for a transitional content pack.</summary>
        /// <param name="uniqueID">The unique mod ID.</param>
        /// <param name="name">The mod name.</param>
        /// <param name="author">The mod author's name.</param>
        /// <param name="description">A brief description of the mod.</param>
        /// <param name="version">The mod version.</param>
        /// <param name="contentPackFor">The modID which will read this as a content pack.</param>
        public Manifest(string uniqueID, string name, string author, string description, ISemanticVersion version, string? contentPackFor = null)
            : this(
                uniqueId: uniqueID,
                name: name,
                author: author,
                description: description,
                version: version,
                minimumApiVersion: null,
                entryDll: null,
                contentPackFor: contentPackFor != null
                    ? new ManifestContentPackFor(contentPackFor, null)
                    : null,
                dependencies: null,
                updateKeys: null
            )
        { }

        /// <summary>Construct an instance for a transitional content pack.</summary>
        /// <param name="uniqueId">The unique mod ID.</param>
        /// <param name="name">The mod name.</param>
        /// <param name="author">The mod author's name.</param>
        /// <param name="description">A brief description of the mod.</param>
        /// <param name="version">The mod version.</param>
        /// <param name="minimumApiVersion">The minimum SMAPI version required by this mod, if any.</param>
        /// <param name="entryDll">The name of the DLL in the directory that has the <c>Entry</c> method. Mutually exclusive with <see cref="ContentPackFor"/>.</param>
        /// <param name="contentPackFor">The modID which will read this as a content pack.</param>
        /// <param name="dependencies">The other mods that must be loaded before this mod.</param>
        /// <param name="updateKeys">The namespaced mod IDs to query for updates (like <c>Nexus:541</c>).</param>
        [JsonConstructor]
        public Manifest(string uniqueId, string name, string author, string description, ISemanticVersion version, ISemanticVersion? minimumApiVersion, string? entryDll, IManifestContentPackFor? contentPackFor, IManifestDependency[]? dependencies, string[]? updateKeys)
        {
            this.UniqueID = this.NormalizeField(uniqueId);
            this.Name = this.NormalizeField(name, replaceSquareBrackets: true);
            this.Author = this.NormalizeField(author);
            this.Description = this.NormalizeField(description);
            this.Version = version;
            this.MinimumApiVersion = minimumApiVersion;
            this.EntryDll = this.NormalizeField(entryDll);
            this.ContentPackFor = contentPackFor;
            this.Dependencies = dependencies ?? Array.Empty<IManifestDependency>();
            this.UpdateKeys = updateKeys ?? Array.Empty<string>();
        }

        /// <summary>Try to validate a manifest's fields. Fails if any invalid field is found.</summary>
        /// <param name="error">The error message to display to the user.</param>
        /// <returns>Returns whether the manifest was validated successfully.</returns>
        public bool TryValidate(out string error)
        {
            // validate DLL / content pack fields
            bool hasDll = !string.IsNullOrWhiteSpace(this.EntryDll);
            bool isContentPack = this.ContentPackFor != null;

            // validate field presence
            if (!hasDll && !isContentPack)
            {
                error = $"manifest has no {nameof(IManifest.EntryDll)} or {nameof(IManifest.ContentPackFor)} field; must specify one.";
                return false;
            }
            if (hasDll && isContentPack)
            {
                error = $"manifest sets both {nameof(IManifest.EntryDll)} and {nameof(IManifest.ContentPackFor)}, which are mutually exclusive.";
                return false;
            }

            // validate DLL filename format
            if (hasDll && this.EntryDll!.Intersect(Path.GetInvalidFileNameChars()).Any())
            {
                error = $"manifest has invalid filename '{this.EntryDll}' for the EntryDLL field.";
                return false;
            }

            // validate content pack
            else if (isContentPack)
            {
                // invalid content pack ID
                if (string.IsNullOrWhiteSpace(this.ContentPackFor!.UniqueID))
                {
                    error = $"manifest declares {nameof(IManifest.ContentPackFor)} without its required {nameof(IManifestContentPackFor.UniqueID)} field.";
                    return false;
                }
            }

            // validate required fields
            {
                List<string> missingFields = new List<string>(3);

                if (string.IsNullOrWhiteSpace(this.Name))
                    missingFields.Add(nameof(IManifest.Name));
                if (this.Version == null || this.Version.ToString() == "0.0.0")
                    missingFields.Add(nameof(IManifest.Version));
                if (string.IsNullOrWhiteSpace(this.UniqueID))
                    missingFields.Add(nameof(IManifest.UniqueID));

                if (missingFields.Any())
                {
                    error = $"manifest is missing required fields ({string.Join(", ", missingFields)}).";
                    return false;
                }
            }

            // validate ID format
            if (!PathUtilities.IsSlug(this.UniqueID))
            {
                error = "manifest specifies an invalid ID (IDs must only contain letters, numbers, underscores, periods, or hyphens).";
                return false;
            }

            // validate dependencies
            foreach (IManifestDependency? dependency in this.Dependencies)
            {
                // null dependency
                if (dependency == null)
                {
                    error = $"manifest has a null entry under {nameof(IManifest.Dependencies)}.";
                    return false;
                }

                // missing ID
                if (string.IsNullOrWhiteSpace(dependency.UniqueID))
                {
                    error = $"manifest has a {nameof(IManifest.Dependencies)} entry with no {nameof(IManifestDependency.UniqueID)} field.";
                    return false;
                }

                // invalid ID
                if (!PathUtilities.IsSlug(dependency.UniqueID))
                {
                    error = $"manifest has a {nameof(IManifest.Dependencies)} entry with an invalid {nameof(IManifestDependency.UniqueID)} field (IDs must only contain letters, numbers, underscores, periods, or hyphens).";
                    return false;
                }
            }

            error = "";
            return true;
        }

        /// <summary>Override the update keys loaded from the mod info.</summary>
        /// <param name="updateKeys">The new update keys to set.</param>
        internal void OverrideUpdateKeys(params string[] updateKeys)
        {
            this.UpdateKeys = updateKeys;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize a manifest field to strip newlines, trim whitespace, and optionally strip square brackets.</summary>
        /// <param name="input">The input to strip.</param>
        /// <param name="replaceSquareBrackets">Whether to replace square brackets with round ones. This is used in the mod name to avoid breaking the log format.</param>
#if NET5_0_OR_GREATER
        [return: NotNullIfNotNull("input")]
#endif
        private string? NormalizeField(string? input, bool replaceSquareBrackets = false)
        {
            input = input?.Trim();

            if (!string.IsNullOrEmpty(input))
            {
                StringBuilder? builder = null;

                for (int i = 0; i < input.Length; i++)
                {
                    switch (input[i])
                    {
                        case '\r':
                        case '\n':
                            builder ??= new StringBuilder(input);
                            builder[i] = ' ';
                            break;

                        case '[' when replaceSquareBrackets:
                            builder ??= new StringBuilder(input);
                            builder[i] = '(';
                            break;

                        case ']' when replaceSquareBrackets:
                            builder ??= new StringBuilder(input);
                            builder[i] = ')';
                            break;
                    }
                }

                if (builder != null)
                    input = builder.ToString();
            }

            return input;
        }
    }
}
