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
        /// <summary>Raised during launch after configuring XNA or MonoGame. The game window hasn't been opened by this point. Called during <see cref="Microsoft.Xna.Framework.Game.Initialize"/>.</summary>
        public static event EventHandler Initialize;

        /// <summary>Raised during launch after configuring Stardew Valley, loading it into memory, and opening the game window. The window is still blank by this point.</summary>
        public static event EventHandler GameLoaded;

        /// <summary>Raised before XNA loads or reloads graphics resources. Called during <see cref="Microsoft.Xna.Framework.Game.LoadContent"/>.</summary>
        public static event EventHandler LoadContent;

        /// <summary>Raised during the first game update tick.</summary>
        public static event EventHandler FirstUpdateTick;

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
        /// <summary>Raise a <see cref="GameLoaded"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeGameLoaded(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.GameLoaded)}", GameEvents.GameLoaded?.GetInvocationList());
        }

        /// <summary>Raise an <see cref="Initialize"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        internal static void InvokeInitialize(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.Initialize)}", GameEvents.Initialize?.GetInvocationList());
        }

        /// <summary>Raise a <see cref="LoadContent"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        internal static void InvokeLoadContent(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.LoadContent)}", GameEvents.LoadContent?.GetInvocationList());
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

        /// <summary>Raise a <see cref="FirstUpdateTick"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeFirstUpdateTick(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GameEvents)}.{nameof(GameEvents.FirstUpdateTick)}", GameEvents.FirstUpdateTick?.GetInvocationList());
        }
    }
}
