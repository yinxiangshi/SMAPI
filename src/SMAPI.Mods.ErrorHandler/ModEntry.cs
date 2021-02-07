using System.Reflection;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Logging;
using StardewModdingAPI.Framework.Patching;
using StardewModdingAPI.Mods.ErrorHandler.Patches;
using StardewValley;

namespace StardewModdingAPI.Mods.ErrorHandler
{
    /// <summary>The main entry point for the mod.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Private methods
        *********/
        /// <summary>Whether custom content was removed from the save data to avoid a crash.</summary>
        private bool IsSaveContentRemoved;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // get SMAPI core types
            IMonitor monitorForGame = this.GetMonitorForGame();

            // apply patches
            new GamePatcher(this.Monitor).Apply(
                new EventErrorPatch(monitorForGame),
                new DialogueErrorPatch(monitorForGame, this.Helper.Reflection),
                new ObjectErrorPatch(),
                new LoadErrorPatch(this.Monitor, this.OnSaveContentRemoved),
                new ScheduleErrorPatch(monitorForGame),
                new UtilityErrorPatches()
            );

            // hook events
            this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after custom content is removed from the save data to avoid a crash.</summary>
        internal void OnSaveContentRemoved()
        {
            this.IsSaveContentRemoved = true;
        }

        /// <summary>The method invoked when a save is loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // show in-game warning for removed save content
            if (this.IsSaveContentRemoved)
            {
                this.IsSaveContentRemoved = false;
                Game1.addHUDMessage(new HUDMessage(this.Helper.Translation.Get("warn.invalid-content-removed"), HUDMessage.error_type));
            }
        }

        /// <summary>Get the monitor with which to log game errors.</summary>
        private IMonitor GetMonitorForGame()
        {
            SCore core = SCore.Instance;
            LogManager logManager = core.GetType().GetField("LogManager", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(core) as LogManager;
            if (logManager == null)
                this.Monitor.Log("Can't access SMAPI's internal log manager. Some game errors may be reported as being from Error Handler.", LogLevel.Error);

            return logManager?.MonitorForGame ?? this.Monitor;
        }
    }
}
