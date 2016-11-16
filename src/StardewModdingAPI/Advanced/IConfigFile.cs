namespace StardewModdingAPI.Advanced
{
    /// <summary>Wraps a configuration file with IO methods for convenience.</summary>
    public interface IConfigFile
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Provides methods for interacting with the mod directory, including read/writing the config file.</summary>
        IModHelper ModHelper { get; set; }

        /// <summary>The file path from which the model was loaded, relative to the mod directory.</summary>
        string FilePath { get; set; }


        /*********
        ** Methods
        *********/
        /// <summary>Reparse the underlying file and update this model.</summary>
        void Reload();

        /// <summary>Save this model to the underlying file.</summary>
        void Save();
    }
}
