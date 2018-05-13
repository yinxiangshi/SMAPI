using System;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>Indicates a detected non-error mod issue.</summary>
    [Flags]
    internal enum ModWarning
    {
        /// <summary>No issues detected.</summary>
        None = 0,

        /// <summary>SMAPI detected incompatible code in the mod, but was configured to load it anyway.</summary>
        BrokenCodeLoaded = 1,

        /// <summary>The mod affects the save serializer in a way that may make saves unloadable without the mod.</summary>
        ChangesSaveSerialiser = 2,

        /// <summary>The mod patches the game in a way that may impact stability.</summary>
        PatchesGame = 4,

        /// <summary>The mod uses the <c>dynamic</c> keyword which won't work on Linux/Mac.</summary>
        UsesDynamic = 8,

        /// <summary>The mod references <see cref="SpecialisedEvents.UnvalidatedUpdateTick"/> which may impact stability.</summary>
        UsesUnvalidatedUpdateTick = 16,

        /// <summary>The mod has no update keys set.</summary>
        NoUpdateKeys = 32
    }
}
