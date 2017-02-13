using System;
using System.IO;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Framework.Serialisation;

namespace StardewModdingAPI.Framework
{
    /// <summary>Provides simplified APIs for writing mods.</summary>
    internal class ModHelper : IModHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
        private readonly JsonHelper JsonHelper;


        /*********
        ** Accessors
        *********/
        /// <summary>The mod directory path.</summary>
        public string DirectoryPath { get; }

        /// <summary>Simplifies access to private game code.</summary>
        public IReflectionHelper Reflection { get; } = new ReflectionHelper();

        /// <summary>Metadata about loaded mods.</summary>
        public IModRegistry ModRegistry { get; }

        /// <summary>An API for managing console commands.</summary>
        public ICommandHelper ConsoleCommands { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modName">The friendly mod name.</param>
        /// <param name="modDirectory">The mod directory path.</param>
        /// <param name="jsonHelper">Encapsulate SMAPI's JSON parsing.</param>
        /// <param name="modRegistry">Metadata about loaded mods.</param>
        /// <param name="commandManager">Manages console commands.</param>
        /// <exception cref="ArgumentNullException">An argument is null or empty.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="modDirectory"/> path does not exist on disk.</exception>
        public ModHelper(string modName, string modDirectory, JsonHelper jsonHelper, IModRegistry modRegistry, CommandManager commandManager)
        {
            // validate
            if (string.IsNullOrWhiteSpace(modDirectory))
                throw new ArgumentNullException(nameof(modDirectory));
            if (jsonHelper == null)
                throw new ArgumentNullException(nameof(jsonHelper));
            if (modRegistry == null)
                throw new ArgumentNullException(nameof(modRegistry));
            if (!Directory.Exists(modDirectory))
                throw new InvalidOperationException("The specified mod directory does not exist.");

            // initialise
            this.JsonHelper = jsonHelper;
            this.DirectoryPath = modDirectory;
            this.ModRegistry = modRegistry;
            this.ConsoleCommands = new CommandHelper(modName, commandManager);
        }

        /****
        ** Mod config file
        ****/
        /// <summary>Read the mod's configuration file (and create it if needed).</summary>
        /// <typeparam name="TConfig">The config class type. This should be a plain class that has public properties for the settings you want. These can be complex types.</typeparam>
        public TConfig ReadConfig<TConfig>()
            where TConfig : class, new()
        {
            var config = this.ReadJsonFile<TConfig>("config.json") ?? new TConfig();
            this.WriteConfig(config); // create file or fill in missing fields
            return config;
        }

        /// <summary>Save to the mod's configuration file.</summary>
        /// <typeparam name="TConfig">The config class type.</typeparam>
        /// <param name="config">The config settings to save.</param>
        public void WriteConfig<TConfig>(TConfig config)
            where TConfig : class, new()
        {
            this.WriteJsonFile("config.json", config);
        }

        /****
        ** Generic JSON files
        ****/
        /// <summary>Read a JSON file.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="path">The file path relative to the mod directory.</param>
        /// <returns>Returns the deserialised model, or <c>null</c> if the file doesn't exist or is empty.</returns>
        public TModel ReadJsonFile<TModel>(string path)
            where TModel : class
        {
            path = Path.Combine(this.DirectoryPath, path);
            return this.JsonHelper.ReadJsonFile<TModel>(path, this);
        }

        /// <summary>Save to a JSON file.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="path">The file path relative to the mod directory.</param>
        /// <param name="model">The model to save.</param>
        public void WriteJsonFile<TModel>(string path, TModel model)
            where TModel : class
        {
            path = Path.Combine(this.DirectoryPath, path);
            this.JsonHelper.WriteJsonFile(path, model);
        }
    }
}
