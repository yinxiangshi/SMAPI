using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Framework
{
    /// <summary>Base class for a type reference finder.</summary>
    public abstract class BaseTypeFinder : IInstructionFinder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public abstract string NounPhrase { get; }

        /// <summary>The full type name to match.</summary>
        public abstract string FullTypeName { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public virtual bool IsMatch(Instruction instruction, bool platformChanged)
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
