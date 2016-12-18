using System;
using Newtonsoft.Json;

namespace StardewModdingAPI
{
    /// <summary>Wraps <see cref="Manifest"/> so it can implement <see cref="IManifest"/> without breaking backwards compatibility.</summary>
    [Obsolete("Use " + nameof(IManifest) + " or " + nameof(Mod) + "." + nameof(Mod.ModManifest) + " instead")]
    internal class ManifestImpl : Manifest, IManifest
    {
        /// <summary>The mod version.</summary>
        public new ISemanticVersion Version
        {
            get { return base.Version; }
            set { base.Version = (Version)value; }
        }
    }

    /// <summary>A manifest which describes a mod for SMAPI.</summary>
    public class Manifest
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>A brief description of the mod.</summary>
        public string Description { get; set; }

        /// <summary>The mod author's name.</summary>
        public string Author { get; set; }

        /// <summary>The mod version.</summary>
        public Version Version { get; set; } = new Version(0, 0, 0, "", suppressDeprecationWarning: true);

        /// <summary>The minimum SMAPI version required by this mod, if any.</summary>
        public string MinimumApiVersion { get; set; }

        /// <summary>The name of the DLL in the directory that has the <see cref="Mod.Entry"/> method.</summary>
        public string EntryDll { get; set; }

        /// <summary>The unique mod ID.</summary>
        public string UniqueID { get; set; } = Guid.NewGuid().ToString();


        /****
        ** Obsolete
        ****/
        /// <summary>Whether the manifest defined the deprecated <see cref="Authour"/> field.</summary>
        [JsonIgnore]
        internal bool UsedAuthourField { get; private set; }

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

        /// <summary>Whether the mod uses per-save config files.</summary>
        [Obsolete("Use " + nameof(Mod) + "." + nameof(Mod.Helper) + "." + nameof(IModHelper.ReadConfig) + " instead")]
        public bool PerSaveConfigs { get; set; }
    }
}
