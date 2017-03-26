using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Rewrites method references from one parent type to another if the signatures match.</summary>
    public class MethodParentRewriter : IInstructionRewriter
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
        public string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fromType">The type whose methods to remap.</param>
        /// <param name="toType">The type with methods to map to.</param>
        /// <param name="onlyIfPlatformChanged">Whether to only rewrite references if loading the assembly on a different platform than it was compiled on.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public MethodParentRewriter(Type fromType, Type toType, bool onlyIfPlatformChanged = false, string nounPhrase = null)
        {
            this.FromType = fromType;
            this.ToType = toType;
            this.NounPhrase = nounPhrase ?? $"{fromType.Name} methods";
            this.OnlyIfPlatformChanged = onlyIfPlatformChanged;
        }

        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public bool IsMatch(Instruction instruction, bool platformChanged)
        {
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            return
                methodRef != null
                && (platformChanged || !this.OnlyIfPlatformChanged)
                && methodRef.DeclaringType.FullName == this.FromType.FullName
                && RewriteHelper.HasMatchingSignature(this.ToType, methodRef);
        }

        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        public void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap)
        {
            MethodReference methodRef = (MethodReference)instruction.Operand;
            methodRef.DeclaringType = module.Import(this.ToType);
        }
    }
}
