using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace StardewModdingAPI.Framework
{
    /// <summary>Tracks the installed mods.</summary>
    internal class ModRegistry
    {
        /*********
        ** Properties
        *********/
        /// <summary>The registered mod data.</summary>
        private readonly List<IModMetadata> Mods = new List<IModMetadata>();

        /// <summary>An assembly full name => mod lookup.</summary>
        private readonly IDictionary<string, IModMetadata> ModNamesByAssembly = new Dictionary<string, IModMetadata>();

        /// <summary>Whether all mods have been initialised and their <see cref="IMod.Entry"/> method called.</summary>
        public bool AreAllModsInitialised { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Register a mod.</summary>
        /// <param name="metadata">The mod metadata.</param>
        public void Add(IModMetadata metadata)
        {
            this.Mods.Add(metadata);
            if (!metadata.IsContentPack)
                this.ModNamesByAssembly[metadata.Mod.GetType().Assembly.FullName] = metadata;
        }

        /// <summary>Get metadata for all loaded mods.</summary>
        /// <param name="assemblyMods">Whether to include SMAPI mods.</param>
        /// <param name="contentPacks">Whether to include content pack mods.</param>
        public IEnumerable<IModMetadata> GetAll(bool assemblyMods = true, bool contentPacks = true)
        {
            IEnumerable<IModMetadata> query = this.Mods;
            if (!assemblyMods)
                query = query.Where(p => p.IsContentPack);
            if (!contentPacks)
                query = query.Where(p => !p.IsContentPack);

            return query;
        }

        /// <summary>Get metadata for a loaded mod.</summary>
        /// <param name="uniqueID">The mod's unique ID.</param>
        /// <returns>Returns the matching mod's metadata, or <c>null</c> if not found.</returns>
        public IModMetadata Get(string uniqueID)
        {
            // normalise search ID
            if (string.IsNullOrWhiteSpace(uniqueID))
                return null;
            uniqueID = uniqueID.Trim();

            // find match
            return this.GetAll().FirstOrDefault(p => p.Manifest.UniqueID.Trim().Equals(uniqueID, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>Get the mod metadata from one of its assemblies.</summary>
        /// <param name="type">The type to check.</param>
        /// <returns>Returns the mod name, or <c>null</c> if the type isn't part of a known mod.</returns>
        public IModMetadata GetFrom(Type type)
        {
            // null
            if (type == null)
                return null;

            // known type
            string assemblyName = type.Assembly.FullName;
            if (this.ModNamesByAssembly.ContainsKey(assemblyName))
                return this.ModNamesByAssembly[assemblyName];

            // not found
            return null;
        }

        /// <summary>Get the friendly name for the closest assembly registered as a source of deprecation warnings.</summary>
        /// <returns>Returns the source name, or <c>null</c> if no registered assemblies were found.</returns>
        public IModMetadata GetFromStack()
        {
            // get stack frames
            StackTrace stack = new StackTrace();
            StackFrame[] frames = stack.GetFrames();
            if (frames == null)
                return null;

            // search stack for a source assembly
            foreach (StackFrame frame in frames)
            {
                MethodBase method = frame.GetMethod();
                IModMetadata mod = this.GetFrom(method.ReflectedType);
                if (mod != null)
                    return mod;
            }

            // no known assembly found
            return null;
        }
    }
}
