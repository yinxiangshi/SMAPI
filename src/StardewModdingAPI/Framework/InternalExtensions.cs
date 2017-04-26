using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StardewModdingAPI.Framework
{
    /// <summary>Provides extension methods for SMAPI's internal use.</summary>
    internal static class InternalExtensions
    {
        /*********
        ** Properties
        *********/
        /// <summary>Tracks the installed mods.</summary>
        private static ModRegistry ModRegistry;


        /*********
        ** Public methods
        *********/
        /// <summary>Injects types required for backwards compatibility.</summary>
        /// <param name="modRegistry">Tracks the installed mods.</param>
        internal static void Shim(ModRegistry modRegistry)
        {
            InternalExtensions.ModRegistry = modRegistry;
        }

        /****
        ** IMonitor
        ****/
        /// <summary>Safely raise an <see cref="EventHandler"/> event, and intercept any exceptions thrown by its handlers.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="name">The event name for error messages.</param>
        /// <param name="handlers">The event handlers.</param>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments (or <c>null</c> to pass <see cref="EventArgs.Empty"/>).</param>
        public static void SafelyRaisePlainEvent(this IMonitor monitor, string name, IEnumerable<Delegate> handlers, object sender = null, EventArgs args = null)
        {
            if (handlers == null)
                return;

            foreach (EventHandler handler in handlers.Cast<EventHandler>())
            {
                // handle SMAPI exiting
                if (monitor.IsExiting)
                {
                    monitor.Log($"SMAPI shutting down: aborting {name} event.", LogLevel.Warn);
                    return;
                }

                // raise event
                try
                {
                    handler.Invoke(sender, args ?? EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    monitor.Log($"A mod failed handling the {name} event:\n{ex.GetLogSummary()}", LogLevel.Error);
                }
            }
        }

        /// <summary>Safely raise an <see cref="EventHandler{TEventArgs}"/> event, and intercept any exceptions thrown by its handlers.</summary>
        /// <typeparam name="TEventArgs">The event argument object type.</typeparam>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="name">The event name for error messages.</param>
        /// <param name="handlers">The event handlers.</param>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public static void SafelyRaiseGenericEvent<TEventArgs>(this IMonitor monitor, string name, IEnumerable<Delegate> handlers, object sender, TEventArgs args)
        {
            if (handlers == null)
                return;

            foreach (EventHandler<TEventArgs> handler in handlers.Cast<EventHandler<TEventArgs>>())
            {
                try
                {
                    handler.Invoke(sender, args);
                }
                catch (Exception ex)
                {
                    monitor.Log($"A mod failed handling the {name} event:\n{ex.GetLogSummary()}", LogLevel.Error);
                }
            }
        }

        /****
        ** Exceptions
        ****/
        /// <summary>Get a string representation of an exception suitable for writing to the error log.</summary>
        /// <param name="exception">The error to summarise.</param>
        public static string GetLogSummary(this Exception exception)
        {
            // type load exception
            if (exception is TypeLoadException typeLoadEx)
                return $"Failed loading type: {typeLoadEx.TypeName}: {exception}";

            // reflection type load exception
            if (exception is ReflectionTypeLoadException reflectionTypeLoadEx)
            {
                string summary = exception.ToString();
                foreach (Exception childEx in reflectionTypeLoadEx.LoaderExceptions)
                    summary += $"\n\n{childEx.GetLogSummary()}";
                return summary;
            }

            // anything else
            return exception.ToString();
        }

        /****
        ** Deprecation
        ****/
        /// <summary>Log a deprecation warning for mods using an event.</summary>
        /// <param name="deprecationManager">The deprecation manager to extend.</param>
        /// <param name="handlers">The event handlers.</param>
        /// <param name="nounPhrase">A noun phrase describing what is deprecated.</param>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <param name="severity">How deprecated the code is.</param>
        public static void WarnForEvent(this DeprecationManager deprecationManager, Delegate[] handlers, string nounPhrase, string version, DeprecationLevel severity)
        {
            if (handlers == null || !handlers.Any())
                return;

            foreach (Delegate handler in handlers)
            {
                string modName = InternalExtensions.ModRegistry.GetModFrom(handler) ?? "an unknown mod"; // suppress stack trace for unknown mods, not helpful here
                deprecationManager.Warn(modName, nounPhrase, version, severity);
            }
        }
    }
}
