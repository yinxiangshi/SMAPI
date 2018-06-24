using System.Collections.Generic;
using Mono.Cecil;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>A minimal assembly definition resolver which resolves references to known assemblies.</summary>
    internal class AssemblyDefinitionResolver : DefaultAssemblyResolver
    {
        /*********
        ** Properties
        *********/
        /// <summary>The known assemblies.</summary>
        private readonly IDictionary<string, AssemblyDefinition> Loaded = new Dictionary<string, AssemblyDefinition>();


        /*********
        ** Public methods
        *********/
        /// <summary>Add known assemblies to the resolver.</summary>
        /// <param name="assemblies">The known assemblies.</param>
        public void Add(params AssemblyDefinition[] assemblies)
        {
            foreach (AssemblyDefinition assembly in assemblies)
            {
                this.Loaded[assembly.Name.Name] = assembly;
                this.Loaded[assembly.Name.FullName] = assembly;
            }
        }

        /// <summary>Resolve an assembly reference.</summary>
        /// <param name="name">The assembly name.</param>
        public override AssemblyDefinition Resolve(AssemblyNameReference name) => this.ResolveName(name.Name) ?? base.Resolve(name);

        /// <summary>Resolve an assembly reference.</summary>
        /// <param name="name">The assembly name.</param>
        /// <param name="parameters">The assembly reader parameters.</param>
        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => this.ResolveName(name.Name) ?? base.Resolve(name, parameters);


        /*********
        ** Private methods
        *********/
        /// <summary>Resolve a known assembly definition based on its short or full name.</summary>
        /// <param name="name">The assembly's short or full name.</param>
        private AssemblyDefinition ResolveName(string name)
        {
            return this.Loaded.ContainsKey(name)
                ? this.Loaded[name]
                : null;
        }
    }
}
