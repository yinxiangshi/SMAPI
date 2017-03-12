using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Framework
{
    /// <summary>Base class for a field rewriter.</summary>
    public abstract class BaseFieldRewriter : BaseFieldFinder, IInstructionRewriter
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
            FieldReference fieldRef = (FieldReference)instruction.Operand;
            this.Rewrite(module, cil, instruction, fieldRef, assemblyMap);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction which references the field.</param>
        /// <param name="fieldRef">The field reference invoked by the <paramref name="instruction"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        protected abstract void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, FieldReference fieldRef, PlatformAssemblyMap assemblyMap);
    }
}
