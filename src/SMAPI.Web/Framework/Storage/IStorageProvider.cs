using System.Threading.Tasks;

namespace StardewModdingAPI.Web.Framework.Storage
{
    /// <summary>Provides access to raw data storage.</summary>
    internal interface IStorageProvider
    {
        /// <summary>Save a text file to Pastebin or Amazon S3, if available.</summary>
        /// <param name="title">The display title, if applicable.</param>
        /// <param name="content">The content to upload.</param>
        /// <param name="compress">Whether to gzip the text.</param>
        /// <returns>Returns metadata about the save attempt.</returns>
        Task<UploadResult> SaveAsync(string title, string content, bool compress = true);

        /// <summary>Fetch raw text from storage.</summary>
        /// <param name="id">The storage ID returned by <see cref="SaveAsync"/>.</param>
        Task<StoredFileInfo> GetAsync(string id);
    }
}
