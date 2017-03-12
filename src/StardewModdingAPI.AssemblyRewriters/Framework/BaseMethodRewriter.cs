using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Framework
{
    /// <summary>Base class for a method rewriter.</summary>
    public abstract class BaseMethodRewriter : BaseMethodFinder, IInstructionRewriter
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        public void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap)
        {
            MethodReference methodRef = (MethodReference)instruction.Operand;
            this.Rewrite(module, cil, instruction, methodRef, assemblyMap);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction which calls the method.</param>
        /// <param name="methodRef">The method reference invoked by the <paramref name="instruction"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        protected abstract void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, MethodReference methodRef, PlatformAssemblyMap assemblyMap);
    }
}
