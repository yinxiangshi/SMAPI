// Copyright 2022 Jamie Taylor
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest
{
    /// <summary>A <see cref="JsonMediaTypeFormatter"/> that can parse from content of type <c>text/plain</c>.</summary>
    internal class TextAsJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct a new <see cref="JsonMediaTypeFormatter"/></summary>
        public TextAsJsonMediaTypeFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
        }
    }
}
