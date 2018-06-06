namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Indicates which mod can read the content pack represented by the containing manifest.</summary>
    internal class ManifestContentPackFor : IManifestContentPackFor
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique ID of the mod which can read this content pack.</summary>
        public string UniqueID { get; }

        /// <summary>The minimum required version (if any).</summary>
        public ISemanticVersion MinimumVersion { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPackFor">The toolkit instance.</param>
        public ManifestContentPackFor(Toolkit.Serialisation.Models.ManifestContentPackFor contentPackFor)
        {
            this.UniqueID = contentPackFor.UniqueID;
            this.MinimumVersion = contentPackFor.MinimumVersion != null ? new SemanticVersion(contentPackFor.MinimumVersion) : null;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="uniqueID">The unique ID of the mod which can read this content pack.</param>
        /// <param name="minimumVersion">The minimum required version (if any).</param>
        public ManifestContentPackFor(string uniqueID, ISemanticVersion minimumVersion = null)
        {
            this.UniqueID = uniqueID;
            this.MinimumVersion = minimumVersion;
        }
    }
}
