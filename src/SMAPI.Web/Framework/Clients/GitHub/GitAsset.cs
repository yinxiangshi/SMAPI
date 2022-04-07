#nullable disable

using Newtonsoft.Json;

namespace StardewModdingAPI.Web.Framework.Clients.GitHub
{
    /// <summary>A GitHub download attached to a release.</summary>
    internal class GitAsset
    {
        /// <summary>The file name.</summary>
        [JsonProperty("name")]
        public string FileName { get; set; }

        /// <summary>The file content type.</summary>
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        /// <summary>The download URL.</summary>
        [JsonProperty("browser_download_url")]
        public string DownloadUrl { get; set; }
    }
}
