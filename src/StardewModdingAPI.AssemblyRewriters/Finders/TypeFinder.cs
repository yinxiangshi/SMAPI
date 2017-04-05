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
            if (this.IsMatch(method.ReturnType))
                return true;

            foreach (VariableDefinition variable in method.Body.Variables)
            {
                if (this.IsMatch(variable.VariableType))
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        protected bool IsMatch(Instruction instruction)
        {
            // field reference
            FieldReference fieldRef = RewriteHelper.AsFieldReference(instruction);
            if (fieldRef != null)
            {
                return
                    this.IsMatch(fieldRef.DeclaringType) // field on target class
                    || this.IsMatch(fieldRef.FieldType); // field value is target class
            }

            // method reference
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            if (methodRef != null)
            {
                return
                    this.IsMatch(methodRef.DeclaringType) // method on target class
                    || this.IsMatch(methodRef.ReturnType) // method returns target class
                    || methodRef.Parameters.Any(p => this.IsMatch(p.ParameterType)); // method parameters
            }

            return false;
        }

        /// <summary>Get whether a type reference matches the expected type.</summary>
        /// <param name="type">The type to check.</param>
        protected bool IsMatch(TypeReference type)
        {
            // root type
            if (type.FullName == this.FullTypeName)
                return true;

            // generic arguments
            if (type is GenericInstanceType genericType)
            {
                if (genericType.GenericArguments.Any(this.IsMatch))
                    return true;
            }

            // generic parameters (e.g. constraints)
            if (type.GenericParameters.Any(this.IsMatch))
                return true;

            return false;
        }
    }
}
