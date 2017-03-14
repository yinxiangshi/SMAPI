using System.IO;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using StardewModdingAPI.Framework.Models;

namespace StardewModdingAPI.Framework
{
    /// <summary>Provides utility methods for mod updates.</summary>
    internal class UpdateHelper
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the latest release from a GitHub repository.</summary>
        /// <param name="repository">The name of the repository from which to fetch releases (like "cjsu/SMAPI").</param>
        public static GitRelease GetLatestVersion(string repository)
        {
            // build request
            // (avoid HttpClient for Mac compatibility)
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://api.github.com/repos/{repository}/releases/latest");
            AssemblyName assembly = typeof(UpdateHelper).Assembly.GetName();
            request.UserAgent = $"{assembly.Name}/{assembly.Version}";
            request.Accept = "application/vnd.github.v3+json";

            // fetch data 
            using (WebResponse response = request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string responseText = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<GitRelease>(responseText);
            }
        }
    }
}
