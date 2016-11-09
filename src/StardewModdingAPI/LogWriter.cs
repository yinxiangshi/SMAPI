using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace StardewModdingAPI
{
    /// <summary>A log writer which queues messages for output, and periodically flushes them to the console and log file.</summary>
    /// <remarks>Only one instance should be created.</remarks>
    public class LogWriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The queued messages to flush.</summary>
        private readonly ConcurrentQueue<LogInfo> Queue;

        /// <summary>The underlying file stream.</summary>
        private readonly StreamWriter FileStream;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="path">The log path to write.</param>
        public LogWriter(string path)
        {
            // create log directory (required for stream writer)
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // initialise
            this.Queue = new ConcurrentQueue<LogInfo>();
            this.FileStream = new StreamWriter(Constants.LogPath, false);
        }

        /// <summary>Queue a message for output.</summary>
        /// <param name="message">The message to log.</param>
        public void WriteToLog(string message)
        {
            lock (this.Queue)
            {
                var logEntry = new LogInfo(message);
                this.Queue.Enqueue(logEntry);

                if (this.Queue.Any())
                    this.FlushLog();
            }
        }

        /// <summary>Queue a message for output.</summary>
        /// <param name="message">The message to log.</param>
        public void WriteToLog(LogInfo message)
        {
            lock (this.Queue)
            {
                this.Queue.Enqueue(message);
                if (this.Queue.Any())
                    this.FlushLog();
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Flush the underlying queue to the console and file.</summary>
        private void FlushLog()
        {
            lock (this.FileStream)
            {
                LogInfo entry;
                while (this.Queue.TryDequeue(out entry))
                {
                    string message = $"[{entry.LogTime}] {entry.Message}";

                    if (entry.PrintConsole)
                    {
                        Console.ForegroundColor = entry.Colour;
                        Console.WriteLine(message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    this.FileStream.WriteLine(message);
                }
                this.FileStream.Flush();
            }
        }
    }
}