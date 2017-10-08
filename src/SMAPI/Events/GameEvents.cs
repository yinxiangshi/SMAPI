using System;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the game changes state.</summary>
    public static class GameEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised during launch after configuring XNA or MonoGame. The game window hasn't been opened by this point. Called after <see cref="Microsoft.Xna.Framework.Game.Initialize"/>.</summary>
        internal static event EventHandler InitializeInternal;

        /// <summary>Raised when the game updates its state (≈60 times per second).</summary>
        public static event EventHandler UpdateTick;

        /// <summary>Raised every other tick (≈30 times per second).</summary>
        public static event EventHandler SecondUpdateTick;

        /// <summary>Raised every fourth tick (≈15 times per second).</summary>
        public static event EventHandler FourthUpdateTick;

        /// <summary>Raised every eighth tick (≈8 times per second).</summary>
        public static event EventHandler EighthUpdateTick;

        /// <summary>Raised every 15th tick (≈4 times per second).</summary>
        public static event EventHandler QuarterSecondTick;

        /// <summary>Raised every 30th tick (≈twice per second).</summary>
        public static event EventHandler HalfSecondTick;

        /// <summary>Raised every 60th tick (≈once per second).</summary>
        public static event EventHandler OneSecondTick;


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise an <see cref="InitializeInternal"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        internal static void InvokeInitialize(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.InitializeInternal)}", GameEvents.InitializeInternal?.GetInvocationList());
        }

        /// <summary>Raise an <see cref="UpdateTick"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        internal static void InvokeUpdateTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.UpdateTick)}", GameEvents.UpdateTick?.GetInvocationList());
        }

        /// <summary>Raise a <see cref="SecondUpdateTick"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeSecondUpdateTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.SecondUpdateTick)}", GameEvents.SecondUpdateTick?.GetInvocationList());
        }

        /// <summary>Raise a <see cref="FourthUpdateTick"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeFourthUpdateTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.FourthUpdateTick)}", GameEvents.FourthUpdateTick?.GetInvocationList());
        }

        /// <summary>Raise a <see cref="EighthUpdateTick"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeEighthUpdateTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.EighthUpdateTick)}", GameEvents.EighthUpdateTick?.GetInvocationList());
        }

        /// <summary>Raise a <see cref="QuarterSecondTick"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeQuarterSecondTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.QuarterSecondTick)}", GameEvents.QuarterSecondTick?.GetInvocationList());
        }

        /// <summary>Raise a <see cref="HalfSecondTick"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeHalfSecondTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.HalfSecondTick)}", GameEvents.HalfSecondTick?.GetInvocationList());
        }

        /// <summary>Raise a <see cref="OneSecondTick"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeOneSecondTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.OneSecondTick)}", GameEvents.OneSecondTick?.GetInvocationList());
        }
    }
}
