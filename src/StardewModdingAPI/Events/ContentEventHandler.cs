namespace StardewModdingAPI.Events
{
    /// <summary>Represents a method that will handle a content event.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    /// <remarks>This deviates from <see cref="System.EventHandler{T}"/> in allowing <c>T</c> to be an interface instead of a concrete class. While .NET Framework 4.5 allows that, the current .NET Framework 4.0 targeted by SMAPI to improve compatibility does not.</remarks>
    public delegate void ContentEventHandler(object sender, IContentEventHelper e);
}
