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

        /// <summary>A short sentence shown under the beta download button, if any.</summary>
        public string BetaBlurb { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public IndexModel() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="stableVersion">The latest stable SMAPI version.</param>
        /// <param name="betaVersion">The latest prerelease SMAPI version (if newer than <paramref name="stableVersion"/>).</param>
        /// <param name="betaBlurb">A short sentence shown under the beta download button, if any.</param>
        internal IndexModel(IndexVersionModel stableVersion, IndexVersionModel betaVersion, string betaBlurb)
        {
            this.StableVersion = stableVersion;
            this.BetaVersion = betaVersion;
            this.BetaBlurb = betaBlurb;
        }
    }
}
