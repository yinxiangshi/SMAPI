using System;

namespace StardewModdingAPI
{
    /// <summary>
    ///     A struct to store the message and the Date and Time the log entry was created
    /// </summary>
    public struct LogInfo
    {
        public string Message { get; set; }
        public string LogTime { get; set; }
        public string LogDate { get; set; }
        public ConsoleColor Colour { get; set; }

        public LogInfo(string message, ConsoleColor colour = ConsoleColor.Gray)
        {
            if (string.IsNullOrEmpty(message))
                message = "[null]";
            Message = message;
            LogDate = DateTime.Now.ToString("yyyy-MM-dd");
            LogTime = DateTime.Now.ToString("hh:mm:ss.fff tt");
            Colour = colour;
        }
    }
}