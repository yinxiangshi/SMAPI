namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Metadata for an event raised by SMAPI.</summary>
    internal interface IManagedEvent
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A human-readable name for the event.</summary>
        string EventName { get; }
    }
}
