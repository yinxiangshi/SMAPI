namespace StardewModdingAPI.Web.ViewModels.JsonValidator
{
    /// <summary>The view model for a JSON validation request.</summary>
    public class JsonValidatorRequestModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The schema name with which to validate the JSON.</summary>
        public string SchemaName { get; }

        /// <summary>The raw content to validate.</summary>
        public string Content { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="schemaName">The schema name with which to validate the JSON.</param>
        /// <param name="content">The raw content to validate.</param>
        public JsonValidatorRequestModel(string schemaName, string content)
        {
            this.SchemaName = schemaName;
            this.Content = content;
        }
    }
}
