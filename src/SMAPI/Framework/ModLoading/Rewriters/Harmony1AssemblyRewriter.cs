using System;
using Mono.Cecil;
using StardewModdingAPI.Framework.ModLoading.Finders;
using StardewModdingAPI.Framework.ModLoading.Framework;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters
{
    /// <summary>Rewrites Harmony 1.x assembly references to work with Harmony 2.x.</summary>
    internal class Harmony1AssemblyRewriter : BaseTypeReferenceRewriter
    {
        /*********
        ** Fields
        *********/
        /// <summary>The full assembly name to which to find references.</summary>
        private const string FromAssemblyName = "0Harmony";

        /// <summary>The main Harmony type.</summary>
        private readonly Type HarmonyType = typeof(HarmonyLib.Harmony);


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the rewriter matches.</summary>
        public const string DefaultNounPhrase = "Harmony 1.x";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Harmony1AssemblyRewriter()
            : base(new TypeAssemblyFinder(Harmony1AssemblyRewriter.FromAssemblyName, InstructionHandleResult.None), Harmony1AssemblyRewriter.DefaultNounPhrase)
        { }


        /*********
        ** Private methods
        *********/
        /// <summary>Change a type reference if needed.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="type">The type to replace if it matches.</param>
        /// <param name="set">Assign the new type reference.</param>
        protected override bool RewriteIfNeeded(ModuleDefinition module, TypeReference type, Action<TypeReference> set)
        {
            bool rewritten = false;

            // current type
            if (type.Scope.Name == Harmony1AssemblyRewriter.FromAssemblyName && type.Scope is AssemblyNameReference assemblyScope && assemblyScope.Version.Major == 1)
            {
                Type targetType = this.GetMappedType(type);
                set(module.ImportReference(targetType));
                return true;
            }

            // recurse into generic arguments
            if (type is GenericInstanceType genericType)
            {
                for (int i = 0; i < genericType.GenericArguments.Count; i++)
                    rewritten |= this.RewriteIfNeeded(module, genericType.GenericArguments[i], typeRef => genericType.GenericArguments[i] = typeRef);
            }

            // recurse into generic parameters (e.g. constraints)
            for (int i = 0; i < type.GenericParameters.Count; i++)
                rewritten |= this.RewriteIfNeeded(module, type.GenericParameters[i], typeRef => type.GenericParameters[i] = new GenericParameter(typeRef));

            return rewritten;
        }

        /// <summary>Get an equivalent Harmony 2.x type.</summary>
        /// <param name="type">The Harmony 1.x method.</param>
        private Type GetMappedType(TypeReference type)
        {
            // main Harmony object
            if (type.FullName == "Harmony.HarmonyInstance")
                return this.HarmonyType;

            // other objects
            string fullName = type.FullName.Replace("Harmony.", "HarmonyLib.");
            string targetName = this.HarmonyType.AssemblyQualifiedName.Replace(this.HarmonyType.FullName, fullName);
            return Type.GetType(targetName, throwOnError: true);
        }
    }
}
