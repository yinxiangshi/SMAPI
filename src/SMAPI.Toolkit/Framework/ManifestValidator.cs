using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingAPI.Toolkit.Framework
{
    /// <summary>Validates manifest fields.</summary>
    public static class ManifestValidator
    {
        /// <summary>Try to validate a manifest's fields. Fails if any invalid field is found.</summary>
        /// <param name="manifest">The manifest to validate.</param>
        /// <param name="error">The error message to display to the user.</param>
        /// <returns>Returns whether the manifest was validated successfully.</returns>
        public static bool TryValidate(IManifest manifest, out string error)
        {
            // validate DLL / content pack fields
            bool hasDll = !string.IsNullOrWhiteSpace(manifest.EntryDll);
            bool isContentPack = manifest.ContentPackFor != null;

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
            if (hasDll && manifest.EntryDll!.Intersect(Path.GetInvalidFileNameChars()).Any())
            {
                error = $"manifest has invalid filename '{manifest.EntryDll}' for the EntryDLL field.";
                return false;
            }

            // validate content pack ID
            else if (isContentPack && string.IsNullOrWhiteSpace(manifest.ContentPackFor!.UniqueID))
            {
                error = $"manifest declares {nameof(IManifest.ContentPackFor)} without its required {nameof(IManifestContentPackFor.UniqueID)} field.";
                return false;
            }

            // validate required fields
            {
                List<string> missingFields = new List<string>(3);

                if (string.IsNullOrWhiteSpace(manifest.Name))
                    missingFields.Add(nameof(IManifest.Name));
                if (manifest.Version == null || manifest.Version.ToString() == "0.0.0")
                    missingFields.Add(nameof(IManifest.Version));
                if (string.IsNullOrWhiteSpace(manifest.UniqueID))
                    missingFields.Add(nameof(IManifest.UniqueID));

                if (missingFields.Any())
                {
                    error = $"manifest is missing required fields ({string.Join(", ", missingFields)}).";
                    return false;
                }
            }

            // validate ID format
            if (!PathUtilities.IsSlug(manifest.UniqueID))
            {
                error = "manifest specifies an invalid ID (IDs must only contain letters, numbers, underscores, periods, or hyphens).";
                return false;
            }

            // validate dependencies
            foreach (IManifestDependency? dependency in manifest.Dependencies)
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
    }
}
