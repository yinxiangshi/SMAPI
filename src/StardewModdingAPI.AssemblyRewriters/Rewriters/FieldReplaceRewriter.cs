using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Finders;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Rewrites references to one field with another.</summary>
    public class FieldReplaceRewriter : FieldFinder, IInstructionRewriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The type whose field to which references should be rewritten.</summary>
        private readonly Type Type;

        /// <summary>The new field name to reference.</summary>
        private readonly string ToFieldName;


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
            this.Type = type;
            this.ToFieldName = toFieldName;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        public void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap)
        {
            FieldInfo field = this.Type.GetField(this.ToFieldName);
            if (field == null)
                throw new InvalidOperationException($"The {this.Type.FullName} class doesn't have a {this.ToFieldName} field.");
            FieldReference newRef = module.Import(field);
            cil.Replace(instruction, cil.Create(instruction.OpCode, newRef));
        }
    }
}
