using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace StardewModdingAPI.Framework
{
    /// <summary>A log writer which queues messages for output, and periodically flushes them to the console and log file.</summary>
    internal class LogWriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The singleton instance.</summary>
        private static LogWriter _instance;

        /// <summary>The queued messages to flush.</summary>
        private static ConcurrentQueue<LogInfo> Queue;

        /// <summary>The underlying file stream.</summary>
        private static StreamWriter FileStream;


        /*********
        ** Accessors
        *********/
        /// <summary>The singleton instance.</summary>
        internal static LogWriter Instance
        {
            get
            {
                if (LogWriter._instance == null)
                {
                    LogWriter._instance = new LogWriter();
                    // Field cannot be used by anything else regardless, do not surround with lock { }
                    // ReSharper disable once InconsistentlySynchronizedField
                    LogWriter.Queue = new ConcurrentQueue<LogInfo>();
                    Console.WriteLine(Constants.LogPath);

                    // If the ErrorLogs dir doesn't exist StreamWriter will throw an exception.
                    if (!Directory.Exists(Constants.LogDir))
                    {
                        Directory.CreateDirectory(Constants.LogDir);
                    }

                    LogWriter.FileStream = new StreamWriter(Constants.LogPath, false);
                    Console.WriteLine("Created log instance");
                }
                return LogWriter._instance;
            }
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Queue a message for output.</summary>
        /// <param name="message">The message to log.</param>
        public void WriteToLog(string message)
        {
            lock (LogWriter.Queue)
            {
                var logEntry = new LogInfo(message);
                LogWriter.Queue.Enqueue(logEntry);

                if (LogWriter.Queue.Any())
                    this.FlushLog();
            }
        }

        /// <summary>Queue a message for output.</summary>
        /// <param name="message">The message to log.</param>
        public void WriteToLog(LogInfo message)
        {
            lock (LogWriter.Queue)
            {
                LogWriter.Queue.Enqueue(message);
                if (LogWriter.Queue.Any())
                    this.FlushLog();
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Construct an instance.</summary>
        private LogWriter() { }

        /// <summary>Flush the underlying queue to the console and file.</summary>
        private void FlushLog()
        {
            lock (LogWriter.FileStream)
            {
                LogInfo entry;
                while (LogWriter.Queue.TryDequeue(out entry))
                {
                    string m = $"[{entry.LogTime}] {entry.Message}";

                    Console.ForegroundColor = entry.Colour;
                    Console.WriteLine(m);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    LogWriter.FileStream.WriteLine(m);
                }
                LogWriter.FileStream.Flush();
            }
        }
    }
}