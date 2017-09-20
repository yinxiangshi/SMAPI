using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.Framework.ModLoading.Finders
{
    /// <summary>Finds incompatible CIL instructions that reference a given method and throws an <see cref="IncompatibleInstructionException"/>.</summary>
    internal class MethodFinder : IInstructionRewriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The full type name for which to find references.</summary>
        private readonly string FullTypeName;

        /// <summary>The method name for which to find references.</summary>
        private readonly string MethodName;


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
        /// <param name="methodName">The method name for which to find references.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public MethodFinder(string fullTypeName, string methodName, string nounPhrase = null)
        {
            this.FullTypeName = fullTypeName;
            this.MethodName = methodName;
            this.NounPhrase = nounPhrase ?? $"{fullTypeName}.{methodName} method";
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
            return false;
        }

        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        public bool Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            if (!this.IsMatch(instruction))
                return false;

            throw new IncompatibleInstructionException(this.NounPhrase);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        protected bool IsMatch(Instruction instruction)
        {
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            return
                methodRef != null
                && methodRef.DeclaringType.FullName == this.FullTypeName
                && methodRef.Name == this.MethodName;
        }
    }
}
