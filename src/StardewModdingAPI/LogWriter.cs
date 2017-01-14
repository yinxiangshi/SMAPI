using System;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI
{
    /// <summary>A log writer which queues messages for output, and periodically flushes them to the console and log file.</summary>
    /// <remarks>Only one instance should be created.</remarks>
    [Obsolete("This class is internal and should not be referenced outside SMAPI. It will no longer be exposed in a future version.")]
    public class LogWriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>Manages reading and writing to the log file.</summary>
        private readonly LogFileManager LogFile;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logFile">Manages reading and writing to the log file.</param>
        internal LogWriter(LogFileManager logFile)
        {
            this.WarnDeprecated();
            this.LogFile = logFile;
        }

        /// <summary>Queue a message for output.</summary>
        /// <param name="message">The message to log.</param>
        public void WriteToLog(string message)
        {
            this.WarnDeprecated();
            this.WriteToLog(new LogInfo(message));
        }

        /// <summary>Queue a message for output.</summary>
        /// <param name="message">The message to log.</param>
        public void WriteToLog(LogInfo message)
        {
            this.WarnDeprecated();
            string output = $"[{message.LogTime}] {message.Message}";
            if (message.PrintConsole)
            {
                if (Monitor.ConsoleSupportsColor)
                {
                    Console.ForegroundColor = message.Colour;
                    Console.WriteLine(message);
                    Console.ResetColor();
                }
                else
                    Console.WriteLine(message);
            }
            this.LogFile.WriteLine(output);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Raise a deprecation warning.</summary>
        private void WarnDeprecated()
        {
            Program.DeprecationManager.Warn($"the {nameof(LogWriter)} class", "1.0", DeprecationLevel.Info);
        }
    }
}