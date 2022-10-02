// Copyright 2022 Jamie Taylor
using System;
using Pathoschild.Http.Client;
using StardewModdingAPI.Toolkit.Framework.UpdateData;
using System.Threading.Tasks;
using System.Net;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest {
    /// <summary>An HTTP client for fetching an update manifest from an arbitrary URL.</summary>
    internal class UpdateManifestClient : IUpdateManifestClient {
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
        public UpdateManifestClient(string userAgent) {
            this.Client = new FluentClient()
                .SetUserAgent(userAgent);
            this.Client.Formatters.Add(new TextAsJsonMediaTypeFormatter());
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() {
            this.Client.Dispose();
        }

        /// <inheritdoc/>
        public async Task<IModPage?> GetModData(string id) {
            UpdateManifestModel? manifest;
            try {
                manifest = await this.Client.GetAsync(id).As<UpdateManifestModel?>();
            } catch (ApiException ex) when (ex.Status == HttpStatusCode.NotFound) {
                return new GenericModPage(this.SiteKey, id).SetError(RemoteModStatus.DoesNotExist, $"No update manifest found at {id}");
            }
            if (manifest is null) {
                return new GenericModPage(this.SiteKey, id).SetError(RemoteModStatus.DoesNotExist, $"Error parsing manifest at {id}");
            }

            return new UpdateManifestModPage(id, manifest);
        }
    }
}
