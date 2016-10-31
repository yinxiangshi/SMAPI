using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StardewModdingAPI
{
    public static class Log
    {
        private static readonly LogWriter _writer;

        static Log()
        {
            _writer = LogWriter.Instance;
        }

        private static void PrintLog(LogInfo li)
        {
            _writer.WriteToLog(li);
        }

        #region Exception Logging

        /// <summary>
        ///     Catch unhandled exception from the application
        /// </summary>
        /// <remarks>Should be moved out of here if we do more than just log the exception.</remarks>
        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("An exception has been caught");
            File.WriteAllText(Path.Combine(Constants.LogDir, $"MODDED_ErrorLog.Log_{DateTime.UtcNow.Ticks}.txt"), e.ExceptionObject.ToString());
        }

        /// <summary>
        ///     Catch thread exception from the application
        /// </summary>
        /// <remarks>Should be moved out of here if we do more than just log the exception.</remarks>
        public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Console.WriteLine("A thread exception has been caught");
            File.WriteAllText(Path.Combine(Constants.LogDir, $"MODDED_ErrorLog.Log_{Extensions.Random.Next(100000000, 999999999)}.txt"), e.Exception.ToString());
        }

        #endregion

        #region Sync Logging

        /// <summary>
        ///     NOTICE: Sync logging is discouraged. Please use Async instead.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="colour">Colour of message</param>
        public static void SyncColour(object message, ConsoleColor colour)
        {
            PrintLog(new LogInfo(message?.ToString(), colour));
        }

        #endregion

        #region Async Logging

        public static void AsyncColour(object message, ConsoleColor colour)
        {
            Task.Run(() => { PrintLog(new LogInfo(message?.ToString(), colour)); });
        }

        public static void Async(object message)
        {
            AsyncColour(message?.ToString(), ConsoleColor.Gray);
        }

        public static void AsyncR(object message)
        {
            AsyncColour(message?.ToString(), ConsoleColor.Red);
        }

        public static void AsyncO(object message)
        {
            AsyncColour(message.ToString(), ConsoleColor.DarkYellow);
        }

        public static void AsyncY(object message)
        {
            AsyncColour(message?.ToString(), ConsoleColor.Yellow);
        }

        public static void AsyncG(object message)
        {
            AsyncColour(message?.ToString(), ConsoleColor.Green);
        }

        public static void AsyncC(object message)
        {
            AsyncColour(message?.ToString(), ConsoleColor.Cyan);
        }

        public static void AsyncM(object message)
        {
            AsyncColour(message?.ToString(), ConsoleColor.Magenta);
        }

        public static void Error(object message)
        {
            AsyncR("[ERROR] " + message);
        }

        public static void Success(object message)
        {
            AsyncG("[SUCCESS] " + message);
        }

        public static void Info(object message)
        {
            AsyncY("[INFO] " + message);
        }

        public static void Out(object message)
        {
            Async("[OUT] " + message);
        }

        public static void Debug(object message)
        {
            AsyncO("[DEBUG] " + message);
        }

        #endregion

        #region ToRemove

        public static void LogValueNotSpecified()
        {
            AsyncR("<value> must be specified");
        }

        public static void LogObjectValueNotSpecified()
        {
            AsyncR("<object> and <value> must be specified");
        }

        public static void LogValueInvalid()
        {
            AsyncR("<value> is invalid");
        }

        public static void LogObjectInvalid()
        {
            AsyncR("<object> is invalid");
        }

        public static void LogValueNotInt32()
        {
            AsyncR("<value> must be a whole number (Int32)");
        }

        [Obsolete("Parameter 'values' is no longer supported. Format before logging.")]
        private static void PrintLog(object message, bool disableLogging, params object[] values)
        {
            PrintLog(new LogInfo(message?.ToString()));
        }

        [Obsolete("Parameter 'values' is no longer supported. Format before logging.")]
        public static void Success(object message, params object[] values)
        {
            Success(message);
        }

        [Obsolete("Parameter 'values' is no longer supported. Format before logging.")]
        public static void Verbose(object message, params object[] values)
        {
            Out(message);
        }

        [Obsolete("Parameter 'values' is no longer supported. Format before logging.")]
        public static void Comment(object message, params object[] values)
        {
            AsyncC(message);
        }

        [Obsolete("Parameter 'values' is no longer supported. Format before logging.")]
        public static void Info(object message, params object[] values)
        {
            Info(message);
        }

        [Obsolete("Parameter 'values' is no longer supported. Format before logging.")]
        public static void Error(object message, params object[] values)
        {
            Error(message);
        }

        [Obsolete("Parameter 'values' is no longer supported. Format before logging.")]
        public static void Debug(object message, params object[] values)
        {
            Debug(message);
        }

        #endregion
    }
}