namespace StardewModdingAPI.Web.Models
{
    /// <summary>Generic metadata about a mod.</summary>
    public class ModGenericModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique mod ID.</summary>
        public int ID { get; set; }

        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The mod's vendor ID.</summary>
        public string Vendor { get; set; }

        /// <summary>The mod's semantic version number.</summary>
        public string Version { get; set; }

        /// <summary>The mod's web URL.</summary>
        public string Url { get; set; }

        /// <summary>Whether the mod is valid.</summary>
        public bool Valid { get; set; } = true;
    }
}
