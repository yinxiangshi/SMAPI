using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StardewModdingAPI.AssemblyRewriters;
using StardewModdingAPI.AssemblyRewriters.Finders;
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
        /// <summary>The directory path containing the current save's data (if a save is loaded).</summary>
        private static string RawSavePath => Constants.IsSaveLoaded ? Path.Combine(Constants.SavesPath, Constants.GetSaveFolderName()) : null;

        /// <summary>Whether the directory containing the current save's data exists on disk.</summary>
        private static bool SavePathReady => Constants.IsSaveLoaded && Directory.Exists(Constants.RawSavePath);


        /*********
        ** Accessors
        *********/
        /****
        ** Public
        ****/
        /// <summary>SMAPI's current semantic version.</summary>
        public static ISemanticVersion ApiVersion { get; } = new SemanticVersion(1, 8, 0);

        /// <summary>The minimum supported version of Stardew Valley.</summary>
        public static ISemanticVersion MinimumGameVersion { get; } = new SemanticVersion("1.2.13");

        /// <summary>The path to the game folder.</summary>
        public static string ExecutionPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>The directory path containing Stardew Valley's app data.</summary>
        public static string DataPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley");

        /// <summary>The directory path in which error logs should be stored.</summary>
        public static string LogDir { get; } = Path.Combine(Constants.DataPath, "ErrorLogs");

        /// <summary>The directory path where all saves are stored.</summary>
        public static string SavesPath { get; } = Path.Combine(Constants.DataPath, "Saves");

        /// <summary>The directory name containing the current save's data (if a save is loaded and the directory exists).</summary>
        public static string SaveFolderName => Constants.SavePathReady ? Constants.GetSaveFolderName() : "";

        /// <summary>The directory path containing the current save's data (if a save is loaded and the directory exists).</summary>
        public static string CurrentSavePath => Constants.SavePathReady ? Path.Combine(Constants.SavesPath, Constants.GetSaveFolderName()) : "";

        /****
        ** Internal
        ****/
        /// <summary>The GitHub repository to check for updates.</summary>
        internal const string GitHubRepository = "Pathoschild/SMAPI";

        /// <summary>The file path for the SMAPI configuration file.</summary>
        internal static string ApiConfigPath => Path.Combine(Constants.ExecutionPath, $"{typeof(Program).Assembly.GetName().Name}.config.json");

        /// <summary>The file path to the log where the latest output should be saved.</summary>
        internal static string LogPath => Path.Combine(Constants.LogDir, "SMAPI-latest.txt");

        /// <summary>The full path to the folder containing mods.</summary>
        internal static string ModPath { get; } = Path.Combine(Constants.ExecutionPath, "Mods");

        /// <summary>Whether a player save has been loaded.</summary>
        internal static bool IsSaveLoaded => Game1.hasLoadedGame && !string.IsNullOrEmpty(Game1.player.name);

        /// <summary>The game's current semantic version.</summary>
        internal static ISemanticVersion GameVersion { get; } = Constants.GetGameVersion();

        /// <summary>The game's current version as it should be displayed to players.</summary>
        internal static ISemanticVersion GameDisplayVersion { get; } = Constants.GetGameDisplayVersion(Constants.GameVersion);

        /// <summary>The target game platform.</summary>
        internal static Platform TargetPlatform { get; } =
#if SMAPI_FOR_WINDOWS
        Platform.Windows;
#else
        Platform.Mono;
#endif


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

        /// <summary>Get finders which match incompatible CIL instructions in mod assemblies.</summary>
        internal static IEnumerable<IInstructionFinder> GetIncompatibilityFinders()
        {
            return new IInstructionFinder[]
            {
                // changes in Stardew Valley 1.2 (that don't have rewriters)
                new GenericFieldFinder("StardewValley.Game1", "borderFont", isStatic: true),
                new GenericFieldFinder("StardewValley.Game1", "smoothFont", isStatic: true),
                new GenericFieldFinder("StardewValley.Item", "set_Name", isStatic: false),

                // APIs removed in SMAPI 1.9
                new GenericTypeFinder("StardewModdingAPI.Entities.SPlayer"),
                new GenericTypeFinder("StardewModdingAPI.Extensions"),
                new GenericTypeFinder("StardewModdingAPI.Inheritance.ItemStackChange"),
                new GenericTypeFinder("StardewModdingAPI.Inheritance.SGame"),
                new GenericTypeFinder("StardewModdingAPI.Inheritance.SObject"),
                new GenericTypeFinder("StardewModdingAPI.LogWriter"),
                new GenericTypeFinder("StardewModdingAPI.Manifest"),
                new GenericTypeFinder("StardewModdingAPI.Version"),
                new GenericEventFinder("StardewModdingAPI.Events.GraphicsEvents", "DrawDebug"),
                new GenericEventFinder("StardewModdingAPI.Events.GraphicsEvents", "DrawTick"),
                new GenericEventFinder("StardewModdingAPI.Events.GraphicsEvents", "OnPostRenderHudEventNoCheck"),
                new GenericEventFinder("StardewModdingAPI.Events.GraphicsEvents", "OnPostRenderGuiEventNoCheck"),
                new GenericEventFinder("StardewModdingAPI.Events.GraphicsEvents", "OnPreRenderHudEventNoCheck"),
                new GenericEventFinder("StardewModdingAPI.Events.GraphicsEvents", "OnPreRenderGuiEventNoCheck"),
            };
        }

        /// <summary>Get rewriters which fix incompatible CIL instructions in mod assemblies.</summary>
        internal static IEnumerable<IInstructionRewriter> GetRewriters()
        {
            return new IInstructionRewriter[]
            {
                // crossplatform
                new SpriteBatch_MethodRewriter(),

                // Stardew Valley 1.2
                new Game1_ActiveClickableMenu_FieldRewriter(),
                new Game1_GameMode_FieldRewriter(),
                new Game1_Player_FieldRewriter()
            };
        }

        /// <summary>Get the name of a save directory for the current player.</summary>
        private static string GetSaveFolderName()
        {
            string prefix = new string(Game1.player.name.Where(char.IsLetterOrDigit).ToArray());
            return $"{prefix}_{Game1.uniqueIDForThisGame}";
        }

        /// <summary>Get the game's current semantic version.</summary>
        private static ISemanticVersion GetGameVersion()
        {
            // get raw version
            // we need reflection because it's a constant, so SMAPI's references to it are inlined at compile-time
            FieldInfo field = typeof(Game1).GetField(nameof(Game1.version), BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                throw new InvalidOperationException($"The {nameof(Game1)}.{nameof(Game1.version)} field could not be found.");
            string version = (string)field.GetValue(null);

            // get semantic version
            if (version == "1.11")
                version = "1.1.1"; // The 1.1 patch was released as 1.11, which means it's out of order for semantic version checks
            return new SemanticVersion(version);
        }

        /// <summary>Get game current version as it should be displayed to players.</summary>
        /// <param name="version">The semantic game version.</param>
        private static ISemanticVersion GetGameDisplayVersion(ISemanticVersion version)
        {
            switch (version.ToString())
            {
                case "1.1.1":
                    return new SemanticVersion(1, 11, 0); // The 1.1 patch was released as 1.11
                default:
                    return version;
            }
        }
    }
}
