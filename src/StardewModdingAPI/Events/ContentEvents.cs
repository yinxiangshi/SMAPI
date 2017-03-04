using System;
using System.Collections.Generic;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the game loads content.</summary>
    [Obsolete("This is an undocumented experimental API and may change without warning.")]
    public static class ContentEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>Tracks the installed mods.</summary>
        private static ModRegistry ModRegistry;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private static IMonitor Monitor;

        /// <summary>The mods using the experimental API for which a warning has been raised.</summary>
        private static readonly HashSet<string> WarnedMods = new HashSet<string>();


        /*********
        ** Events
        *********/
        /// <summary>Raised after the content language changes.</summary>
        public static event EventHandler<EventArgsValueChanged<string>> AfterLocaleChanged;

        /// <summary>Raised when an XNB file is being read into the cache. Mods can change the data here before it's cached.</summary>
        public static event EventHandler<IContentEventHelper> AssetLoading;


        /*********
        ** Internal methods
        *********/
        /// <summary>Injects types required for backwards compatibility.</summary>
        /// <param name="modRegistry">Tracks the installed mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void Shim(ModRegistry modRegistry, IMonitor monitor)
        {
            ContentEvents.ModRegistry = modRegistry;
            ContentEvents.Monitor = monitor;
        }

        /// <summary>Raise an <see cref="AfterLocaleChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="oldLocale">The previous locale.</param>
        /// <param name="newLocale">The current locale.</param>
        internal static void InvokeAfterLocaleChanged(IMonitor monitor, string oldLocale, string newLocale)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ContentEvents)}.{nameof(ContentEvents.AfterLocaleChanged)}", ContentEvents.AfterLocaleChanged?.GetInvocationList(), null, new EventArgsValueChanged<string>(oldLocale, newLocale));
        }

        /// <summary>Raise an <see cref="AssetLoading"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="contentHelper">Encapsulates access and changes to content being read from a data file.</param>
        internal static void InvokeAssetLoading(IMonitor monitor, IContentEventHelper contentHelper)
        {
            // raise warning about experimental API
            foreach (Delegate handler in ContentEvents.AssetLoading.GetInvocationList())
            {
                string modName = ContentEvents.ModRegistry.GetModFrom(handler) ?? "An unknown mod";
                if (!ContentEvents.WarnedMods.Contains(modName))
                {
                    ContentEvents.WarnedMods.Add(modName);
                    ContentEvents.Monitor.Log($"{modName} used the undocumented and experimental content API, which may change or be removed without warning.", LogLevel.Warn);
                }
            }

            // raise event
            monitor.SafelyRaiseGenericEvent($"{nameof(ContentEvents)}.{nameof(ContentEvents.AssetLoading)}", ContentEvents.AssetLoading?.GetInvocationList(), null, contentHelper);
        }

        /// <summary>Get whether there are any <see cref="AssetLoading"/> listeners.</summary>
        internal static bool HasAssetLoadingListeners()
        {
            return ContentEvents.AssetLoading != null;
        }
    }
}
