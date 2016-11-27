using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace StardewModdingAPI.Framework.AssemblyRewriting
{
    /// <summary>Rewrites type references.</summary>
    internal class AssemblyTypeRewriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The assemblies to target. Equivalent types will be rewritten to use these assemblies.</summary>
        private readonly Assembly[] TargetAssemblies;

        /// <summary>>The short assembly names to remove as assembly reference, and replace with the <see cref="TargetAssemblies"/>.</summary>
        private readonly string[] RemoveAssemblyNames;

        /// <summary>A type => assembly lookup for types which should be rewritten.</summary>
        private readonly IDictionary<string, Assembly> TypeAssemblies;

        /// <summary>An assembly => reference cache.</summary>
        private readonly IDictionary<Assembly, AssemblyNameReference> AssemblyNameReferences;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="targetAssemblies">The assembly filenames to target. Equivalent types will be rewritten to use these assemblies.</param>
        /// <param name="removeAssemblyNames">The short assembly names to remove as assembly reference, and replace with the <paramref name="targetAssemblies"/>.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public AssemblyTypeRewriter(Assembly[] targetAssemblies, string[] removeAssemblyNames, IMonitor monitor)
        {
            // save config
            this.TargetAssemblies = targetAssemblies;
            this.RemoveAssemblyNames = removeAssemblyNames;
            this.Monitor = monitor;

            // cache assembly metadata
            this.AssemblyNameReferences = targetAssemblies.ToDictionary(assembly => assembly, assembly => AssemblyNameReference.Parse(assembly.FullName));

            // collect type => assembly lookup
            this.TypeAssemblies = new Dictionary<string, Assembly>();
            foreach (Assembly assembly in targetAssemblies)
            {
                foreach (Module assemblyModule in assembly.Modules)
                {
                    ModuleDefinition module = ModuleDefinition.ReadModule(assemblyModule.FullyQualifiedName);
                    foreach (TypeDefinition type in module.GetTypes())
                    {
                        if (!type.IsPublic)
                            continue; // no need to rewrite
                        if (type.Namespace.Contains("<"))
                            continue; // ignore assembly metadata
                        this.TypeAssemblies[type.FullName] = assembly;
                    }
                }
            }
        }

        /// <summary>Rewrite the types referenced by an assembly.</summary>
        /// <param name="assembly">The assembly to rewrite.</param>
        public void RewriteAssembly(AssemblyDefinition assembly)
        {
            ModuleDefinition module = assembly.Modules.Single(); // technically an assembly can have multiple modules, but none of the build tools (including MSBuild) support it; simplify by assuming one module
            bool shouldRewrite = false;

            // remove old assembly references
            for (int i = 0; i < module.AssemblyReferences.Count; i++)
            {
                bool shouldRemove = this.RemoveAssemblyNames.Any(name => module.AssemblyReferences[i].Name == name);
                if (shouldRemove)
                {
                    this.Monitor.Log($"removing reference to {module.AssemblyReferences[i]}", LogLevel.Trace);
                    shouldRewrite = true;
                    module.AssemblyReferences.RemoveAt(i);
                    i--;
                }
            }

            // replace references
            if (shouldRewrite)
            {
                // add target assembly references
                foreach (AssemblyNameReference target in this.AssemblyNameReferences.Values)
                {
                    this.Monitor.Log($"  adding reference to {target}", LogLevel.Trace);
                    module.AssemblyReferences.Add(target);
                }

                // rewrite type scopes to use target assemblies
                IEnumerable<TypeReference> typeReferences = module.GetTypeReferences().OrderBy(p => p.FullName);
                string lastTypeLogged = null;
                foreach (TypeReference type in typeReferences)
                {
                    this.ChangeTypeScope(type, shouldLog: type.FullName != lastTypeLogged);
                    lastTypeLogged = type.FullName;
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the correct reference to use for compatibility with the current platform.</summary>
        /// <param name="type">The type reference to rewrite.</param>
        /// <param name="shouldLog">Whether to log a message.</param>
        private void ChangeTypeScope(TypeReference type, bool shouldLog)
        {
            // check skip conditions
            if (type == null || type.FullName.StartsWith("System."))
                return;

            // get assembly
            Assembly assembly;
            if (!this.TypeAssemblies.TryGetValue(type.FullName, out assembly))
                return;

            // replace scope
            AssemblyNameReference assemblyRef = this.AssemblyNameReferences[assembly];
            if (shouldLog)
                this.Monitor.Log($"redirecting {type.FullName} from {type.Scope.Name} to {assemblyRef.Name}", LogLevel.Trace);
            type.Scope = assemblyRef;
        }
    }
}
