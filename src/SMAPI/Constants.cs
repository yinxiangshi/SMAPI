using System;
using System.IO;
using System.Linq;
using System.Reflection;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.ModLoading;
using StardewModdingAPI.Internal;
using StardewValley;

namespace StardewModdingAPI
{
    /// <summary>Contains SMAPI's constants and assumptions.</summary>
    public static class Constants
    {
        /*********
        ** Properties
        *********/
        /// <summary>The directory path containing the current save's data (if a save is loaded).</summary>
        private static string RawSavePath => Context.IsSaveLoaded ? Path.Combine(Constants.SavesPath, Constants.GetSaveFolderName()) : null;

        /// <summary>Whether the directory containing the current save's data exists on disk.</summary>
        private static bool SavePathReady => Context.IsSaveLoaded && Directory.Exists(Constants.RawSavePath);


        /*********
        ** Accessors
        *********/
        /****
        ** Public
        ****/
        /// <summary>SMAPI's current semantic version.</summary>
        public static ISemanticVersion ApiVersion { get; } = new Toolkit.SemanticVersion("2.9.3");

        /// <summary>The minimum supported version of Stardew Valley.</summary>
        public static ISemanticVersion MinimumGameVersion { get; } = new GameVersion("1.3.32");

        /// <summary>The maximum supported version of Stardew Valley.</summary>
        public static ISemanticVersion MaximumGameVersion { get; } = null;

        /// <summary>The target game platform.</summary>
        public static GamePlatform TargetPlatform => (GamePlatform)Constants.Platform;

        /// <summary>The path to the game folder.</summary>
        public static string ExecutionPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>The directory path containing Stardew Valley's app data.</summary>
        public static string DataPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley");

        /// <summary>The directory path in which error logs should be stored.</summary>
        public static string LogDir { get; } = Path.Combine(Constants.DataPath, "ErrorLogs");

        /// <summary>The directory path where all saves are stored.</summary>
        public static string SavesPath { get; } = Path.Combine(Constants.DataPath, "Saves");

        /// <summary>The directory name containing the current save's data (if a save is loaded and the directory exists).</summary>
        public static string SaveFolderName => Context.IsSaveLoaded ? Constants.GetSaveFolderName() : "";

        /// <summary>The directory path containing the current save's data (if a save is loaded and the directory exists).</summary>
        public static string CurrentSavePath => Constants.SavePathReady ? Path.Combine(Constants.SavesPath, Constants.GetSaveFolderName()) : "";

        /****
        ** Internal
        ****/
        /// <summary>The URL of the SMAPI home page.</summary>
        internal const string HomePageUrl = "https://smapi.io";

        /// <summary>The absolute path to the folder containing SMAPI's internal files.</summary>
        internal static readonly string InternalFilesPath = Program.DllSearchPath;

        /// <summary>The file path for the SMAPI configuration file.</summary>
        internal static string ApiConfigPath => Path.Combine(Constants.InternalFilesPath, "StardewModdingAPI.config.json");

        /// <summary>The file path for the SMAPI metadata file.</summary>
        internal static string ApiMetadataPath => Path.Combine(Constants.InternalFilesPath, "StardewModdingAPI.metadata.json");

        /// <summary>The filename prefix used for all SMAPI logs.</summary>
        internal static string LogNamePrefix { get; } = "SMAPI-";

        /// <summary>The filename for SMAPI's main log, excluding the <see cref="LogExtension"/>.</summary>
        internal static string LogFilename { get; } = $"{Constants.LogNamePrefix}latest";

        /// <summary>The filename extension for SMAPI log files.</summary>
        internal static string LogExtension { get; } = "txt";

        /// <summary>The file path for the log containing the previous fatal crash, if any.</summary>
        internal static string FatalCrashLog => Path.Combine(Constants.LogDir, "SMAPI-crash.txt");

        /// <summary>The file path which stores a fatal crash message for the next run.</summary>
        internal static string FatalCrashMarker => Path.Combine(Constants.InternalFilesPath, "StardewModdingAPI.crash.marker");

        /// <summary>The file path which stores the detected update version for the next run.</summary>
        internal static string UpdateMarker => Path.Combine(Constants.InternalFilesPath, "StardewModdingAPI.update.marker");

        /// <summary>The default full path to search for mods.</summary>
        internal static string DefaultModsPath { get; } = Path.Combine(Constants.ExecutionPath, "Mods");

        /// <summary>The actual full path to search for mods.</summary>
        internal static string ModsPath { get; set; }

        /// <summary>The game's current semantic version.</summary>
        internal static ISemanticVersion GameVersion { get; } = new GameVersion(Constants.GetGameVersion());

        /// <summary>The target game platform.</summary>
        internal static Platform Platform { get; } = EnvironmentUtility.DetectPlatform();

        /// <summary>The game's assembly name.</summary>
        internal static string GameAssemblyName => Constants.Platform == Platform.Windows ? "Stardew Valley" : "StardewValley";


        /*********
        ** Internal methods
        *********/
        /// <summary>Get the SMAPI version to recommend for an older game version, if any.</summary>
        /// <param name="version">The game version to search.</param>
        /// <returns>Returns the compatible SMAPI version, or <c>null</c> if none was found.</returns>
        internal static ISemanticVersion GetCompatibleApiVersion(ISemanticVersion version)
        {
            switch (version.ToString())
            {
                case "1.3.28":
                    return new SemanticVersion(2, 7, 0);

                case "1.2.30":
                case "1.2.31":
                case "1.2.32":
                case "1.2.33":
                    return new SemanticVersion(2, 5, 5);
            }

            return null;
        }

        /// <summary>Get metadata for mapping assemblies to the current platform.</summary>
        /// <param name="targetPlatform">The target game platform.</param>
        internal static PlatformAssemblyMap GetAssemblyMap(Platform targetPlatform)
        {
            // get assembly changes needed for platform
            string[] removeAssemblyReferences;
            Assembly[] targetAssemblies;
            switch (targetPlatform)
            {
                case Platform.Linux:
                case Platform.Mac:
                    removeAssemblyReferences = new[]
                    {
                        "Netcode",
                        "Stardew Valley",
                        "Microsoft.Xna.Framework",
                        "Microsoft.Xna.Framework.Game",
                        "Microsoft.Xna.Framework.Graphics",
                        "Microsoft.Xna.Framework.Xact"
                    };
                    targetAssemblies = new[]
                    {
                        typeof(StardewValley.Game1).Assembly, // note: includes Netcode types on Linux/Mac
                        typeof(Microsoft.Xna.Framework.Vector2).Assembly
                    };
                    break;

                case Platform.Windows:
                    removeAssemblyReferences = new[]
                    {
                        "StardewValley",
                        "MonoGame.Framework"
                    };
                    targetAssemblies = new[]
                    {
                        typeof(Netcode.NetBool).Assembly,
                        typeof(StardewValley.Game1).Assembly,
                        typeof(Microsoft.Xna.Framework.Vector2).Assembly,
                        typeof(Microsoft.Xna.Framework.Game).Assembly,
                        typeof(Microsoft.Xna.Framework.Graphics.SpriteBatch).Assembly
                    };
                    break;

                default:
                    throw new InvalidOperationException($"Unknown target platform '{targetPlatform}'.");
            }

            return new PlatformAssemblyMap(targetPlatform, removeAssemblyReferences, targetAssemblies);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the name of a save directory for the current player.</summary>
        private static string GetSaveFolderName()
        {
            string prefix = new string(Game1.player.Name.Where(char.IsLetterOrDigit).ToArray());
            return $"{prefix}_{Game1.uniqueIDForThisGame}";
        }

        /// <summary>Get the game's current version string.</summary>
        private static string GetGameVersion()
        {
            // we need reflection because it's a constant, so SMAPI's references to it are inlined at compile-time
            FieldInfo field = typeof(Game1).GetField(nameof(Game1.version), BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                throw new InvalidOperationException($"The {nameof(Game1)}.{nameof(Game1.version)} field could not be found.");
            return (string)field.GetValue(null);
        }
    }
}
