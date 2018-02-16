namespace StardewModdingAPI.Framework.Models
{
    /// <summary>A parsed representation of the fields from a <see cref="ModDataRecord"/> for a specific manifest.</summary>
    internal class ParsedModDataRecord
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying data record.</summary>
        public ModDataRecord DataRecord { get; set; }

        /// <summary>The update key to apply.</summary>
        public string UpdateKey { get; set; }

        /// <summary>The mod version to apply.</summary>
        public ISemanticVersion Version { get; set; }

        /// <summary>The alternative URL the player can check for an updated version.</summary>
        public string AlternativeUrl { get; set; }

        /// <summary>The predefined compatibility status.</summary>
        public ModStatus Status { get; set; } = ModStatus.None;

        /// <summary>A reason phrase for the <see cref="Status"/>, or <c>null</c> to use the default reason.</summary>
        public string StatusReasonPhrase { get; set; }

        /// <summary>The upper version for which the <see cref="Status"/> applies (if any).</summary>
        public ISemanticVersion StatusUpperVersion { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get a semantic local version for update checks.</summary>
        /// <param name="version">The remote version to normalise.</param>
        public string GetLocalVersionForUpdateChecks(string version)
        {
            return this.DataRecord.GetLocalVersionForUpdateChecks(version);
        }

        /// <summary>Get a semantic remote version for update checks.</summary>
        /// <param name="version">The remote version to normalise.</param>
        public string GetRemoteVersionForUpdateChecks(string version)
        {
            return this.DataRecord.GetRemoteVersionForUpdateChecks(version);
        }
    }
}
