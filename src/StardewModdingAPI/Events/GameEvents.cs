using System;

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
        internal static void InvokeGameLoaded()
        {
            GameEvents.GameLoaded?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Raise an <see cref="Initialize"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        internal static void InvokeInitialize(IMonitor monitor)
        {
            try
            {
                GameEvents.Initialize?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                monitor.Log($"A mod crashed handling an event.\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>Raise a <see cref="LoadContent"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        internal static void InvokeLoadContent(IMonitor monitor)
        {
            try
            {
                GameEvents.LoadContent?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                monitor.Log($"A mod crashed handling an event.\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>Raise an <see cref="UpdateTick"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        internal static void InvokeUpdateTick(IMonitor monitor)
        {
            try
            {
                GameEvents.UpdateTick?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                monitor.Log($"A mod crashed handling an event.\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>Raise a <see cref="SecondUpdateTick"/> event.</summary>
        internal static void InvokeSecondUpdateTick()
        {
            GameEvents.SecondUpdateTick?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="FourthUpdateTick"/> event.</summary>
        internal static void InvokeFourthUpdateTick()
        {
            GameEvents.FourthUpdateTick?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="EighthUpdateTick"/> event.</summary>
        internal static void InvokeEighthUpdateTick()
        {
            GameEvents.EighthUpdateTick?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="QuarterSecondTick"/> event.</summary>
        internal static void InvokeQuarterSecondTick()
        {
            GameEvents.QuarterSecondTick?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="HalfSecondTick"/> event.</summary>
        internal static void InvokeHalfSecondTick()
        {
            GameEvents.HalfSecondTick?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="OneSecondTick"/> event.</summary>
        internal static void InvokeOneSecondTick()
        {
            GameEvents.OneSecondTick?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="FirstUpdateTick"/> event.</summary>
        internal static void InvokeFirstUpdateTick()
        {
            GameEvents.FirstUpdateTick?.Invoke(null, EventArgs.Empty);
        }
    }
}
