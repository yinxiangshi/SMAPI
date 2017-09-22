using Newtonsoft.Json;

namespace StardewModdingAPI.Web.Models
{
    /// <summary>Generic metadata about a mod.</summary>
    public class ModInfoModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public string Name { get; }

        /// <summary>The mod's semantic version number.</summary>
        public string Version { get; }

        /// <summary>The mod's web URL.</summary>
        public string Url { get; }

        /// <summary>The error message indicating why the mod is invalid (if applicable).</summary>
        public string Error { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct a valid instance.</summary>
        /// <param name="name">The mod name.</param>
        /// <param name="version">The mod's semantic version number.</param>
        /// <param name="url">The mod's web URL.</param>
        /// <param name="error">The error message indicating why the mod is invalid (if applicable).</param>
        [JsonConstructor]
        public ModInfoModel(string name, string version, string url, string error = null)
        {
            this.Name = name;
            this.Version = version;
            this.Url = url;
            this.Error = error; // mainly initialised here for the JSON deserialiser
        }

        /// <summary>Construct an valid instance.</summary>
        /// <param name="error">The error message indicating why the mod is invalid.</param>
        public ModInfoModel(string error)
        {
            this.Error = error;
        }
    }
}
