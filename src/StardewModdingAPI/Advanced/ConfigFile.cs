using System.IO;
using Newtonsoft.Json;

namespace StardewModdingAPI.Advanced
{
    /// <summary>Wraps a configuration file with IO methods for convenience.</summary>
    public abstract class ConfigFile : IConfigFile
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Provides simplified APIs for writing mods.</summary>
        public IModHelper ModHelper { get; set; }

        /// <summary>The file path from which the model was loaded, relative to the mod directory.</summary>
        public string FilePath { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Reparse the underlying file and update this model.</summary>
        public void Reload()
        {
            string json = File.ReadAllText(Path.Combine(this.ModHelper.DirectoryPath, this.FilePath));
            JsonConvert.PopulateObject(json, this);
        }

        /// <summary>Save this model to the underlying file.</summary>
        public void Save()
        {
            this.ModHelper.WriteJsonFile(this.FilePath, this);
        }
    }
}