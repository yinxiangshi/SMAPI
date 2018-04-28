namespace StardewModdingAPI
{
    /// <summary>Provides multiplayer utilities.</summary>
    public interface IMultiplayerHelper : IModLinked
    {
        /// <summary>Get a new multiplayer ID.</summary>
        long GetNewID();
    }
}
