using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI
{
    /// <summary>A command that can be submitted through the SMAPI console to interact with SMAPI.</summary>
    public class Command
    {
        /*********
        ** Properties
        *********/
        /****
        ** SMAPI
        ****/
        /// <summary>The commands registered with SMAPI.</summary>
        internal static List<Command> RegisteredCommands = new List<Command>();

        /// <summary>The event raised when this command is submitted through the console.</summary>
        public event EventHandler<EventArgsCommand> CommandFired;

        /****
        ** Command
        ****/
        /// <summary>The name of the command.</summary>
        public string CommandName;

        /// <summary>A human-readable description of what the command does.</summary>
        public string CommandDesc;

        /// <summary>A human-readable list of accepted arguments.</summary>
        public string[] CommandArgs;

        /// <summary>The actual submitted argument values.</summary>
        public string[] CalledArgs;


        /*********
        ** Public methods
        *********/
        /****
        ** Command
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">A human-readable description of what the command does.</param>
        /// <param name="args">A human-readable list of accepted arguments.</param>
        public Command(string name, string description, string[] args = null)
        {
            this.CommandName = name;
            this.CommandDesc = description;
            if (args == null)
                args = new string[0];
            this.CommandArgs = args;
        }

        /// <summary>Trigger this command.</summary>
        public void Fire()
        {
            if (this.CommandFired == null)
                throw new InvalidOperationException($"Can't run command '{this.CommandName}' because it has no registered handler.");
            this.CommandFired.Invoke(this, new EventArgsCommand(this));
        }

        /****
        ** SMAPI
        ****/
        /// <summary>Parse a command string and invoke it if valid.</summary>
        /// <param name="input">The command to run, including the command name and any arguments.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public static void CallCommand(string input, IMonitor monitor)
        {
            // normalise input
            input = input?.Trim();
            if (string.IsNullOrWhiteSpace(input))
                return;

            // tokenise input
            string[] args = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string commandName = args[0];
            args = args.Skip(1).ToArray();

            // get command
            Command command = Command.FindCommand(commandName);
            if (command == null)
            {
                monitor.Log("Unknown command", LogLevel.Error);
                return;
            }

            // fire command
            command.CalledArgs = args;
            command.Fire();
        }

        /// <summary>Register a command with SMAPI.</summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">A human-readable description of what the command does.</param>
        /// <param name="args">A human-readable list of accepted arguments.</param>
        public static Command RegisterCommand(string name, string description, string[] args = null)
        {
            var command = new Command(name, description, args);
            if (Command.RegisteredCommands.Contains(command))
                throw new InvalidOperationException($"The '{command.CommandName}' command is already registered!");

            Command.RegisteredCommands.Add(command);
            return command;
        }

        /// <summary>Find a command with the given name.</summary>
        /// <param name="name">The command name to find.</param>
        public static Command FindCommand(string name)
        {
            return Command.RegisteredCommands.Find(x => x.CommandName.Equals(name));
        }
    }
}
