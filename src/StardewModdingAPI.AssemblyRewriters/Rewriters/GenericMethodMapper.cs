using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Framework;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Rewrites method references from one parent type to another if the signatures match.</summary>
    public class GenericMethodMapper : BaseMethodRewriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The type whose methods to remap.</summary>
        private readonly Type FromType;

        /// <summary>The type with methods to map to.</summary>
        private readonly Type ToType;

        /// <summary>Whether to only rewrite references if loading the assembly on a different platform than it was compiled on.</summary>
        private readonly bool OnlyIfPlatformChanged;


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public override string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fromType">The type whose methods to remap.</param>
        /// <param name="toType">The type with methods to map to.</param>
        /// <param name="onlyIfPlatformChanged">Whether to only rewrite references if loading the assembly on a different platform than it was compiled on.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public GenericMethodMapper(Type fromType, Type toType, bool onlyIfPlatformChanged = false, string nounPhrase = null)
        {
            this.FromType = fromType;
            this.ToType = toType;
            this.NounPhrase = nounPhrase ?? $"{fromType.Name} methods";
            this.OnlyIfPlatformChanged = onlyIfPlatformChanged;
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
            return
                (!this.OnlyIfPlatformChanged || platformChanged)
                && methodRef.DeclaringType.FullName == this.FromType.FullName
                && this.HasMatchingSignature(this.ToType, methodRef);
        }

        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction which calls the method.</param>
        /// <param name="methodRef">The method reference invoked by the <paramref name="instruction"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        protected override void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, MethodReference methodRef, PlatformAssemblyMap assemblyMap)
        {
            methodRef.DeclaringType = module.Import(this.ToType);
        }
    }
}
