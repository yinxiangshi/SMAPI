namespace StardewModdingAPI.Web.ViewModels
{
    /// <summary>The view model for the index page.</summary>
    public class IndexModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The latest stable SMAPI version.</summary>
        public IndexVersionModel StableVersion { get; set; }

        /// <summary>The latest prerelease SMAPI version (if newer than <see cref="StableVersion"/>).</summary>
        public IndexVersionModel BetaVersion { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public IndexModel() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="stableVersion">The latest stable SMAPI version.</param>
        /// <param name="betaVersion">The latest prerelease SMAPI version (if newer than <paramref name="stableVersion"/>).</param>
        internal IndexModel(IndexVersionModel stableVersion, IndexVersionModel betaVersion)
        {
            this.StableVersion = stableVersion;
            this.BetaVersion = betaVersion;
        }
    }
}
