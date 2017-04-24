namespace StardewModdingAPI
{
    /// <summary>A manifest which describes a mod for SMAPI.</summary>
    public interface IManifest
    {
        /// <summary>The mod name.</summary>
        string Name { get; set; }

        /// <summary>A brief description of the mod.</summary>
        string Description { get; set; }

        /// <summary>The mod author's name.</summary>
        string Author { get; }

        /// <summary>The mod version.</summary>
        ISemanticVersion Version { get; set; }

        /// <summary>The minimum SMAPI version required by this mod, if any.</summary>
        string MinimumApiVersion { get; set; }

        /// <summary>The minimum game version required by this mod, if any.</summary>
        string MinimumGameVersion { get; set; }

        /// <summary>The unique mod ID.</summary>
        string UniqueID { get; set; }

        /// <summary>The name of the DLL in the directory that has the <see cref="Mod.Entry"/> method.</summary>
        string EntryDll { get; set; }
    }
}