using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Framework
{
    /// <summary>Provides extension methods for SMAPI's internal use.</summary>
    internal static class InternalExtensions
    {
        /*********
        ** Public methods
        *********/
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

            foreach (EventHandler handler in Enumerable.Cast<EventHandler>(handlers))
            {
                try
                {
                    handler.Invoke(sender, args ?? EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    monitor.Log($"A mod failed handling the {name} event:\n{ex}", LogLevel.Error);
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

            foreach (EventHandler<TEventArgs> handler in Enumerable.Cast<EventHandler<TEventArgs>>(handlers))
            {
                try
                {
                    handler.Invoke(sender, args);
                }
                catch (Exception ex)
                {
                    monitor.Log($"A mod failed handling the {name} event:\n{ex}", LogLevel.Error);
                }
            }
        }
    }
}
