using System;
using System.Threading;
using StardewModdingAPI.Framework;
using Monitor = StardewModdingAPI.Framework.Monitor;

namespace StardewModdingAPI
{
    /// <summary>A singleton which logs messages to the SMAPI console and log file.</summary>
    [Obsolete("Use " + nameof(Mod) + "." + nameof(Mod.Monitor))]
    public static class Log
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying logger.</summary>
        internal static Monitor Monitor;

        /// <summary>Tracks the installed mods.</summary>
        internal static ModRegistry ModRegistry;

        /// <summary>A temporary field to avoid infinite loops (since we use a deprecated interface to warn about the deprecated interface).</summary>
        private static bool WarnedDeprecated;


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
            Log.WarnDeprecated();
            Log.Monitor.Log($"Critical app domain exception: {e.ExceptionObject}", LogLevel.Error);
        }

        /// <summary>Log a thread exception event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.WarnDeprecated();
            Log.Monitor.Log($"Critical thread exception: {e.Exception}", LogLevel.Error);
        }

        /****
        ** Synchronous logging
        ****/
        /// <summary>Synchronously log a message to the console. NOTE: synchronous logging is discouraged; use asynchronous methods instead.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The message color.</param>
        public static void SyncColour(object message, ConsoleColor color)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), color);
        }

        /****
        ** Asynchronous logging
        ****/
        /// <summary>Asynchronously log a message to the console with the specified color.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The message color.</param>
        public static void AsyncColour(object message, ConsoleColor color)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), color);
        }

        /// <summary>Asynchronously log a message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Async(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.Gray);
        }

        /// <summary>Asynchronously log a red message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncR(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.Red);
        }

        /// <summary>Asynchronously log an orange message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncO(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.DarkYellow);
        }

        /// <summary>Asynchronously log a yellow message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncY(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.Yellow);
        }

        /// <summary>Asynchronously log a green message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncG(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.Green);
        }

        /// <summary>Asynchronously log a cyan message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncC(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.Cyan);
        }

        /// <summary>Asynchronously log a magenta message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void AsyncM(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.Magenta);
        }

        /// <summary>Asynchronously log a warning to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Warning(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.Yellow, LogLevel.Warn);
        }

        /// <summary>Asynchronously log an error to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Error(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.Red, LogLevel.Error);
        }

        /// <summary>Asynchronously log a success message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Success(object message)
        {
            Log.WarnDeprecated();
            Log.AsyncG(message);
        }

        /// <summary>Asynchronously log an info message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Info(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.White, LogLevel.Info);
        }

        /// <summary>Asynchronously log a debug message to the console.</summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(object message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message?.ToString(), ConsoleColor.DarkGray);
        }

        /// <summary>Asynchronously log a message to the file that's not shown in the console.</summary>
        /// <param name="message">The message to log.</param>
        internal static void LogToFile(string message)
        {
            Log.WarnDeprecated();
            Log.Monitor.LegacyLog(Log.GetModName(), message, ConsoleColor.DarkGray, LogLevel.Trace);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raise a deprecation warning.</summary>
        private static void WarnDeprecated()
        {
            if (!Log.WarnedDeprecated)
            {
                Log.WarnedDeprecated = true;
                Program.DeprecationManager.Warn($"the {nameof(Log)} class", "1.1", DeprecationLevel.Notice);
            }
        }

        /// <summary>Get the name of the mod logging a message from the stack.</summary>
        private static string GetModName()
        {
            return Log.ModRegistry.GetModFromStack() ?? "<unknown mod>";
        }
    }
}