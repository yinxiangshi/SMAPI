using System.Threading.Tasks;
using StardewModdingAPI.Toolkit.Framework.Clients.Wiki;

namespace StardewModdingAPI.Toolkit
{
    /// <summary>A convenience wrapper for the various tools.</summary>
    public class ModToolkit
    {
        /*********
        ** Properties
        *********/
        /// <summary>The default HTTP user agent for the toolkit.</summary>
        private readonly string UserAgent;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ModToolkit()
        {
            ISemanticVersion version = new SemanticVersion(this.GetType().Assembly.GetName().Version);
            this.UserAgent = $"SMAPI Mod Handler Toolkit/{version}";
        }

        /// <summary>Extract mod metadata from the wiki compatibility list.</summary>
        public async Task<WikiCompatibilityEntry[]> GetWikiCompatibilityListAsync()
        {
            var client = new WikiCompatibilityClient(this.UserAgent);
            return await client.FetchAsync();
        }
    }
}
