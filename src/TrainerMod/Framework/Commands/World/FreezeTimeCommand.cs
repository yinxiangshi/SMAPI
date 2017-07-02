using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.World
{
    /// <summary>A command which freezes the current time.</summary>
    internal class FreezeTimeCommand : TrainerCommand
    {
        /*********
        ** Properties
        *********/
        /// <summary>The time of day at which to freeze time.</summary>
        internal static int FrozenTime;

        /// <summary>Whether to freeze time.</summary>
        private bool FreezeTime;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the command needs to perform logic when the game updates.</summary>
        public override bool NeedsUpdate => this.FreezeTime;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public FreezeTimeCommand()
            : base("world_freezetime", "Freezes or resumes time.\n\nUsage: world_freezetime [value]\n- value: one of 0 (resume), 1 (freeze), or blank (toggle).") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            if (args.Any())
            {
                if (int.TryParse(args[0], out int value))
                {
                    if (value == 0 || value == 1)
                    {
                        this.FreezeTime = value == 1;
                        FreezeTimeCommand.FrozenTime = Game1.timeOfDay;
                        monitor.Log($"OK, time is now {(this.FreezeTime ? "frozen" : "resumed")}.", LogLevel.Info);
                    }
                    else
                        this.LogUsageError(monitor, "The value should be 0 (not frozen), 1 (frozen), or empty (toggle).", command);
                }
                else
                    this.LogArgumentNotInt(monitor, command);
            }
            else
            {
                this.FreezeTime = !this.FreezeTime;
                FreezeTimeCommand.FrozenTime = Game1.timeOfDay;
                monitor.Log($"OK, time is now {(this.FreezeTime ? "frozen" : "resumed")}.", LogLevel.Info);
            }
        }

        /// <summary>Perform any logic needed on update tick.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        public override void Update(IMonitor monitor)
        {
            if (this.FreezeTime)
                Game1.timeOfDay = FreezeTimeCommand.FrozenTime;
        }
    }
}
