using System.IO;

namespace StardewModdingAPI
{
    /// <summary>The base class for a mod.</summary>
    public class Mod
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's manifest.</summary>
        public Manifest Manifest { get; internal set; }

        /// <summary>The full path to the mod's directory on the disk.</summary>
        public string PathOnDisk { get; internal set; }

        /// <summary>The full path to the mod's <c>config.json</c> file on the disk.</summary>
        public string BaseConfigPath => Path.Combine(this.PathOnDisk, "config.json");

        /// <summary>The full path to the per-save configs folder (if <see cref="StardewModdingAPI.Manifest.PerSaveConfigs"/> is <c>true</c>).</summary>
        public string PerSaveConfigFolder => this.GetPerSaveConfigFolder();

        /// <summary>The full path to the per-save configuration file for the current save (if <see cref="StardewModdingAPI.Manifest.PerSaveConfigs"/> is <c>true</c>).</summary>
        public string PerSaveConfigPath => Constants.CurrentSavePathExists ? Path.Combine(this.PerSaveConfigFolder, Constants.SaveFolderName + ".json") : "";


        /*********
        ** Public methods
        *********/
        /// <summary>The entry point for your mod. It will always be called once when the mod loads.</summary>
        public virtual void Entry(params object[] objects)
        {
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the full path to the per-save configuration file for the current save (if <see cref="StardewModdingAPI.Manifest.PerSaveConfigs"/> is <c>true</c>).</summary>
        private string GetPerSaveConfigFolder()
        {
            if (!this.Manifest.PerSaveConfigs)
            {
                Log.AsyncR($"The mod [{this.Manifest.Name}] is not configured to use per-save configs.");
                return "";
            }
            return Path.Combine(this.PathOnDisk, "psconfigs");
        }
    }
}
