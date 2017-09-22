namespace StardewModdingAPI.Web.Models
{
    /// <summary>Generic metadata about a mod.</summary>
    public class ModGenericModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique mod ID.</summary>
        public int ID { get; }

        /// <summary>The mod name.</summary>
        public string Name { get; }

        /// <summary>The mod's vendor ID.</summary>
        public string Vendor { get; }

        /// <summary>The mod's semantic version number.</summary>
        public string Version { get; }

        /// <summary>The mod's web URL.</summary>
        public string Url { get; }

        /// <summary>Whether the mod is valid.</summary>
        public bool Valid { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct a valid instance.</summary>
        /// <param name="vendor">The mod's vendor ID.</param>
        /// <param name="id">The unique mod ID.</param>
        /// <param name="name">The mod name.</param>
        /// <param name="version">The mod's semantic version number.</param>
        /// <param name="url">The mod's web URL.</param>
        /// <param name="valid">Whether the mod is valid.</param>
        public ModGenericModel(string vendor, int id, string name, string version, string url, bool valid = true)
        {
            this.Vendor = vendor;
            this.ID = id;
            this.Name = name;
            this.Version = version;
            this.Url = url;
            this.Valid = valid;
        }

        /// <summary>Construct an valid instance.</summary>
        /// <param name="vendor">The mod's vendor ID.</param>
        /// <param name="id">The unique mod ID.</param>
        public ModGenericModel(string vendor, int id)
        {
            this.Vendor = vendor;
            this.ID = id;
            this.Valid = false;
        }
    }
}
