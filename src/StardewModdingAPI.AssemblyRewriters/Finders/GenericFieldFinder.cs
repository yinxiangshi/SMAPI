using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Framework;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Finds CIL instructions that reference a given field.</summary>
    public sealed class GenericFieldFinder : BaseFieldFinder
    {
        /*********
        ** Properties
        *********/
        /// <summary>The full type name for which to find references.</summary>
        private readonly string FullTypeName;

        /// <summary>The field name for which to find references.</summary>
        private readonly string FieldName;

        /// <summary>Whether the field to match is static.</summary>
        private readonly bool IsStatic;


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public override string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fullTypeName">The full type name for which to find references.</param>
        /// <param name="fieldName">The field name for which to find references.</param>
        /// <param name="isStatic">Whether the field to match is static.</param>
        public GenericFieldFinder(string fullTypeName, string fieldName, bool isStatic)
        {
            this.FullTypeName = fullTypeName;
            this.FieldName = fieldName;
            this.IsStatic = isStatic;
            this.NounPhrase = $"obsolete {fullTypeName}.{fieldName} field";
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
                this.IsStaticField(instruction) == this.IsStatic
                && fieldRef.DeclaringType.FullName == this.FullTypeName
                && fieldRef.Name == this.FieldName;
        }
    }
}
