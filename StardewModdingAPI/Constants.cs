using System;
using System.IO;
using System.Reflection;

namespace StardewModdingAPI
{
    /// <summary>
    /// Static class containing readonly values.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Stardew Valley's local app data location.
        /// %LocalAppData%//StardewValley
        /// </summary>
        public static string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley");

        /// <summary>
        /// Execution path to execute the code.
        /// </summary>
        public static string ExecutionPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Title for the API console
        /// </summary>
        public static string ConsoleTitle => string.Format("Stardew Modding API Console - Version {0}", VersionString);

        /// <summary>
        /// Path for log files to be output to.
        /// %LocalAppData%//StardewValley//ErrorLogs
        /// </summary>
        public static string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "ErrorLogs");

        public const int MajorVersion = 0;

        public const int MinorVersion = 38;

        public const int PatchVersion = 6;

        public const string Build = "Alpha";

        public static string VersionString => string.Format("{0}.{1}.{2} {3}", MajorVersion, MinorVersion, PatchVersion, Build);
    }
}
