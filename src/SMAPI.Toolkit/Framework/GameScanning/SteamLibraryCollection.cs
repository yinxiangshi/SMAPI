#if SMAPI_FOR_WINDOWS
using System.Collections.Generic;

namespace StardewModdingAPI.Toolkit.Framework.GameScanning
{
#pragma warning disable IDE1006 // Model requires lowercase naming.
#pragma warning disable CS8618 // Required for model.
    /// <summary>Model for Steam's libraryfolders.vdf.</summary>
    public class SteamLibraryCollection
    {
        /// <summary>Each entry identifies a different location that part of the Steam games library is installed to.</summary>
        public LibraryFolders<int, LibraryFolder> libraryfolders { get; set; }
    }

    /// <summary>A collection of LibraryFolders. Like a dictionary, but has contentstatsid used as an index also.</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
#pragma warning disable CS8714 // Required for model.
    public class LibraryFolders<TKey, TValue> : Dictionary<TKey, TValue>
#pragma warning restore CS8714
    {
        /// <summary>Index of the library, starting from "0".</summary>
        public string contentstatsid { get; set; }
    }

    /// <summary>A Steam library folder, containing information on the location and size of games installed there.</summary>
    public class LibraryFolder
    {
        /// <summary>The escaped path to this Steam library folder. There will be a steam.exe here, but this may not be the one the player generally launches.</summary>
        public string path { get; set; }
        /// <summary>Label for the library, or ""</summary>
        public string label { get; set; }
        /// <summary>~19-digit identifier.</summary>
        public string contentid { get; set; }
        /// <summary>Size of the library in bytes. May show 0 when size is non-zero.</summary>
        public string totalsize { get; set; }
        /// <summary>Used for downloads.</summary>
        public string update_clean_bytes_tally { get; set; }
        /// <summary>Normally "0".</summary>
        public string time_last_update_corruption { get; set; }
        /// <summary>List of Steam app IDs, and their current size in bytes.</summary>
        public Dictionary<string, string> apps { get; set; }
    }
#pragma warning restore IDE1006
#pragma warning restore CS8618
}
#endif
