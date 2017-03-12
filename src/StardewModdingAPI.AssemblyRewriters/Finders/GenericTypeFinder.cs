using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Finds CIL instructions that reference a given type.</summary>
    public sealed class GenericTypeFinder : IInstructionFinder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The full type name for which to find references.</summary>
        private readonly string FullTypeName;


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fullTypeName">The full type name to match.</param>
        public GenericTypeFinder(string fullTypeName)
        {
            this.FullTypeName = fullTypeName;
            this.NounPhrase = $"obsolete {fullTypeName} type";
        }

        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public bool IsMatch(Instruction instruction, bool platformChanged)
        {
            string fullName = this.FullTypeName;

            // field reference
            if (instruction.OpCode == OpCodes.Ldfld || instruction.OpCode == OpCodes.Ldsfld || instruction.OpCode == OpCodes.Stfld || instruction.OpCode == OpCodes.Stsfld)
            {
                FieldReference field = (FieldReference)instruction.Operand;
                return
                    field.DeclaringType.FullName == fullName // field on target class
                    || field.FieldType.FullName == fullName; // field value is target class
            }

            // method reference
            if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
            {
                MethodReference method = (MethodReference)instruction.Operand;
                return
                    method.DeclaringType.FullName == fullName // method on target class
                    || method.ReturnType.FullName == fullName // method returns target class
                    || method.Parameters.Any(p => p.ParameterType.FullName == fullName); // method parameters
            }

            return false;
        }
    }
}
