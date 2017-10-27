namespace StardewModdingAPI.Web.Framework.LogParser
{
    /// <summary>The response for a get-paste request.</summary>
    internal class GetPasteResponse
    {
        /// <summary>Whether the log was successfully fetched.</summary>
        public bool Success { get; set; }

        /// <summary>The fetched paste content (if <see cref="Success"/> is <c>true</c>).</summary>
        public string Content { get; set; }

        /// <summary>The error message (if saving failed).</summary>
        public string Error { get; set; }
    }
}
