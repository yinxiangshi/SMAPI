using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace StardewModdingAPI.Toolkit.Utilities
{
    /// <summary>Provides utilities for normalising file paths.</summary>
    public static class PathUtilities
    {
        /*********
        ** Properties
        *********/
        /// <summary>The possible directory separator characters in a file path.</summary>
        private static readonly char[] PossiblePathSeparators = new[] { '/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }.Distinct().ToArray();

        /// <summary>The preferred directory separator chaeacter in an asset key.</summary>
        private static readonly string PreferredPathSeparator = Path.DirectorySeparatorChar.ToString();


        /*********
        ** Public methods
        *********/
        /// <summary>Get the segments from a path (e.g. <c>/usr/bin/boop</c> => <c>usr</c>, <c>bin</c>, and <c>boop</c>).</summary>
        /// <param name="path">The path to split.</param>
        /// <param name="limit">The number of segments to match. Any additional segments will be merged into the last returned part.</param>
        public static string[] GetSegments(string path, int? limit = null)
        {
            return limit.HasValue
                ? path.Split(PathUtilities.PossiblePathSeparators, limit.Value, StringSplitOptions.RemoveEmptyEntries)
                : path.Split(PathUtilities.PossiblePathSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Normalise path separators in a file path.</summary>
        /// <param name="path">The file path to normalise.</param>
        [Pure]
        public static string NormalisePathSeparators(string path)
        {
            string[] parts = PathUtilities.GetSegments(path);
            string normalised = string.Join(PathUtilities.PreferredPathSeparator, parts);
            if (path.StartsWith(PathUtilities.PreferredPathSeparator))
                normalised = PathUtilities.PreferredPathSeparator + normalised; // keep root slash
            return normalised;
        }

        /// <summary>Get a directory or file path relative to a given source path.</summary>
        /// <param name="sourceDir">The source folder path.</param>
        /// <param name="targetPath">The target folder or file path.</param>
        [Pure]
        public static string GetRelativePath(string sourceDir, string targetPath)
        {
            // convert to URIs
            Uri from = new Uri(sourceDir.TrimEnd(PathUtilities.PossiblePathSeparators) + "/");
            Uri to = new Uri(targetPath.TrimEnd(PathUtilities.PossiblePathSeparators) + "/");
            if (from.Scheme != to.Scheme)
                throw new InvalidOperationException($"Can't get path for '{targetPath}' relative to '{sourceDir}'.");

            // get relative path
            string relative = PathUtilities.NormalisePathSeparators(Uri.UnescapeDataString(from.MakeRelativeUri(to).ToString()));
            if (relative == "")
                relative = "./";
            return relative;
        }
    }
}
