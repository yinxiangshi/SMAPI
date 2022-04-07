using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StardewModdingAPI.Toolkit.Utilities
{
    /// <summary>Provides utilities for normalizing file paths.</summary>
    public static class PathUtilities
    {
        /*********
        ** Fields
        *********/
        /// <summary>The root prefix for a Windows UNC path.</summary>
        private const string WindowsUncRoot = @"\\";


        /*********
        ** Accessors
        *********/
        /// <summary>The possible directory separator characters in a file path.</summary>
        public static readonly char[] PossiblePathSeparators = new[] { '/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }.Distinct().ToArray();

        /// <summary>The preferred directory separator character in a file path.</summary>
        public static readonly char PreferredPathSeparator = Path.DirectorySeparatorChar;

        /// <summary>The preferred directory separator character in an asset key.</summary>
        public static readonly char PreferredAssetSeparator = '/';


        /*********
        ** Public methods
        *********/
        /// <summary>Get the segments from a path (e.g. <c>/usr/bin/example</c> => <c>usr</c>, <c>bin</c>, and <c>example</c>).</summary>
        /// <param name="path">The path to split.</param>
        /// <param name="limit">The number of segments to match. Any additional segments will be merged into the last returned part.</param>
        [Pure]
        public static string[] GetSegments(string? path, int? limit = null)
        {
            if (path == null)
                return Array.Empty<string>();

            return limit.HasValue
                ? path.Split(PathUtilities.PossiblePathSeparators, limit.Value, StringSplitOptions.RemoveEmptyEntries)
                : path.Split(PathUtilities.PossiblePathSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Normalize an asset name to match how MonoGame's content APIs would normalize and cache it.</summary>
        /// <param name="assetName">The asset name to normalize.</param>
        [Pure]
        [return: NotNullIfNotNull("assetName")]
        public static string? NormalizeAssetName(string? assetName)
        {
            assetName = assetName?.Trim();
            if (string.IsNullOrEmpty(assetName))
                return assetName;

            return string.Join(PathUtilities.PreferredAssetSeparator.ToString(), PathUtilities.GetSegments(assetName)); // based on MonoGame's ContentManager.Load<T> logic
        }

        /// <summary>Normalize separators in a file path for the current platform.</summary>
        /// <param name="path">The file path to normalize.</param>
        /// <remarks>This should only be used for file paths. For asset names, use <see cref="NormalizeAssetName"/> instead.</remarks>
        [Pure]
        [return: NotNullIfNotNull("path")]
        public static string? NormalizePath(string? path)
        {
            path = path?.Trim();
            if (string.IsNullOrEmpty(path))
                return path;

            // get basic path format (e.g. /some/asset\\path/ => some\asset\path)
            string[] segments = PathUtilities.GetSegments(path);
            string newPath = string.Join(PathUtilities.PreferredPathSeparator.ToString(), segments);

            // keep root prefix
            bool hasRoot = false;
            if (path.StartsWith(PathUtilities.WindowsUncRoot))
            {
                newPath = PathUtilities.WindowsUncRoot + newPath;
                hasRoot = true;
            }
            else if (PathUtilities.PossiblePathSeparators.Contains(path[0]))
            {
                newPath = PathUtilities.PreferredPathSeparator + newPath;
                hasRoot = true;
            }

            // keep trailing separator
            if ((!hasRoot || segments.Any()) && PathUtilities.PossiblePathSeparators.Contains(path[^1]))
                newPath += PathUtilities.PreferredPathSeparator;

            return newPath;
        }

        /// <summary>Get a directory or file path relative to a given source path. If no relative path is possible (e.g. the paths are on different drives), an absolute path is returned.</summary>
        /// <param name="sourceDir">The source folder path.</param>
        /// <param name="targetPath">The target folder or file path.</param>
        [Pure]
        public static string GetRelativePath(string sourceDir, string targetPath)
        {
            return Path.GetRelativePath(sourceDir, targetPath);
        }

        /// <summary>Get whether a path is relative and doesn't try to climb out of its containing folder (e.g. doesn't contain <c>../</c>).</summary>
        /// <param name="path">The path to check.</param>
        [Pure]
        public static bool IsSafeRelativePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return true;

            return
                !Path.IsPathRooted(path)
                && PathUtilities.GetSegments(path).All(segment => segment.Trim() != "..");
        }

        /// <summary>Get whether a string is a valid 'slug', containing only basic characters that are safe in all contexts (e.g. filenames, URLs, etc).</summary>
        /// <param name="str">The string to check.</param>
        [Pure]
        public static bool IsSlug(string? str)
        {
            return
                string.IsNullOrWhiteSpace(str)
                || !Regex.IsMatch(str, "[^a-z0-9_.-]", RegexOptions.IgnoreCase);
        }
    }
}
