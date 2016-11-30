using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Wrappers;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Rewrites references to <see cref="SpriteBatch"/> to fix inconsistent method signatures between MonoGame and XNA.</summary>
    /// <remarks>MonoGame has one <c>SpriteBatch.Begin</c> method with optional arguments, but XNA has multiple method overloads. Incompatible method references are rewritten to use <see cref="CompatibleSpriteBatch"/>, which redirects all method signatures to the proper compiled MonoGame/XNA method.</remarks>
    public class SpriteBatchRewriter : BaseMethodRewriter
    {
        /// <summary>Get whether the given method reference can be rewritten.</summary>
        /// <param name="methodRef">The method reference.</param>
        public override bool ShouldRewrite(MethodReference methodRef)
        {
            return methodRef.DeclaringType.FullName == typeof(SpriteBatch).FullName && this.HasMatchingSignature(typeof(CompatibleSpriteBatch), methodRef);
        }

        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="callOp">The instruction which calls the method.</param>
        /// <param name="methodRef">The method reference invoked by the <paramref name="callOp"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        public override void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction callOp, MethodReference methodRef, PlatformAssemblyMap assemblyMap)
        {
            methodRef.DeclaringType = module.Import(typeof(CompatibleSpriteBatch));
        }
    }
}