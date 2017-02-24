using System;
using System.IO;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI
{
    /// <summary>The base class for a mod.</summary>
    public class Mod : IMod
    {
        /*********
        ** Properties
        *********/
        /// <summary>Manages deprecation warnings.</summary>
        private static DeprecationManager DeprecationManager;

        /// <summary>The backing field for <see cref="Mod.PathOnDisk"/>.</summary>
        private string _pathOnDisk;


        /*********
        ** Accessors
        *********/
        /// <summary>Provides simplified APIs for writing mods.</summary>
        public IModHelper Helper { get; internal set; }

        /// <summary>Writes messages to the console and log file.</summary>
        public IMonitor Monitor { get; internal set; }

        /// <summary>The mod's manifest.</summary>
        public IManifest ModManifest { get; internal set; }

        /// <summary>The full path to the mod's directory on the disk.</summary>
        [Obsolete("Use " + nameof(Mod.Helper) + "." + nameof(IModHelper.DirectoryPath) + " instead")]
        public string PathOnDisk
        {
            get
            {
                Mod.DeprecationManager.Warn($"{nameof(Mod)}.{nameof(Mod.PathOnDisk)}", "1.0", DeprecationLevel.Info);
                return this._pathOnDisk;
            }
            internal set { this._pathOnDisk = value; }
        }

        /// <summary>The full path to the mod's <c>config.json</c> file on the disk.</summary>
        [Obsolete("Use " + nameof(Mod.Helper) + "." + nameof(IModHelper.ReadConfig) + " instead")]
        public string BaseConfigPath
        {
            get
            {
                Mod.DeprecationManager.Warn($"{nameof(Mod)}.{nameof(Mod.BaseConfigPath)}", "1.0", DeprecationLevel.Info);
                Mod.DeprecationManager.MarkWarned($"{nameof(Mod)}.{nameof(Mod.PathOnDisk)}", "1.0"); // avoid redundant warnings
                return Path.Combine(this.PathOnDisk, "config.json");
            }
        }

        /// <summary>The full path to the per-save configs folder (if <see cref="Manifest.PerSaveConfigs"/> is <c>true</c>).</summary>
        [Obsolete("Use " + nameof(Mod.Helper) + "." + nameof(IModHelper.ReadJsonFile) + " instead")]
        public string PerSaveConfigFolder => this.GetPerSaveConfigFolder();

        /// <summary>The full path to the per-save configuration file for the current save (if <see cref="Manifest.PerSaveConfigs"/> is <c>true</c>).</summary>
        [Obsolete("Use " + nameof(Mod.Helper) + "." + nameof(IModHelper.ReadJsonFile) + " instead")]
        public string PerSaveConfigPath
        {
            get
            {
                Mod.DeprecationManager.Warn($"{nameof(Mod)}.{nameof(Mod.PerSaveConfigPath)}", "1.0", DeprecationLevel.Info);
                Mod.DeprecationManager.MarkWarned($"{nameof(Mod)}.{nameof(Mod.PerSaveConfigFolder)}", "1.0"); // avoid redundant warnings
                return Constants.IsSaveLoaded ? Path.Combine(this.PerSaveConfigFolder, $"{Constants.SaveFolderName}.json") : "";
            }
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Injects types required for backwards compatibility.</summary>
        /// <param name="deprecationManager">Manages deprecation warnings.</param>
        internal static void Shim(DeprecationManager deprecationManager)
        {
            Mod.DeprecationManager = deprecationManager;
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        [Obsolete("This overload is obsolete since SMAPI 1.0.")]
        public virtual void Entry(params object[] objects) { }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public virtual void Entry(IModHelper helper) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the full path to the per-save configuration file for the current save (if <see cref="Manifest.PerSaveConfigs"/> is <c>true</c>).</summary>
        [Obsolete]
        private string GetPerSaveConfigFolder()
        {
            Mod.DeprecationManager.Warn($"{nameof(Mod)}.{nameof(Mod.PerSaveConfigFolder)}", "1.0", DeprecationLevel.Info);
            Mod.DeprecationManager.MarkWarned($"{nameof(Mod)}.{nameof(Mod.PathOnDisk)}", "1.0"); // avoid redundant warnings

            if (!((Manifest)this.ModManifest).PerSaveConfigs)
            {
                this.Monitor.Log("Tried to fetch the per-save config folder, but this mod isn't configured to use per-save config files.", LogLevel.Error);
                return "";
            }
            return Path.Combine(this.PathOnDisk, "psconfigs");
        }
    }
}
