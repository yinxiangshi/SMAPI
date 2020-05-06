using System;
using Mono.Cecil;
using StardewModdingAPI.Framework.ModLoading.Framework;

namespace StardewModdingAPI.Framework.ModLoading.Finders
{
    /// <summary>Finds incompatible CIL instructions that reference a given type.</summary>
    internal class TypeFinder : BaseTypeFinder
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fullTypeName">The full type name to match.</param>
        /// <param name="result">The result to return for matching instructions.</param>
        /// <param name="shouldIgnore">A lambda which overrides a matched type.</param>
        public TypeFinder(string fullTypeName, InstructionHandleResult result, Func<TypeReference, bool> shouldIgnore = null)
            : base(
                isMatch: type => type.FullName == fullTypeName && (shouldIgnore == null || !shouldIgnore(type)),
                result: result,
                nounPhrase: $"{fullTypeName} type"
            )
        { }
    }
}
