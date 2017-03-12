using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Framework;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Finds CIL instructions that reference a given method.</summary>
    public sealed class GenericMethodFinder : BaseMethodFinder
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
        public override string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fullTypeName">The full type name for which to find references.</param>
        /// <param name="methodName">The method name for which to find references.</param>
        public GenericMethodFinder(string fullTypeName, string methodName)
        {
            this.FullTypeName = fullTypeName;
            this.MethodName = methodName;
            this.NounPhrase = $"obsolete {fullTypeName}.{methodName} method";
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a method reference should be rewritten.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        protected override bool IsMatch(Instruction instruction, MethodReference methodRef, bool platformChanged)
        {
            return methodRef.DeclaringType.FullName == this.FullTypeName
                && methodRef.Name == this.MethodName;
        }
    }
}
