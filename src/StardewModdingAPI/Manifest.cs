using System;
using Newtonsoft.Json;

namespace StardewModdingAPI
{
    /// <summary>A manifest which describes a mod for SMAPI.</summary>
    public class Manifest
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the manifest defined the deprecated <see cref="Authour"/> field.</summary>
        [JsonIgnore]
        internal bool UsedAuthourField { get; private set; }

        /// <summary>The mod name.</summary>
        public virtual string Name { get; set; } = "";

        /// <summary>The mod author's name.</summary>
        public virtual string Author { get; set; } = "";

        /// <summary>Obsolete.</summary>
        [Obsolete("Use " + nameof(Manifest) + "." + nameof(Manifest.Author) + ".")]
        public virtual string Authour
        {
            get { return this.Author; }
            set
            {
                this.UsedAuthourField = true;
                this.Author = value;
            }
        }

        /// <summary>The mod version.</summary>
        public virtual Version Version { get; set; } = new Version(0, 0, 0, "");

        /// <summary>A brief description of the mod.</summary>
        public virtual string Description { get; set; } = "";

        /// <summary>The unique mod ID.</summary>
        public virtual string UniqueID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Whether the mod uses per-save config files.</summary>
        [Obsolete("Use " + nameof(Mod) + "." + nameof(Mod.Helper) + "." + nameof(IModHelper.ReadConfig) + " instead")]
        public virtual bool PerSaveConfigs { get; set; }

        /// <summary>The minimum SMAPI version required by this mod, if any.</summary>
        public string MinimumApiVersion { get; set; }

        /// <summary>The name of the DLL in the directory that has the <see cref="Mod.Entry"/> method.</summary>
        public virtual string EntryDll { get; set; } = "";
    }
}
