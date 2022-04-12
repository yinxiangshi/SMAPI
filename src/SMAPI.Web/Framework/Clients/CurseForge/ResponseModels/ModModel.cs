namespace StardewModdingAPI.Web.Framework.Clients.CurseForge.ResponseModels
{
    /// <summary>An mod from the CurseForge API.</summary>
    public class ModModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's unique ID on CurseForge.</summary>
        public int ID { get; }

        /// <summary>The mod name.</summary>
        public string Name { get; }

        /// <summary>The web URL for the mod page.</summary>
        public string WebsiteUrl { get; }

        /// <summary>The available file downloads.</summary>
        public ModFileModel[] LatestFiles { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The mod's unique ID on CurseForge.</param>
        /// <param name="name">The mod name.</param>
        /// <param name="websiteUrl">The web URL for the mod page.</param>
        /// <param name="latestFiles">The available file downloads.</param>
        public ModModel(int id, string name, string websiteUrl, ModFileModel[] latestFiles)
        {
            this.ID = id;
            this.Name = name;
            this.WebsiteUrl = websiteUrl;
            this.LatestFiles = latestFiles;
        }
    }
}
