using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace StardewModdingAPI
{
    /// <summary>
    ///     A Logging class implementing the Singleton pattern and an internal Queue to be flushed perdiodically
    /// </summary>
    public class LogWriter
    {
        private static LogWriter _instance;
        private static ConcurrentQueue<LogInfo> _logQueue;
        private static StreamWriter _stream;

        /// <summary>
        ///     Private to prevent creation of other instances
        /// </summary>
        private LogWriter()
        {
        }

        /// <summary>
        ///     Exposes _instace and creates a new one if it is null
        /// </summary>
        internal static LogWriter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogWriter();
                    // Field cannot be used by anything else regardless, do not surround with lock { }
                    // ReSharper disable once InconsistentlySynchronizedField
                    _logQueue = new ConcurrentQueue<LogInfo>();
                    Console.WriteLine(Constants.LogPath);

                    // If the ErrorLogs dir doesn't exist StreamWriter will throw an exception.
                    if (!Directory.Exists(Constants.LogDir))
                    {
                        Directory.CreateDirectory(Constants.LogDir);
                    }

                    _stream = new StreamWriter(Constants.LogPath, false);
                    Console.WriteLine("Created log instance");
                }
                return _instance;
            }
        }

        /// <summary>
        ///     Writes into the ConcurrentQueue the Message specified
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        public void WriteToLog(string message)
        {
            lock (_logQueue)
            {
                var logEntry = new LogInfo(message);
                _logQueue.Enqueue(logEntry);

                if (_logQueue.Any())
                {
                    FlushLog();
                }
            }
        }

        /// <summary>
        ///     Writes into the ConcurrentQueue the Entry specified
        /// </summary>
        /// <param name="logEntry">The logEntry to write to the log</param>
        public void WriteToLog(LogInfo logEntry)
        {
            lock (_logQueue)
            {
                _logQueue.Enqueue(logEntry);

                if (_logQueue.Any())
                {
                    FlushLog();
                }
            }
        }

        /// <summary>
        ///     Flushes the ConcurrentQueue to the log file specified in Constants
        /// </summary>
        private void FlushLog()
        {
            lock (_stream)
            {
                LogInfo entry;
                while (_logQueue.TryDequeue(out entry))
                {
                    string m = $"[{entry.LogTime}] {entry.Message}";

                    Console.ForegroundColor = entry.Colour;
                    Console.WriteLine(m);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    _stream.WriteLine(m);
                }
                _stream.Flush();
            }
        }
    }
}