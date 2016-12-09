using StardewModdingAPI.Reflection;

namespace StardewModdingAPI
{
    /// <summary>Provides simplified APIs for writing mods.</summary>
    public interface IModHelper
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod directory path.</summary>
        string DirectoryPath { get; }

        /// <summary>Simplifies access to private game code.</summary>
        IReflectionHelper Reflection { get; }


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
    }
}