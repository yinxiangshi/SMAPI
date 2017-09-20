namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>Indicates how an instruction was handled.</summary>
    internal enum InstructionHandleResult
    {
        /// <summary>No special handling is needed.</summary>
        None,

        /// <summary>The instruction was successfully rewritten for compatibility.</summary>
        Rewritten,

        /// <summary>The instruction is not compatible and can't be rewritten for compatibility.</summary>
        NotCompatible,

        /// <summary>The instruction is compatible, but patches the game in a way that may impact stability.</summary>
        DetectedGamePatch,

        /// <summary>The instruction is compatible, but affects the save serializer in a way that may make saves unloadable without the mod.</summary>
        DetectedSaveSerialiser
    }
}
