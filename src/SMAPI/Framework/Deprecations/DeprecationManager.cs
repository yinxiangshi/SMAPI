using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace StardewModdingAPI.Framework.Deprecations
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
            var stack = new StackTrace(skipFrames: 1); // skip this method
            this.QueuedWarnings.Add(new DeprecationWarning(source, nounPhrase, version, severity, stack));
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
                    this.Monitor.Log($"{message}\n{this.GetSimplifiedStackTrace(warning.StackTrace, warning.Mod)}", level);
                else
                {
                    this.Monitor.Log(message, level);
                    this.Monitor.Log(this.GetSimplifiedStackTrace(warning.StackTrace, warning.Mod), LogLevel.Debug);
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

        /// <summary>Get the simplest stack trace which shows where in the mod the deprecated code was called from.</summary>
        /// <param name="stack">The stack trace.</param>
        /// <param name="mod">The mod for which to show a stack trace.</param>
        private string GetSimplifiedStackTrace(StackTrace stack, IModMetadata? mod)
        {
            // unknown mod, show entire stack trace
            if (mod == null)
                return stack.ToString();

            // get frame info
            var frames = stack
                .GetFrames()
                .Select(frame => (Frame: frame, Mod: this.ModRegistry.GetFrom(frame)))
                .ToArray();
            var modIds = new HashSet<string>(
                from frame in frames
                let id = frame.Mod?.Manifest.UniqueID
                where id != null
                select id
            );

            // can't filter to the target mod
            if (modIds.Count != 1 || !modIds.Contains(mod.Manifest.UniqueID))
                return stack.ToString();

            // get stack frames for the target mod, plus one for context
            var framesStartingAtMod = frames.SkipWhile(p => p.Mod == null).ToArray();
            var displayFrames = framesStartingAtMod.TakeWhile(p => p.Mod != null).ToArray();
            displayFrames = displayFrames.Concat(framesStartingAtMod.Skip(displayFrames.Length).Take(1)).ToArray();

            // build stack trace
            StringBuilder str = new();
            foreach (var frame in displayFrames)
                str.Append(new StackTrace(frame.Frame));
            return str.ToString().TrimEnd();
        }
    }
}
