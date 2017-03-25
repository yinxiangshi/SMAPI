using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Finders;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Rewrites field references into property references.</summary>
    public class FieldToPropertyRewriter : FieldFinder, IInstructionRewriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The type whose field to which references should be rewritten.</summary>
        private readonly Type Type;

        /// <summary>The field name to rewrite.</summary>
        private readonly string FieldName;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The type whose field to which references should be rewritten.</param>
        /// <param name="fieldName">The field name to rewrite.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public FieldToPropertyRewriter(Type type, string fieldName, string nounPhrase = null)
            : base(type.FullName, fieldName, nounPhrase)
        {
            this.Type = type;
            this.FieldName = fieldName;
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
            string methodPrefix = instruction.OpCode == OpCodes.Ldsfld || instruction.OpCode == OpCodes.Ldfld ? "get" : "set";
            MethodReference propertyRef = module.Import(this.Type.GetMethod($"{methodPrefix}_{this.FieldName}"));
            cil.Replace(instruction, cil.Create(OpCodes.Call, propertyRef));
        }
    }
}
