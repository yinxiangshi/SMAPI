using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.Input;
using StardewModdingAPI.Toolkit.Serialisation;
using StardewModdingAPI.Toolkit.Serialisation.Models;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingAPI.Framework.ModHelpers
{
    /// <summary>Provides simplified APIs for writing mods.</summary>
    internal class ModHelper : BaseHelper, IModHelper, IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
        private readonly JsonHelper JsonHelper;

        /// <summary>The content packs loaded for this mod.</summary>
        private readonly IContentPack[] ContentPacks;

        /// <summary>Create a transitional content pack.</summary>
        private readonly Func<string, IManifest, IContentPack> CreateContentPack;

        /// <summary>Manages deprecation warnings.</summary>
        private readonly DeprecationManager DeprecationManager;


        /*********
        ** Accessors
        *********/
        /// <summary>The full path to the mod's folder.</summary>
        public string DirectoryPath { get; }

        /// <summary>Manages access to events raised by SMAPI, which let your mod react when something happens in the game.</summary>
        public IModEvents Events { get; }

        /// <summary>An API for loading content assets.</summary>
        public IContentHelper Content { get; }

        /// <summary>An API for checking and changing input state.</summary>
        public IInputHelper Input { get; }

        /// <summary>An API for accessing private game code.</summary>
        public IReflectionHelper Reflection { get; }

        /// <summary>an API for fetching metadata about loaded mods.</summary>
        public IModRegistry ModRegistry { get; }

        /// <summary>An API for managing console commands.</summary>
        public ICommandHelper ConsoleCommands { get; }

        /// <summary>Provides multiplayer utilities.</summary>
        public IMultiplayerHelper Multiplayer { get; }

        /// <summary>An API for reading translations stored in the mod's <c>i18n</c> folder, with one file per locale (like <c>en.json</c>) containing a flat key => value structure. Translations are fetched with locale fallback, so missing translations are filled in from broader locales (like <c>pt-BR.json</c> &lt; <c>pt.json</c> &lt; <c>default.json</c>).</summary>
        public ITranslationHelper Translation { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modID">The mod's unique ID.</param>
        /// <param name="modDirectory">The full path to the mod's folder.</param>
        /// <param name="jsonHelper">Encapsulate SMAPI's JSON parsing.</param>
        /// <param name="inputState">Manages the game's input state.</param>
        /// <param name="events">Manages access to events raised by SMAPI.</param>
        /// <param name="contentHelper">An API for loading content assets.</param>
        /// <param name="commandHelper">An API for managing console commands.</param>
        /// <param name="modRegistry">an API for fetching metadata about loaded mods.</param>
        /// <param name="reflectionHelper">An API for accessing private game code.</param>
        /// <param name="multiplayer">Provides multiplayer utilities.</param>
        /// <param name="translationHelper">An API for reading translations stored in the mod's <c>i18n</c> folder.</param>
        /// <param name="contentPacks">The content packs loaded for this mod.</param>
        /// <param name="createContentPack">Create a transitional content pack.</param>
        /// <param name="deprecationManager">Manages deprecation warnings.</param>
        /// <exception cref="ArgumentNullException">An argument is null or empty.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="modDirectory"/> path does not exist on disk.</exception>
        public ModHelper(string modID, string modDirectory, JsonHelper jsonHelper, SInputState inputState, IModEvents events, IContentHelper contentHelper, ICommandHelper commandHelper, IModRegistry modRegistry, IReflectionHelper reflectionHelper, IMultiplayerHelper multiplayer, ITranslationHelper translationHelper, IEnumerable<IContentPack> contentPacks, Func<string, IManifest, IContentPack> createContentPack, DeprecationManager deprecationManager)
            : base(modID)
        {
            // validate directory
            if (string.IsNullOrWhiteSpace(modDirectory))
                throw new ArgumentNullException(nameof(modDirectory));
            if (!Directory.Exists(modDirectory))
                throw new InvalidOperationException("The specified mod directory does not exist.");

            // initialise
            this.DirectoryPath = modDirectory;
            this.JsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
            this.Content = contentHelper ?? throw new ArgumentNullException(nameof(contentHelper));
            this.Input = new InputHelper(modID, inputState);
            this.ModRegistry = modRegistry ?? throw new ArgumentNullException(nameof(modRegistry));
            this.ConsoleCommands = commandHelper ?? throw new ArgumentNullException(nameof(commandHelper));
            this.Reflection = reflectionHelper ?? throw new ArgumentNullException(nameof(reflectionHelper));
            this.Multiplayer = multiplayer ?? throw new ArgumentNullException(nameof(multiplayer));
            this.Translation = translationHelper ?? throw new ArgumentNullException(nameof(translationHelper));
            this.ContentPacks = contentPacks.ToArray();
            this.CreateContentPack = createContentPack;
            this.DeprecationManager = deprecationManager;
            this.Events = events;
        }

        /****
        ** Mod config file
        ****/
        /// <summary>Read the mod's configuration file (and create it if needed).</summary>
        /// <typeparam name="TConfig">The config class type. This should be a plain class that has public properties for the settings you want. These can be complex types.</typeparam>
        public TConfig ReadConfig<TConfig>()
            where TConfig : class, new()
        {
            TConfig config = this.ReadJsonFile<TConfig>("config.json") ?? new TConfig();
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
            path = Path.Combine(this.DirectoryPath, PathUtilities.NormalisePathSeparators(path));
            return this.JsonHelper.ReadJsonFileIfExists(path, out TModel data)
                ? data
                : null;
        }

        /// <summary>Save to a JSON file.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="path">The file path relative to the mod directory.</param>
        /// <param name="model">The model to save.</param>
        public void WriteJsonFile<TModel>(string path, TModel model)
            where TModel : class
        {
            path = Path.Combine(this.DirectoryPath, PathUtilities.NormalisePathSeparators(path));
            this.JsonHelper.WriteJsonFile(path, model);
        }

        /****
        ** Content packs
        ****/
        /// <summary>Manually create a transitional content pack to support pre-SMAPI content packs. This provides a way to access legacy content packs using the SMAPI content pack APIs, but the content pack will not be visible in the log or validated by SMAPI.</summary>
        /// <param name="directoryPath">The absolute directory path containing the content pack files.</param>
        /// <param name="id">The content pack's unique ID.</param>
        /// <param name="name">The content pack name.</param>
        /// <param name="description">The content pack description.</param>
        /// <param name="author">The content pack author's name.</param>
        /// <param name="version">The content pack version.</param>
        [Obsolete("This method supports mods which previously had their own content packs, and shouldn't be used by new mods. It will be removed in SMAPI 3.0.")]
        public IContentPack CreateTransitionalContentPack(string directoryPath, string id, string name, string description, string author, ISemanticVersion version)
        {
            // raise deprecation notice
            this.DeprecationManager.Warn($"{nameof(IModHelper)}.{nameof(IModHelper.CreateTransitionalContentPack)}", "2.5", DeprecationLevel.Notice);

            // validate
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentNullException(nameof(directoryPath));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (!Directory.Exists(directoryPath))
                throw new ArgumentException($"Can't create content pack for directory path '{directoryPath}' because no such directory exists.");

            // create manifest
            IManifest manifest = new Manifest(
                uniqueID: id,
                name: name,
                author: author,
                description: description,
                version: version,
                contentPackFor: this.ModID
            );

            // create content pack
            return this.CreateContentPack(directoryPath, manifest);
        }

        /// <summary>Get all content packs loaded for this mod.</summary>
        public IEnumerable<IContentPack> GetContentPacks()
        {
            return this.ContentPacks;
        }

        /****
        ** Disposal
        ****/
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            // nothing to dispose yet
        }
    }
}
