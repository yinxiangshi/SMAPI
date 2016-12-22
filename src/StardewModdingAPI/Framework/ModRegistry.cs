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
        private readonly List<IMod> Mods = new List<IMod>();

        /// <summary>The friendly mod names treated as deprecation warning sources (assembly full name => mod name).</summary>
        private readonly IDictionary<string, string> ModNamesByAssembly = new Dictionary<string, string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Register a mod as a possible source of deprecation warnings.</summary>
        /// <param name="mod">The mod instance.</param>
        public void Add(IMod mod)
        {
            this.Mods.Add(mod);
            this.ModNamesByAssembly[mod.GetType().Assembly.FullName] = mod.ModManifest.Name;
        }

        /// <summary>Get all enabled mods.</summary>
        public IEnumerable<IMod> GetMods()
        {
            return (from mod in this.Mods select mod);
        }

        /// <summary>Get the friendly name for the closest assembly registered as a source of deprecation warnings.</summary>
        /// <returns>Returns the source name, or <c>null</c> if no registered assemblies were found.</returns>
        public string GetModFromStack()
        {
            // get stack frames
            StackTrace stack = new StackTrace();
            StackFrame[] frames = stack.GetFrames();
            if (frames == null)
                return null;

            // search stack for a source assembly
            foreach (StackFrame frame in frames)
            {
                // get assembly name
                MethodBase method = frame.GetMethod();
                Type type = method.ReflectedType;
                if (type == null)
                    continue;
                string assemblyName = type.Assembly.FullName;

                // get name if it's a registered source
                if (this.ModNamesByAssembly.ContainsKey(assemblyName))
                    return this.ModNamesByAssembly[assemblyName];
            }

            // no known assembly found
            return null;
        }
    }
}