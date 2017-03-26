using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Finds incompatible CIL instructions that reference a given type and throws an <see cref="IncompatibleInstructionException"/>.</summary>
    public class TypeFinder : IInstructionRewriter
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
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public TypeFinder(string fullTypeName, string nounPhrase = null)
        {
            this.FullTypeName = fullTypeName;
            this.NounPhrase = nounPhrase ?? $"{fullTypeName} type";
        }

        /// <summary>Rewrite a method definition for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="method">The method definition to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        public virtual bool Rewrite(ModuleDefinition module, MethodDefinition method, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            if (!this.IsMatch(method))
                return false;

            throw new IncompatibleInstructionException(this.NounPhrase);
        }

        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        public virtual bool Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            if (!this.IsMatch(instruction))
                return false;

            throw new IncompatibleInstructionException(this.NounPhrase);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="method">The method deifnition.</param>
        protected bool IsMatch(MethodDefinition method)
        {
            if (method.ReturnType.FullName == this.FullTypeName)
                return true;

            foreach (VariableDefinition variable in method.Body.Variables)
            {
                if (variable.VariableType.FullName == this.FullTypeName)
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        protected bool IsMatch(Instruction instruction)
        {
            string fullName = this.FullTypeName;

            // field reference
            FieldReference fieldRef = RewriteHelper.AsFieldReference(instruction);
            if (fieldRef != null)
            {
                return
                    fieldRef.DeclaringType.FullName == fullName // field on target class
                    || fieldRef.FieldType.FullName == fullName; // field value is target class
            }

            // method reference
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            if (methodRef != null)
            {
                return
                    methodRef.DeclaringType.FullName == fullName // method on target class
                    || methodRef.ReturnType.FullName == fullName // method returns target class
                    || methodRef.Parameters.Any(p => p.ParameterType.FullName == fullName); // method parameters
            }

            return false;
        }
    }
}
