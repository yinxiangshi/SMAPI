using System.Threading.Tasks;
using Pathoschild.Http.Client;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Web.Framework.Clients.ModDrop.ResponseModels;

namespace StardewModdingAPI.Web.Framework.Clients.ModDrop
{
    /// <summary>An HTTP client for fetching mod metadata from the ModDrop API.</summary>
    internal class ModDropClient : IModDropClient
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;

        /// <summary>The URL for a ModDrop mod page for the user, where {0} is the mod ID.</summary>
        private readonly string ModUrlFormat;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="userAgent">The user agent for the API client.</param>
        /// <param name="apiUrl">The base URL for the ModDrop API.</param>
        /// <param name="modUrlFormat">The URL for a ModDrop mod page for the user, where {0} is the mod ID.</param>
        public ModDropClient(string userAgent, string apiUrl, string modUrlFormat)
        {
            this.Client = new FluentClient(apiUrl).SetUserAgent(userAgent);
            this.ModUrlFormat = modUrlFormat;
        }

        /// <summary>Get metadata about a mod.</summary>
        /// <param name="id">The ModDrop mod ID.</param>
        /// <returns>Returns the mod info if found, else <c>null</c>.</returns>
        public async Task<ModDropMod> GetModAsync(long id)
        {
            // get raw data
            ModListModel response = await this.Client
                .PostAsync("")
                .WithBody(new
                {
                    ModIDs = new[] { id },
                    Files = true,
                    Mods = true
                })
                .As<ModListModel>();
            ModModel mod = response.Mods[id];
            if (mod.Mod?.Title == null || mod.Mod.ErrorCode.HasValue)
                return null;

            // get latest versions
            ISemanticVersion latest = null;
            ISemanticVersion optional = null;
            foreach (FileDataModel file in mod.Files)
            {
                if (file.IsOld || file.IsDeleted || file.IsHidden)
                    continue;

                if (!SemanticVersion.TryParse(file.Version, out ISemanticVersion version))
                    continue;

                if (file.IsDefault)
                {
                    if (latest == null || version.IsNewerThan(latest))
                        latest = version;
                }
                else if (optional == null || version.IsNewerThan(optional))
                    optional = version;
            }
            if (latest == null)
            {
                latest = optional;
                optional = null;
            }
            if (optional != null && latest.IsNewerThan(optional))
                optional = null;

            // generate result
            return new ModDropMod
            {
                Name = mod.Mod?.Title,
                LatestDefaultVersion = latest,
                LatestOptionalVersion = optional,
                Url = string.Format(this.ModUrlFormat, id)
            };
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.Client?.Dispose();
        }
    }
}
