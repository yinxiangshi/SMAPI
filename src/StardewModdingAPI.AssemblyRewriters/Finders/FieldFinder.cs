using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Finds CIL instructions that reference a given field.</summary>
    public class FieldFinder : IInstructionFinder
    {
        /*********
        ** Properties
        *********/
        /// <summary>The full type name for which to find references.</summary>
        private readonly string FullTypeName;

        /// <summary>The field name for which to find references.</summary>
        private readonly string FieldName;


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fullTypeName">The full type name for which to find references.</param>
        /// <param name="fieldName">The field name for which to find references.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public FieldFinder(string fullTypeName, string fieldName, string nounPhrase = null)
        {
            this.FullTypeName = fullTypeName;
            this.FieldName = fieldName;
            this.NounPhrase = nounPhrase ?? $"{fullTypeName}.{fieldName} field";
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public bool IsMatch(Instruction instruction, bool platformChanged)
        {
            FieldReference fieldRef = RewriteHelper.AsFieldReference(instruction);
            return
                fieldRef != null
                && fieldRef.DeclaringType.FullName == this.FullTypeName
                && fieldRef.Name == this.FieldName;
        }
    }
}
