using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StardewModdingAPI.Framework
{
    /// <summary>Manages deprecation warnings.</summary>
    internal class DeprecationManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>The deprecations which have already been logged (as 'mod name::noun phrase::version').</summary>
        private readonly HashSet<string> LoggedDeprecations = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Encapsulates monitoring and logging for a given module.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Tracks the installed mods.</summary>
        private readonly ModRegistry ModRegistry;

        /// <summary>The queued deprecation warnings to display.</summary>
        private readonly IList<DeprecationWarning> QueuedWarnings = new List<DeprecationWarning>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging for a given module.</param>
        /// <param name="modRegistry">Tracks the installed mods.</param>
        public DeprecationManager(IMonitor monitor, ModRegistry modRegistry)
        {
            this.Monitor = monitor;
            this.ModRegistry = modRegistry;
        }

        /// <summary>Get a mod for the closest assembly registered as a source of deprecation warnings.</summary>
        /// <returns>Returns the source name, or <c>null</c> if no registered assemblies were found.</returns>
        public IModMetadata? GetModFromStack()
        {
            return this.ModRegistry.GetFromStack();
        }

        /// <summary>Get a mod from its unique ID.</summary>
        /// <param name="modId">The mod's unique ID.</param>
        public IModMetadata? GetMod(string modId)
        {
            return this.ModRegistry.Get(modId);
        }

        /// <summary>Log a deprecation warning.</summary>
        /// <param name="source">The mod which used the deprecated code, if known.</param>
        /// <param name="nounPhrase">A noun phrase describing what is deprecated.</param>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <param name="severity">How deprecated the code is.</param>
        public void Warn(IModMetadata? source, string nounPhrase, string version, DeprecationLevel severity)
        {
            // ignore if already warned
            if (!this.MarkWarned(source, nounPhrase, version))
                return;

            // queue warning
            this.QueuedWarnings.Add(new DeprecationWarning(source, nounPhrase, version, severity, new StackTrace(skipFrames: 1)));
        }

        /// <summary>A placeholder method used to track deprecated code for which a separate warning will be shown.</summary>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <param name="severity">How deprecated the code is.</param>
        public void PlaceholderWarn(string version, DeprecationLevel severity) { }

        /// <summary>Print any queued messages.</summary>
        public void PrintQueued()
        {
            foreach (DeprecationWarning warning in this.QueuedWarnings.OrderBy(p => p.ModName).ThenBy(p => p.NounPhrase))
            {
                // build message
                string message = $"{warning.ModName} uses deprecated code ({warning.NounPhrase} is deprecated since SMAPI {warning.Version}).";

                // get log level
                LogLevel level;
                switch (warning.Level)
                {
                    case DeprecationLevel.Notice:
                        level = LogLevel.Trace;
                        break;

                    case DeprecationLevel.Info:
                        level = LogLevel.Debug;
                        break;

                    case DeprecationLevel.PendingRemoval:
                        level = LogLevel.Warn;
                        break;

                    default:
                        throw new NotSupportedException($"Unknown deprecation level '{warning.Level}'.");
                }

                // log message
                if (level == LogLevel.Trace)
                    this.Monitor.Log($"{message}\n{warning.StackTrace}", level);
                else
                {
                    this.Monitor.Log(message, level);
                    this.Monitor.Log(warning.StackTrace.ToString(), LogLevel.Debug);
                }
            }

            this.QueuedWarnings.Clear();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Mark a deprecation warning as already logged.</summary>
        /// <param name="source">The mod which used the deprecated code.</param>
        /// <param name="nounPhrase">A noun phrase describing what is deprecated (e.g. "the Extensions.AsInt32 method").</param>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <returns>Returns whether the deprecation was successfully marked as warned. Returns <c>false</c> if it was already marked.</returns>
        private bool MarkWarned(IModMetadata? source, string nounPhrase, string version)
        {
            string key = $"{source?.DisplayName ?? "<unknown>"}::{nounPhrase}::{version}";
            if (this.LoggedDeprecations.Contains(key))
                return false;
            this.LoggedDeprecations.Add(key);
            return true;
        }
    }
}
