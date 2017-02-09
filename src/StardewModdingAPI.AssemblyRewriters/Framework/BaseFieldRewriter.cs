using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Framework
{
    /// <summary>Base class for a field rewriter.</summary>
    public abstract class BaseFieldRewriter : IInstructionRewriter
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether a CIL instruction should be rewritten.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public bool ShouldRewrite(Instruction instruction, bool platformChanged)
        {
            if (instruction.OpCode != OpCodes.Ldfld && instruction.OpCode != OpCodes.Ldsfld && instruction.OpCode != OpCodes.Stfld && instruction.OpCode != OpCodes.Stsfld)
                return false; // not a field reference
            return this.ShouldRewrite(instruction, (FieldReference)instruction.Operand, platformChanged);
        }

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
        /// <summary>Get whether a field reference should be rewritten.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="fieldRef">The field reference.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        protected abstract bool ShouldRewrite(Instruction instruction, FieldReference fieldRef, bool platformChanged);

        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction which references the field.</param>
        /// <param name="fieldRef">The field reference invoked by the <paramref name="instruction"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        protected abstract void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, FieldReference fieldRef, PlatformAssemblyMap assemblyMap);
    }
}
