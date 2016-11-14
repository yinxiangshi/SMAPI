namespace StardewModdingAPI
{
    /// <summary>Encapsulates monitoring and logging for a given module.</summary>
    public interface IMonitor
    {
        /*********
        ** Methods
        *********/
        /// <summary>Log a message for the player or developer.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The log severity level.</param>
        void Log(string message, LogLevel level = LogLevel.Debug);
    }
}
