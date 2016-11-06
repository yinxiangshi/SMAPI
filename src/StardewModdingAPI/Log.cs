using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI
{
    /// <summary>A singleton which logs messages to the SMAPI console and log file.</summary>
    public static class Log
    {
        /*********
        ** Properties
        *********/
        /// <summary>A pseudorandom number generator used to generate log files.</summary>
        private static readonly Random Random = new Random();

        /// <summary>The underlying log writer.</summary>
        private static readonly LogWriter Writer = new LogWriter(Constants.LogPath);


        /*********
        ** Public methods
        *********/
        /****
        ** Exceptions
        ****/
        /// <summary>Log an exception event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("An exception has been caught");
            File.WriteAllText(Path.Combine(Constants.LogDir, $"MODDED_ErrorLog.Log_{DateTime.UtcNow.Ticks}.txt"), e.ExceptionObject.ToString());
        }

        /// <summary>Log a thread exception event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Console.WriteLine("A thread exception has been caught");
            File.WriteAllText(Path.Combine(Constants.LogDir, $"MODDED_ErrorLog.Log_{Log.Random.Next(100000000, 999999999)}.txt"), e.Exception.ToString());
        }

        /****
        ** Synchronous logging
        ****/
        /// <summary>Synchronously log a message to the console. NOTE: synchronous logging is discouraged; use asynchronous methods instead.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The message color.</param>
        public static void SyncColour(object message, ConsoleColor color)
        {
            Log.PrintLog(new LogInfo(message?.ToString(), color));
        }

        /****
        ** Asynchronous logging
        ****/
        /// <summary>Asynchronously log a message to the console with the specified color.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The message color.</param>
        public static void AsyncColour(object message, ConsoleColor color)
        {
            Task.Run(() => { Log.PrintLog(new LogInfo(message?.ToString(), color)); });
        }

        /// <summary>Asynchronously log a message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Async(object message)
        {
            Log.AsyncColour(message?.ToString(), ConsoleColor.Gray);
        }

        /// <summary>Asynchronously log a red message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncR(object message)
        {
            Log.AsyncColour(message?.ToString(), ConsoleColor.Red);
        }

        /// <summary>Asynchronously log an orange message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncO(object message)
        {
            Log.AsyncColour(message.ToString(), ConsoleColor.DarkYellow);
        }

        /// <summary>Asynchronously log a yellow message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncY(object message)
        {
            Log.AsyncColour(message?.ToString(), ConsoleColor.Yellow);
        }

        /// <summary>Asynchronously log a green message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncG(object message)
        {
            Log.AsyncColour(message?.ToString(), ConsoleColor.Green);
        }

        /// <summary>Asynchronously log a cyan message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncC(object message)
        {
            Log.AsyncColour(message?.ToString(), ConsoleColor.Cyan);
        }

        /// <summary>Asynchronously log a magenta message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncM(object message)
        {
            Log.AsyncColour(message?.ToString(), ConsoleColor.Magenta);
        }

        /// <summary>Asynchronously log a warning to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Warning(object message)
        {
            Log.AsyncColour("[WARN] " + message, ConsoleColor.DarkRed);
        }

        /// <summary>Asynchronously log an error to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Error(object message)
        {
            Log.AsyncR("[ERROR] " + message);
        }

        /// <summary>Asynchronously log a success message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Success(object message)
        {
            Log.AsyncG(message);
        }

        /// <summary>Asynchronously log an info message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Info(object message)
        {
            Log.AsyncY(message);
        }

        /// <summary>Asynchronously log a debug message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(object message)
        {
            Log.AsyncColour(message, ConsoleColor.DarkGray);
        }

        /// <summary>Asynchronously log a message to the file that's not shown in the console.</summary>
        /// <param name="message">The message to log.</param>
        internal static void LogToFile(string message)
        {
            Task.Run(() => { Log.PrintLog(new LogInfo(message) { PrintConsole = false }); });
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Write a message to the log.</summary>
        /// <param name="message">The message to write.</param>
        private static void PrintLog(LogInfo message)
        {
            Log.Writer.WriteToLog(message);
        }
    }
}