using System;
using System.Collections.Generic;
using StardewModdingAPI.Events;

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
            {
                Log.AsyncR($"Command failed to fire because it's fire event is null: {this.CommandName}");
                return;
            }
            this.CommandFired.Invoke(this, new EventArgsCommand(this));
        }

        /****
        ** SMAPI
        ****/
        /// <summary>Invoke the specified command.</summary>
        /// <param name="input">The command to run, including the command name and any arguments.</param>
        public static void CallCommand(string input)
        {
            input = input.TrimEnd(' ');
            string[] args = new string[0];
            Command command;
            if (input.Contains(" "))
            {
                args = input.Split(new[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
                command = Command.FindCommand(args[0]);
                args = args[1].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
                command = Command.FindCommand(input);

            if (command != null)
            {
                command.CalledArgs = args;
                command.Fire();
            }
            else
                Log.AsyncR("Unknown command");
        }

        /// <summary>Register a command with SMAPI.</summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">A human-readable description of what the command does.</param>
        /// <param name="args">A human-readable list of accepted arguments.</param>
        public static Command RegisterCommand(string name, string description, string[] args = null)
        {
            var command = new Command(name, description, args);
            if (Command.RegisteredCommands.Contains(command))
            {
                Log.AsyncR($"Command already registered! [{command.CommandName}]");
                return Command.RegisteredCommands.Find(x => x.Equals(command));
            }

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