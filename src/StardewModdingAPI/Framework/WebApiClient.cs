using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StardewModdingAPI.Models;

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
        public async Task<IDictionary<string, ModInfoModel>> GetModInfoAsync(params string[] modKeys)
        {
            string url = $"v{this.Version}/mods?modKeys={Uri.EscapeDataString(string.Join(",", modKeys))}";
            return await this.GetAsync<Dictionary<string, ModInfoModel>>(url);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Fetch the response from the backend API.</summary>
        /// <typeparam name="T">The expected response type.</typeparam>
        /// <param name="url">The request URL, optionally excluding the base URL.</param>
        private async Task<T> GetAsync<T>(string url)
        {
            // build request (avoid HttpClient for Mac compatibility)
            HttpWebRequest request = WebRequest.CreateHttp(new Uri(this.BaseUrl, url).ToString());
            request.UserAgent = $"SMAPI/{this.Version}";

            // fetch data
            using (WebResponse response = await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string responseText = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(responseText);
            }
        }
    }
}
