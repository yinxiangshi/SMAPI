using System;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised before and after the player saves/loads the game.</summary>
    public static class SaveEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised before the game creates the save file.</summary>
        public static event EventHandler BeforeCreate;

        /// <summary>Raised after the game finishes creating the save file.</summary>
        public static event EventHandler AfterCreate;

        /// <summary>Raised before the game begins writes data to the save file.</summary>
        public static event EventHandler BeforeSave;

        /// <summary>Raised after the game finishes writing data to the save file.</summary>
        public static event EventHandler AfterSave;

        /// <summary>Raised after the player loads a save slot.</summary>
        public static event EventHandler AfterLoad;

        /// <summary>Raised after the game returns to the title screen.</summary>
        public static event EventHandler AfterReturnToTitle;


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="BeforeCreate"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeBeforeCreate(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(SaveEvents)}.{nameof(SaveEvents.BeforeCreate)}", SaveEvents.BeforeCreate?.GetInvocationList(), null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="AfterCreate"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeAfterCreated(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(SaveEvents)}.{nameof(SaveEvents.AfterCreate)}", SaveEvents.AfterCreate?.GetInvocationList(), null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="BeforeSave"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeBeforeSave(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(SaveEvents)}.{nameof(SaveEvents.BeforeSave)}", SaveEvents.BeforeSave?.GetInvocationList(), null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="AfterSave"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeAfterSave(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(SaveEvents)}.{nameof(SaveEvents.AfterSave)}", SaveEvents.AfterSave?.GetInvocationList(), null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="AfterLoad"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeAfterLoad(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(SaveEvents)}.{nameof(SaveEvents.AfterLoad)}", SaveEvents.AfterLoad?.GetInvocationList(), null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="AfterReturnToTitle"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeAfterReturnToTitle(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(SaveEvents)}.{nameof(SaveEvents.AfterReturnToTitle)}", SaveEvents.AfterReturnToTitle?.GetInvocationList(), null, EventArgs.Empty);
        }
    }
}
