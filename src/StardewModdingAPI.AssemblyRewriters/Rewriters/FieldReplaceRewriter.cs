using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Framework;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Rewrites references to one field with another.</summary>
    public class FieldReplaceRewriter : BaseFieldRewriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The type whose field to which references should be rewritten.</summary>
        private readonly Type Type;

        /// <summary>The field name to rewrite.</summary>
        private readonly string FromFieldName;

        /// <summary>The new field name to reference.</summary>
        private readonly string ToFieldName;


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public override string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The type whose field to which references should be rewritten.</param>
        /// <param name="fromFieldName">The field name to rewrite.</param>
        /// <param name="toFieldName">The new field name to reference.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public FieldReplaceRewriter(Type type, string fromFieldName, string toFieldName, string nounPhrase = null)
        {
            this.Type = type;
            this.FromFieldName = fromFieldName;
            this.ToFieldName = toFieldName;
            this.NounPhrase = nounPhrase ?? $"{type.Name}.{fromFieldName} field";
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a field reference should be rewritten.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="fieldRef">The field reference.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        protected override bool IsMatch(Instruction instruction, FieldReference fieldRef, bool platformChanged)
        {
            return
                fieldRef.DeclaringType.FullName == this.Type.FullName
                && fieldRef.Name == this.FromFieldName;
        }

        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction which references the field.</param>
        /// <param name="fieldRef">The field reference invoked by the <paramref name="instruction"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        protected override void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, FieldReference fieldRef, PlatformAssemblyMap assemblyMap)
        {
            FieldInfo field = this.Type.GetField(this.ToFieldName);
            if(field == null)
                throw new InvalidOperationException($"The {this.Type.FullName} class doesn't have a {this.ToFieldName} field.");
            FieldReference newRef = module.Import(field);
            cil.Replace(instruction, cil.Create(instruction.OpCode, newRef));
        }
    }
}
