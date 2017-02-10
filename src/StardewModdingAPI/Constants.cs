using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StardewModdingAPI.AssemblyRewriters;
using StardewModdingAPI.AssemblyRewriters.Rewriters.Crossplatform;
using StardewModdingAPI.AssemblyRewriters.Rewriters.SDV1_2;
using StardewValley;

namespace StardewModdingAPI
{
    /// <summary>Contains SMAPI's constants and assumptions.</summary>
    public static class Constants
    {
        /*********
        ** Properties
        *********/
        /// <summary>The directory name containing the current save's data (if a save is loaded).</summary>
        private static string RawSaveFolderName => Constants.PlayerNull ? string.Empty : Constants.GetSaveFolderName();

        /// <summary>The directory path containing the current save's data (if a save is loaded).</summary>
        private static string RawSavePath => Constants.PlayerNull ? string.Empty : Path.Combine(Constants.SavesPath, Constants.RawSaveFolderName);


        /*********
        ** Accessors
        *********/
        /// <summary>SMAPI's current semantic version.</summary>
        public static ISemanticVersion ApiVersion => new SemanticVersion(1, 8, 0, null);

        /// <summary>The minimum supported version of Stardew Valley.</summary>
        public const string MinimumGameVersion = "1.2";

        /// <summary>The GitHub repository to check for updates.</summary>
        public const string GitHubRepository = "Pathoschild/SMAPI";

        /// <summary>The directory path containing Stardew Valley's app data.</summary>
        public static string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley");

        /// <summary>The directory path where all saves are stored.</summary>
        public static string SavesPath => Path.Combine(Constants.DataPath, "Saves");

        /// <summary>Whether the directory containing the current save's data exists on disk.</summary>
        public static bool CurrentSavePathExists => Directory.Exists(Constants.RawSavePath);

        /// <summary>The directory name containing the current save's data (if a save is loaded and the directory exists).</summary>
        public static string SaveFolderName => Constants.CurrentSavePathExists ? Constants.RawSaveFolderName : "";

        /// <summary>The directory path containing the current save's data (if a save is loaded and the directory exists).</summary>
        public static string CurrentSavePath => Constants.CurrentSavePathExists ? Constants.RawSavePath : "";

        /// <summary>Whether a player save has been loaded.</summary>
        public static bool PlayerNull => !Game1.hasLoadedGame || Game1.player == null || string.IsNullOrEmpty(Game1.player.name);

        /// <summary>The path to the current assembly being executing.</summary>
        public static string ExecutionPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>The title of the SMAPI console window.</summary>
        public static string ConsoleTitle => $"Stardew Modding API Console - Version {Constants.ApiVersion} - Mods Loaded: {Program.ModsLoaded}";

        /// <summary>The directory path in which error logs should be stored.</summary>
        public static string LogDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "ErrorLogs");

        /// <summary>The file path to the log where the latest output should be saved.</summary>
        public static string LogPath => Path.Combine(Constants.LogDir, "SMAPI-latest.txt");

        /// <summary>The file path for the SMAPI configuration file.</summary>
        internal static string ApiConfigPath => Path.Combine(Constants.ExecutionPath, $"{typeof(Program).Assembly.GetName().Name}.config.json");

        /// <summary>The file path for the SMAPI data file containing metadata about known mods.</summary>
        internal static string ApiModMetadataPath => Path.Combine(Constants.ExecutionPath, $"{typeof(Program).Assembly.GetName().Name}.data.json");


        /*********
        ** Protected methods
        *********/
        /// <summary>Get metadata for mapping assemblies to the current platform.</summary>
        /// <param name="targetPlatform">The target game platform.</param>
        internal static PlatformAssemblyMap GetAssemblyMap(Platform targetPlatform)
        {
            // get assembly changes needed for platform
            string[] removeAssemblyReferences;
            Assembly[] targetAssemblies;
            switch (targetPlatform)
            {
                case Platform.Mono:
                    removeAssemblyReferences = new[]
                    {
                        "Stardew Valley",
                        "Microsoft.Xna.Framework",
                        "Microsoft.Xna.Framework.Game",
                        "Microsoft.Xna.Framework.Graphics"
                    };
                    targetAssemblies = new[]
                    {
                        typeof(StardewValley.Game1).Assembly,
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

        /// <summary>Get rewriters which fix incompatible CIL instructions in mod assemblies.</summary>
        internal static IEnumerable<IInstructionRewriter> GetRewriters()
        {
            return new IInstructionRewriter[]
            {
                // crossplatform
                new SpriteBatch_MethodRewriter(),

                // Stardew Valley 1.2
                new Game1_ActiveClickableMenu_FieldRewriter()
            };
        }

        /// <summary>Get the name of a save directory for the current player.</summary>
        private static string GetSaveFolderName()
        {
            string prefix = new string(Game1.player.name.Where(char.IsLetterOrDigit).ToArray());
            return $"{prefix}_{Game1.uniqueIDForThisGame}";
        }
    }
}
