using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewModdingAPI
{
    /// <summary>A manifest which describes a mod for SMAPI.</summary>
    public class Manifest : Config
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public virtual string Name { get; set; }

        /// <summary>The mod author's name.</summary>
        public virtual string Author { get; set; }

        /// <summary>Obsolete.</summary>
        [Obsolete("Use 'Author'.")]
        public virtual string Authour
        {
            get { return this.Author; }
            set { this.Author = value; }
        }

        /// <summary>The mod version.</summary>
        public virtual Version Version { get; set; }

        /// <summary>A brief description of the mod.</summary>
        public virtual string Description { get; set; }

        /// <summary>The unique mod ID.</summary>
        public virtual string UniqueID { get; set; }

        /// <summary>Whether the mod uses per-save config files.</summary>
        public virtual bool PerSaveConfigs { get; set; }

        /// <summary>The name of the DLL in the directory that has the <see cref="Mod.Entry"/> method.</summary>
        public virtual string EntryDll { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the default config values.</summary>
        public override T GenerateDefaultConfig<T>()
        {
            this.Name = "";
            this.Author = "";
            this.Version = new Version(0, 0, 0, "");
            this.Description = "";
            this.UniqueID = Guid.NewGuid().ToString();
            this.PerSaveConfigs = false;
            this.EntryDll = "";
            return this as T;
        }

        /// <summary>Load the config from the JSON file, saving it to disk if needed.</summary>
        /// <typeparam name="T">The config class type.</typeparam>
        public override T LoadConfig<T>()
        {
            if (File.Exists(this.ConfigLocation))
            {
                try
                {
                    JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(this.ConfigLocation));
                }
                catch
                {
                    //Invalid json blob. Try to remove version?
                    try
                    {
                        JObject j = JObject.Parse(File.ReadAllText(this.ConfigLocation));
                        if (!j.GetValue("Version").Contains("{"))
                        {
                            Log.AsyncC("INVALID JSON VERSION. TRYING TO REMOVE SO A NEW CAN BE AUTO-GENERATED");
                            j.Remove("Version");
                            File.WriteAllText(this.ConfigLocation, j.ToString());
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return base.LoadConfig<T>();
        }
    }
}
