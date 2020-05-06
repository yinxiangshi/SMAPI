using System;
using Mono.Cecil;
using StardewModdingAPI.Framework.ModLoading.Framework;

namespace StardewModdingAPI.Framework.ModLoading.Finders
{
    /// <summary>Finds incompatible CIL instructions that reference types in a given assembly.</summary>
    internal class TypeAssemblyFinder : BaseTypeFinder
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="assemblyName">The full assembly name to which to find references.</param>
        /// <param name="result">The result to return for matching instructions.</param>
        /// <param name="shouldIgnore">A lambda which overrides a matched type.</param>
        public TypeAssemblyFinder(string assemblyName, InstructionHandleResult result, Func<TypeReference, bool> shouldIgnore = null)
            : base(
                isMatch: type => type.Scope.Name == assemblyName && (shouldIgnore == null || !shouldIgnore(type)),
                result: result,
                nounPhrase: $"{assemblyName} assembly"
            )
        { }
    }
}
