using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>Rewrites CIL instructions for compatibility.</summary>
    internal interface IInstructionRewriter
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the rewriter matches.</summary>
        string NounPhrase { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Rewrite a method definition for compatibility.</summary>
        /// <param name="mod">The mod to which the module belongs.</param>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="method">The method definition to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        bool Rewrite(IModMetadata mod, ModuleDefinition module, MethodDefinition method, PlatformAssemblyMap assemblyMap, bool platformChanged);

        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="mod">The mod to which the module belongs.</param>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        bool Rewrite(IModMetadata mod, ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged);
    }
}
