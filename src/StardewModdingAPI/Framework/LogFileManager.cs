using System;
using System.IO;

namespace StardewModdingAPI.Framework
{
    /// <summary>Manages reading and writing to log file.</summary>
    internal class LogFileManager : IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying stream writer.</summary>
        private readonly StreamWriter Stream;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="path">The log file to write.</param>
        public LogFileManager(string path)
        {
            // create log directory if needed
            string logDir = Path.GetDirectoryName(path);
            if (logDir == null)
                throw new ArgumentException($"The log path '{path}' is not valid.");
            Directory.CreateDirectory(logDir);

            // open log file stream
            this.Stream = new StreamWriter(path, append: false) { AutoFlush = true };
        }

        /// <summary>Write a message to the log.</summary>
        /// <param name="message">The message to log.</param>
        public void WriteLine(string message)
        {
            this.Stream.WriteLine(message);
        }

        /// <summary>Release all resources.</summary>
        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }
}