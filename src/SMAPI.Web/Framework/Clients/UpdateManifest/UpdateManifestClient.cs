// Copyright 2022 Jamie Taylor
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Pathoschild.Http.Client;
using StardewModdingAPI.Toolkit.Framework.UpdateData;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest
{
    /// <summary>An API client for fetching update metadata from an arbitrary JSON URL.</summary>
    internal class UpdateManifestClient : IUpdateManifestClient
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;


        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for the mod site.</summary>
        public ModSiteKey SiteKey => ModSiteKey.UpdateManifest;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="userAgent">The user agent for the API client.</param>
        public UpdateManifestClient(string userAgent)
        {
            this.Client = new FluentClient()
                .SetUserAgent(userAgent);

            this.Client.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.Client.Dispose();
        }

        /// <inheritdoc/>
        public async Task<IModPage?> GetModData(string id)
        {
            UpdateManifestModel? manifest;
            try
            {
                manifest = await this.Client.GetAsync(id).As<UpdateManifestModel?>();
            }
            catch (ApiException ex) when (ex.Status == HttpStatusCode.NotFound)
            {
                return new GenericModPage(this.SiteKey, id).SetError(RemoteModStatus.DoesNotExist, $"No update manifest found at {id}");
            }

            return manifest is not null
                ? new UpdateManifestModPage(id, manifest)
                : new GenericModPage(this.SiteKey, id).SetError(RemoteModStatus.DoesNotExist, $"The update manifest at {id} has an invalid format");
        }
    }
}
