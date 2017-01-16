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

        /// <summary>Get the friendly mod name which handles a delegate.</summary>
        /// <param name="delegate">The delegate to follow.</param>
        /// <returns>Returns the mod name, or <c>null</c> if the delegate isn't implemented by a known mod.</returns>
        public string GetModFrom(Delegate @delegate)
        {
            return @delegate?.Target != null
                ? this.GetModFrom(@delegate.Target.GetType())
                : null;
        }

        /// <summary>Get the friendly mod name which defines a type.</summary>
        /// <param name="type">The type to check.</param>
        /// <returns>Returns the mod name, or <c>null</c> if the type isn't part of a known mod.</returns>
        public string GetModFrom(Type type)
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
                MethodBase method = frame.GetMethod();
                string name = this.GetModFrom(method.ReflectedType);
                if (name != null)
                    return name;
            }

            // no known assembly found
            return null;
        }
    }
}