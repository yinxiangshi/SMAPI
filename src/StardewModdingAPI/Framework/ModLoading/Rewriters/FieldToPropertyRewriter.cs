using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Finders;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters
{
    /// <summary>Rewrites field references into property references.</summary>
    internal class FieldToPropertyRewriter : FieldFinder
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

        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="mod">The mod to which the module belongs.</param>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        public override bool Rewrite(IModMetadata mod, ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            if (!this.IsMatch(instruction))
                return false;

            string methodPrefix = instruction.OpCode == OpCodes.Ldsfld || instruction.OpCode == OpCodes.Ldfld ? "get" : "set";
            MethodReference propertyRef = module.Import(this.Type.GetMethod($"{methodPrefix}_{this.FieldName}"));
            cil.Replace(instruction, cil.Create(OpCodes.Call, propertyRef));
            return true;
        }
    }
}
