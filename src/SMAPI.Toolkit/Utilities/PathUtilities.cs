using System;
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

        /// <summary>The preferred directory separator character in an asset key.</summary>
        public static readonly string PreferredPathSeparator = Path.DirectorySeparatorChar.ToString();


        /*********
        ** Public methods
        *********/
        /// <summary>Get the segments from a path (e.g. <c>/usr/bin/example</c> => <c>usr</c>, <c>bin</c>, and <c>example</c>).</summary>
        /// <param name="path">The path to split.</param>
        /// <param name="limit">The number of segments to match. Any additional segments will be merged into the last returned part.</param>
        [Pure]
        public static string[] GetSegments(string path, int? limit = null)
        {
            return limit.HasValue
                ? path.Split(PathUtilities.PossiblePathSeparators, limit.Value, StringSplitOptions.RemoveEmptyEntries)
                : path.Split(PathUtilities.PossiblePathSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Normalize path separators in a file path.</summary>
        /// <param name="path">The file path to normalize.</param>
        [Pure]
        public static string NormalizePathSeparators(string path)
        {
            string normalized = string.Join(PathUtilities.PreferredPathSeparator, PathUtilities.GetSegments(path));

            // keep root
#if SMAPI_FOR_WINDOWS
            if (path.StartsWith(PathUtilities.WindowsUncRoot))
                normalized = PathUtilities.WindowsUncRoot + normalized;
            else
#endif
            if (path.StartsWith(PathUtilities.PreferredPathSeparator) || path.StartsWith(PathUtilities.WindowsUncRoot))
                normalized = PathUtilities.PreferredPathSeparator + normalized;

            return normalized;
        }

        /// <summary>Get a directory or file path relative to a given source path. If no relative path is possible (e.g. the paths are on different drives), an absolute path is returned.</summary>
        /// <param name="sourceDir">The source folder path.</param>
        /// <param name="targetPath">The target folder or file path.</param>
        /// <remarks>
        ///
        /// NOTE: this is a heuristic implementation that works in the cases SMAPI needs it for, but it doesn't handle all edge cases (e.g. case-sensitivity on Linux, or traversing between UNC paths on Windows). This should be replaced with the more comprehensive <c>Path.GetRelativePath</c> if the game ever migrates to .NET Core.
        ///
        /// </remarks>
        [Pure]
        public static string GetRelativePath(string sourceDir, string targetPath)
        {
            // convert to URIs
            Uri from = new Uri(sourceDir.TrimEnd(PathUtilities.PossiblePathSeparators) + "/");
            Uri to = new Uri(targetPath.TrimEnd(PathUtilities.PossiblePathSeparators) + "/");
            if (from.Scheme != to.Scheme)
                throw new InvalidOperationException($"Can't get path for '{targetPath}' relative to '{sourceDir}'.");

            // get relative path
            string relative = PathUtilities.NormalizePathSeparators(Uri.UnescapeDataString(from.MakeRelativeUri(to).ToString()));

            // set empty path to './'
            if (relative == "")
                relative = "./";

            // fix root
            if (relative.StartsWith("file:") && !targetPath.Contains("file:"))
            {
                relative = relative.Substring("file:".Length);
                if (targetPath.StartsWith(PathUtilities.WindowsUncRoot) && !relative.StartsWith(PathUtilities.WindowsUncRoot))
                    relative = PathUtilities.WindowsUncRoot + relative.TrimStart('\\');
            }

            return relative;
        }

        /// <summary>Get whether a path is relative and doesn't try to climb out of its containing folder (e.g. doesn't contain <c>../</c>).</summary>
        /// <param name="path">The path to check.</param>
        [Pure]
        public static bool IsSafeRelativePath(string path)
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
        public static bool IsSlug(string str)
        {
            return !Regex.IsMatch(str, "[^a-z0-9_.-]", RegexOptions.IgnoreCase);
        }
    }
}
