using StardewModdingAPI;

namespace TrainerMod.Framework.Commands
{
    /// <summary>The base implementation for a trainer command.</summary>
    internal abstract class TrainerCommand : ITrainerCommand
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The command name the user must type.</summary>
        public string Name { get; }

        /// <summary>The command description.</summary>
        public string Description { get; }

        /// <summary>Whether the command needs to perform logic when the game updates.</summary>
        public virtual bool NeedsUpdate { get; } = false;


        /*********
        ** Public methods
        *********/
        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public abstract void Handle(IMonitor monitor, string command, string[] args);

        /// <summary>Perform any logic needed on update tick.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        public virtual void Update(IMonitor monitor) { }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The command name the user must type.</param>
        /// <param name="description">The command description.</param>
        protected TrainerCommand(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>Log an error indicating incorrect usage.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="error">A sentence explaining the problem.</param>
        /// <param name="command">The name of the command.</param>
        protected void LogUsageError(IMonitor monitor, string error, string command)
        {
            monitor.Log($"{error} Type 'help {command}' for usage.", LogLevel.Error);
        }

        /// <summary>Log an error indicating a value must be an integer.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The name of the command.</param>
        protected void LogArgumentNotInt(IMonitor monitor, string command)
        {
            this.LogUsageError(monitor, "The value must be a whole number.", command);
        }

        /// <summary>Log an error indicating a value is invalid.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The name of the command.</param>
        protected void LogArgumentsInvalid(IMonitor monitor, string command)
        {
            this.LogUsageError(monitor, "The arguments are invalid.", command);
        }
    }
}
