using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Framework
{
    /// <summary>Manages console commands.</summary>
    internal class CommandManager
    {
        /*********
        ** Properties
        *********/
        /// <summary>The commands registered with SMAPI.</summary>
        private readonly IDictionary<string, Command> Commands = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);


        /*********
        ** Public methods
        *********/
        /// <summary>Add a console command.</summary>
        /// <param name="modName">The friendly mod name for this instance.</param>
        /// <param name="name">The command name, which the user must type to trigger it.</param>
        /// <param name="documentation">The human-readable documentation shown when the player runs the built-in 'help' command.</param>
        /// <param name="callback">The method to invoke when the command is triggered. This method is passed the command name and arguments submitted by the user.</param>
        /// <param name="allowNullCallback">Whether to allow a null <paramref name="callback"/> argument; this should only used for backwards compatibility.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> or <paramref name="callback"/> is null or empty.</exception>
        /// <exception cref="FormatException">The <paramref name="name"/> is not a valid format.</exception>
        /// <exception cref="ArgumentException">There's already a command with that name.</exception>
        public void Add(string modName, string name, string documentation, Action<string, string[]> callback, bool allowNullCallback = false)
        {
            name = this.GetNormalisedName(name);

            // validate format
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Can't register a command with no name.");
            if (name.Any(char.IsWhiteSpace))
                throw new FormatException($"Can't register the '{name}' command because the name can't contain whitespace.");
            if (callback == null && !allowNullCallback)
                throw new ArgumentNullException(nameof(callback), $"Can't register the '{name}' command because without a callback.");

            // ensure uniqueness
            if (this.Commands.ContainsKey(name))
                throw new ArgumentException(nameof(callback), $"Can't register the '{name}' command because there's already a command with that name.");

            // add command
            this.Commands.Add(name, new Command(modName, name, documentation, callback));
        }

        /// <summary>Get a command by its unique name.</summary>
        /// <param name="name">The command name.</param>
        /// <returns>Returns the matching command, or <c>null</c> if not found.</returns>
        public Command Get(string name)
        {
            name = this.GetNormalisedName(name);
            Command command;
            this.Commands.TryGetValue(name, out command);
            return command;
        }

        /// <summary>Get all registered commands.</summary>
        public IEnumerable<Command> GetAll()
        {
            return this.Commands
                .Values
                .OrderBy(p => p.Name);
        }

        /// <summary>Trigger a command.</summary>
        /// <param name="input">The raw command input.</param>
        /// <returns>Returns whether a matching command was triggered.</returns>
        public bool Trigger(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string[] args = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string name = args[0];
            args = args.Skip(1).ToArray();

            return this.Trigger(name, args);
        }

        /// <summary>Trigger a command.</summary>
        /// <param name="name">The command name.</param>
        /// <param name="arguments">The command arguments.</param>
        /// <returns>Returns whether a matching command was triggered.</returns>
        public bool Trigger(string name, string[] arguments)
        {
            // get normalised name
            name = this.GetNormalisedName(name);
            if (name == null)
                return false;

            // get command
            Command command;
            if (this.Commands.TryGetValue(name, out command))
            {
                command.Callback.Invoke(name, arguments);
                return true;
            }
            return false;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get a normalised command name.</summary>
        /// <param name="name">The command name.</param>
        private string GetNormalisedName(string name)
        {
            name = name?.Trim().ToLower();
            return !string.IsNullOrWhiteSpace(name)
                ? name
                : null;
        }
    }
}