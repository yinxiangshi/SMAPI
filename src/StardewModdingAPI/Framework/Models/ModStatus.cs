namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Indicates how SMAPI should treat a mod.</summary>
    internal enum ModStatus
    {
        /// <summary>Don't override the status.</summary>
        None = 0,

        /// <summary>Assume the mod is not compatible, even if SMAPI doesn't detect any incompatible code.</summary>
        AssumeBroken = 0,

        /// <summary>Assume the mod is compatible, even if SMAPI detects incompatible code.</summary>
        AssumeCompatible = 1
    }
}
