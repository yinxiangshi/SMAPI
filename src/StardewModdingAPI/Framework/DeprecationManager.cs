using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace StardewModdingAPI.Framework
{
    /// <summary>Manages deprecation warnings.</summary>
    internal class DeprecationManager
    {
        /*********
        ** Properties
        *********/
        /// <summary>The friendly mod names treated as deprecation warning sources (assembly full name => mod name).</summary>
        private readonly IDictionary<string, string> ModNamesByAssembly = new Dictionary<string, string>();

        /// <summary>The deprecations which have already been logged (as 'mod name::noun phrase::version').</summary>
        private readonly HashSet<string> LoggedDeprecations = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);


        /*********
        ** Accessors
        *********/
        /// <summary>Whether <see cref="DeprecationLevel.Notice"/>-level deprecation messages should be shown in the console.</summary>
        public bool SendNoticesToConsole { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Register a mod as a possible source of deprecation warnings.</summary>
        /// <param name="assembly">The mod assembly.</param>
        /// <param name="name">The mod's friendly name.</param>
        public void AddMod(Assembly assembly, string name)
        {
            this.ModNamesByAssembly[assembly.FullName] = name;
        }

        /// <summary>Log a deprecation warning.</summary>
        /// <param name="nounPhrase">A noun phrase describing what is deprecated.</param>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <param name="severity">How deprecated the code is.</param>
        public void Warn(string nounPhrase, string version, DeprecationLevel severity)
        {
            this.Warn(this.GetSourceNameFromStack(), nounPhrase, version, severity);
        }

        /// <summary>Log a deprecation warning.</summary>
        /// <param name="source">The friendly mod name which used the deprecated code.</param>
        /// <param name="nounPhrase">A noun phrase describing what is deprecated.</param>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <param name="severity">How deprecated the code is.</param>
        public void Warn(string source, string nounPhrase, string version, DeprecationLevel severity)
        {
            // ignore if already warned
            if (source != null && !this.MarkWarned(source, nounPhrase, version))
                return;

            // build message
            string message = source != null
                ? $"{source} used {nounPhrase}, which is deprecated since SMAPI {version}."
                : $"An unknown mod used {nounPhrase}, which is deprecated since SMAPI {version}.";
            message += severity != DeprecationLevel.PendingRemoval
                ? " It will work fine for now, but may be removed in a future version of SMAPI."
                : " It will be removed soon, so the mod will break if it's not updated.";
            if (source == null)
                message += $"{Environment.NewLine}{Environment.StackTrace}";

            // log message
            switch (severity)
            {
                case DeprecationLevel.Notice:
                    if (this.SendNoticesToConsole)
                        Log.Debug($"[DEV] {message}");
                    else
                        Log.LogToFile(message);
                    break;

                case DeprecationLevel.Info:
                    Log.Debug(message);
                    break;

                case DeprecationLevel.PendingRemoval:
                    Log.Warning(message);
                    break;

                default:
                    throw new NotImplementedException($"Unknown deprecation level '{severity}'");
            }
        }

        /// <summary>Mark a deprecation warning as already logged.</summary>
        /// <param name="nounPhrase">A noun phrase describing what is deprecated (e.g. "the Extensions.AsInt32 method").</param>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <returns>Returns whether the deprecation was successfully marked as warned. Returns <c>false</c> if it was already marked.</returns>
        public bool MarkWarned(string nounPhrase, string version)
        {
            return this.MarkWarned(this.GetSourceNameFromStack(), nounPhrase, version);
        }

        /// <summary>Mark a deprecation warning as already logged.</summary>
        /// <param name="source">The friendly name of the assembly which used the deprecated code.</param>
        /// <param name="nounPhrase">A noun phrase describing what is deprecated (e.g. "the Extensions.AsInt32 method").</param>
        /// <param name="version">The SMAPI version which deprecated it.</param>
        /// <returns>Returns whether the deprecation was successfully marked as warned. Returns <c>false</c> if it was already marked.</returns>
        public bool MarkWarned(string source, string nounPhrase, string version)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new InvalidOperationException("The deprecation source cannot be empty.");

            string key = $"{source}::{nounPhrase}::{version}";
            if (this.LoggedDeprecations.Contains(key))
                return false;
            this.LoggedDeprecations.Add(key);
            return true;
        }

        /// <summary>Get whether a type implements the given virtual method.</summary>
        /// <param name="subtype">The type to check.</param>
        /// <param name="baseType">The base type which declares the virtual method.</param>
        /// <param name="name">The method name.</param>
        public bool IsVirtualMethodImplemented(Type subtype, Type baseType, string name)
        {
            MethodInfo method = subtype.GetMethod(nameof(Mod.Entry), new[] { typeof(object[]) });
            return method.DeclaringType != baseType;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the friendly name for the closest assembly registered as a source of deprecation warnings.</summary>
        /// <returns>Returns the source name, or <c>null</c> if no registered assemblies were found.</returns>
        private string GetSourceNameFromStack()
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
