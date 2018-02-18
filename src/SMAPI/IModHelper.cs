using System.Collections.Generic;

namespace StardewModdingAPI
{
    /// <summary>Provides simplified APIs for writing mods.</summary>
    public interface IModHelper
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The full path to the mod's folder.</summary>
        string DirectoryPath { get; }

        /// <summary>An API for loading content assets.</summary>
        IContentHelper Content { get; }

        /// <summary>Simplifies access to private game code.</summary>
        IReflectionHelper Reflection { get; }

        /// <summary>Metadata about loaded mods.</summary>
        IModRegistry ModRegistry { get; }

        /// <summary>An API for managing console commands.</summary>
        ICommandHelper ConsoleCommands { get; }

        /// <summary>Provides translations stored in the mod's <c>i18n</c> folder, with one file per locale (like <c>en.json</c>) containing a flat key => value structure. Translations are fetched with locale fallback, so missing translations are filled in from broader locales (like <c>pt-BR.json</c> &lt; <c>pt.json</c> &lt; <c>default.json</c>).</summary>
        ITranslationHelper Translation { get; }


        /*********
        ** Public methods
        *********/
        /****
        ** Mod config file
        ****/
        /// <summary>Read the mod's configuration file (and create it if needed).</summary>
        /// <typeparam name="TConfig">The config class type. This should be a plain class that has public properties for the settings you want. These can be complex types.</typeparam>
        TConfig ReadConfig<TConfig>() where TConfig : class, new();

        /// <summary>Save to the mod's configuration file.</summary>
        /// <typeparam name="TConfig">The config class type.</typeparam>
        /// <param name="config">The config settings to save.</param>
        void WriteConfig<TConfig>(TConfig config) where TConfig : class, new();

        /****
        ** Generic JSON files
        ****/
        /// <summary>Read a JSON file.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="path">The file path relative to the mod directory.</param>
        /// <returns>Returns the deserialised model, or <c>null</c> if the file doesn't exist or is empty.</returns>
        TModel ReadJsonFile<TModel>(string path) where TModel : class;

        /// <summary>Save to a JSON file.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="path">The file path relative to the mod directory.</param>
        /// <param name="model">The model to save.</param>
        void WriteJsonFile<TModel>(string path, TModel model) where TModel : class;

        /****
        ** Content packs
        ****/
        /// <summary>Get all content packs loaded for this mod.</summary>
        IEnumerable<IContentPack> GetContentPacks();
    }
}
