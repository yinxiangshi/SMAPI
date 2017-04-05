using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Finders;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Rewrites references to one field with another.</summary>
    public class FieldReplaceRewriter : FieldFinder
    {
        /*********
        ** Properties
        *********/
        /// <summary>The new field to reference.</summary>
        private readonly FieldInfo ToField;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The type whose field to which references should be rewritten.</param>
        /// <param name="fromFieldName">The field name to rewrite.</param>
        /// <param name="toFieldName">The new field name to reference.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public FieldReplaceRewriter(Type type, string fromFieldName, string toFieldName, string nounPhrase = null)
            : base(type.FullName, fromFieldName, nounPhrase)
        {
            this.ToField = type.GetField(toFieldName);
            if (this.ToField == null)
                throw new InvalidOperationException($"The {type.FullName} class doesn't have a {toFieldName} field.");
        }

        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        public override bool Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            if (!this.IsMatch(instruction))
                return false;

            FieldReference newRef = module.Import(this.ToField);
            cil.Replace(instruction, cil.Create(instruction.OpCode, newRef));
            return true;
        }
    }
}
