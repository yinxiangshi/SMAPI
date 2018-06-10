namespace StardewModdingAPI.Framework.ModUpdateChecking
{
    /// <summary>Update status for a mod.</summary>
    internal class ModUpdateStatus
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The version that this mod can be updated to (if any).</summary>
        public ISemanticVersion Version { get; }

        /// <summary>The error checking for updates of this mod (if any).</summary>
        public string Error { get; }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="version">The version that this mod can be update to.</param>
        public ModUpdateStatus(ISemanticVersion version)
        {
            this.Version = version;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="error">The error checking for updates of this mod.</param>
        public ModUpdateStatus(string error)
        {
            this.Error = error;
        }

        /// <summary>Construct an instance.</summary>
        public ModUpdateStatus()
        {
        }
    }
}
