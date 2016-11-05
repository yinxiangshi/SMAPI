using System;

namespace StardewModdingAPI
{
    /// <summary>A message queued for log output.</summary>
    public struct LogInfo
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The message to log.</summary>
        public string Message { get; set; }

        /// <summary>The log date.</summary>
        public string LogDate { get; set; }

        /// <summary>The log time.</summary>
        public string LogTime { get; set; }

        /// <summary>The message color.</summary>
        public ConsoleColor Colour { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The message color.</param>
        public LogInfo(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            if (string.IsNullOrEmpty(message))
                message = "[null]";
            this.Message = message;
            this.LogDate = DateTime.Now.ToString("yyyy-MM-dd");
            this.LogTime = DateTime.Now.ToString("hh:mm:ss.fff tt");
            this.Colour = color;
        }
    }
}
