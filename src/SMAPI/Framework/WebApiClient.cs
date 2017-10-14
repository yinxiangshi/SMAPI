using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using StardewModdingAPI.Common.Models;

namespace StardewModdingAPI.Framework
{
    /// <summary>Provides methods for interacting with the SMAPI web API.</summary>
    internal class WebApiClient
    {
        /*********
        ** Properties
        *********/
        /// <summary>The base URL for the web API.</summary>
        private readonly Uri BaseUrl;

        /// <summary>The API version number.</summary>
        private readonly ISemanticVersion Version;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="baseUrl">The base URL for the web API.</param>
        /// <param name="version">The web API version.</param>
        public WebApiClient(string baseUrl, ISemanticVersion version)
        {
#if !SMAPI_FOR_WINDOWS
            baseUrl = baseUrl.Replace("https://", "http://"); // workaround for OpenSSL issues with the game's bundled Mono on Linux/Mac
#endif
            this.BaseUrl = new Uri(baseUrl);
            this.Version = version;
        }

        /// <summary>Get the latest SMAPI version.</summary>
        /// <param name="modKeys">The mod keys for which to fetch the latest version.</param>
        public IDictionary<string, ModInfoModel> GetModInfo(params string[] modKeys)
        {
            return this.Post<ModSearchModel, Dictionary<string, ModInfoModel>>(
                $"v{this.Version}/mods",
                new ModSearchModel(modKeys)
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Fetch the response from the backend API.</summary>
        /// <typeparam name="TBody">The body content type.</typeparam>
        /// <typeparam name="TResult">The expected response type.</typeparam>
        /// <param name="url">The request URL, optionally excluding the base URL.</param>
        /// <param name="content">The body content to post.</param>
        private TResult Post<TBody, TResult>(string url, TBody content)
        {
            /***
            ** Note: avoid HttpClient for Mac compatibility.
            ***/
            using (WebClient client = new WebClient())
            {
                Uri fullUrl = new Uri(this.BaseUrl, url);
                string data = JsonConvert.SerializeObject(content);

                client.Headers["Content-Type"] = "application/json";
                client.Headers["User-Agent"] = $"SMAPI/{this.Version}";
                string response = client.UploadString(fullUrl, data);
                return JsonConvert.DeserializeObject<TResult>(response);
            }
        }
    }
}
