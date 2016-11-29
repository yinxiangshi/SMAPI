using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters
{
    /// <summary>Rewrites a method for compatibility.</summary>
    public interface IMethodRewriter
    {
        /// <summary>Get whether the given method reference can be rewritten.</summary>
        /// <param name="methodRef">The method reference.</param>
        bool ShouldRewrite(MethodReference methodRef);

        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="callOp">The instruction which calls the method.</param>
        /// <param name="methodRef">The method reference invoked by the <paramref name="callOp"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction callOp, MethodReference methodRef, PlatformAssemblyMap assemblyMap);
    }
}
