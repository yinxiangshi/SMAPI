using System.Diagnostics;

namespace StardewModdingAPI.Framework.Deprecations
{
    /// <summary>A deprecation warning for a mod.</summary>
    internal class DeprecationWarning
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The affected mod.</summary>
        public IModMetadata? Mod { get; }

        /// <summary>Get the display name for the affected mod.</summary>
        public string ModName => this.Mod?.DisplayName ?? "<unknown mod>";

        /// <summary>A noun phrase describing what is deprecated.</summary>
        public string NounPhrase { get; }

        /// <summary>The SMAPI version which deprecated it.</summary>
        public string Version { get; }

        /// <summary>The deprecation level for the affected code.</summary>
        public DeprecationLevel Level { get; }

        /// <summary>The stack trace when the deprecation warning was raised.</summary>
        public ImmutableStackTrace StackTrace { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The affected mod.</param>
        /// <param name="nounPhrase">A noun phrase describing what is deprecated.</param>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <param name="level">The deprecation level for the affected code.</param>
        /// <param name="stackTrace">The stack trace when the deprecation warning was raised.</param>
        public DeprecationWarning(IModMetadata? mod, string nounPhrase, string version, DeprecationLevel level, ImmutableStackTrace stackTrace)
        {
            this.Mod = mod;
            this.NounPhrase = nounPhrase;
            this.Version = version;
            this.Level = level;
            this.StackTrace = stackTrace;
        }
    }
}
