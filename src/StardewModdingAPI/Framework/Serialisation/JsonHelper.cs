using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI.Advanced;

namespace StardewModdingAPI.Framework.Serialisation
{
    /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
    internal class JsonHelper
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The JSON settings to use when serialising and deserialising files.</summary>
        private readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ObjectCreationHandling = ObjectCreationHandling.Replace, // avoid issue where default ICollection<T> values are duplicated each time the config is loaded
            Converters = new List<JsonConverter>
            {
                new SelectiveStringEnumConverter(typeof(Buttons), typeof(Keys))
            }
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Read a JSON file.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="fullPath">The absolete file path.</param>
        /// <param name="modHelper">The mod helper to inject for <see cref="IConfigFile"/> instances.</param>
        /// <returns>Returns the deserialised model, or <c>null</c> if the file doesn't exist or is empty.</returns>
        public TModel ReadJsonFile<TModel>(string fullPath, IModHelper modHelper)
            where TModel : class
        {
            // read file
            string json;
            try
            {
                json = File.ReadAllText(fullPath);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                return null;
            }

            // deserialise model
            TModel model = JsonConvert.DeserializeObject<TModel>(json, this.JsonSettings);
            if (model is IConfigFile)
            {
                var wrapper = (IConfigFile)model;
                wrapper.ModHelper = modHelper;
                wrapper.FilePath = fullPath;
            }

            return model;
        }

        /// <summary>Save to a JSON file.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="fullPath">The absolete file path.</param>
        /// <param name="model">The model to save.</param>
        public void WriteJsonFile<TModel>(string fullPath, TModel model)
            where TModel : class
        {
            // create directory if needed
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // write file
            string json = JsonConvert.SerializeObject(model, this.JsonSettings);
            File.WriteAllText(fullPath, json);
        }
    }
}
