using StardewValley;

namespace StardewModdingAPI.Framework
{
    /// <summary>A minimal content manager which defers to SMAPI's main content manager.</summary>
    internal class ContentManagerShim : LocalizedContentManager
    {
        /*********
        ** Properties
        *********/
        /// <summary>SMAPI's underlying content manager.</summary>
        private readonly SContentManager ContentManager;


        /*********
        ** Accessors
        *********/
        /// <summary>The content manager's name for logs (if any).</summary>
        public string Name { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentManager">SMAPI's underlying content manager.</param>
        /// <param name="name">The content manager's name for logs (if any).</param>
        public ContentManagerShim(SContentManager contentManager, string name)
                : base(contentManager.ServiceProvider, contentManager.RootDirectory, contentManager.CurrentCulture, contentManager.LanguageCodeOverride)
        {
            this.ContentManager = contentManager;
            this.Name = name;
        }

        /// <summary>Load an asset that has been processed by the content pipeline.</summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        public override T Load<T>(string assetName)
        {
            return this.ContentManager.LoadFor<T>(assetName, this);
        }

        /// <summary>Dispose held resources.</summary>
        /// <param name="disposing">Whether the content manager is disposing (rather than finalising).</param>
        protected override void Dispose(bool disposing)
        {
            this.ContentManager.DisposeFor(this);
        }
    }
}
