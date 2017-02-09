using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Framework;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Rewrites references to <see cref="SpriteBatch"/> to fix inconsistent method signatures between MonoGame and XNA.</summary>
    /// <remarks>MonoGame has one <c>SpriteBatch.Begin</c> method with optional arguments, but XNA has multiple method overloads. Incompatible method references are rewritten to use <see cref="WrapperMethods"/>, which redirects all method signatures to the proper compiled MonoGame/XNA method.</remarks>
    public class SpriteBatchRewriter : BaseMethodRewriter
    {
        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a method reference should be rewritten.</summary>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        protected override bool ShouldRewrite(MethodReference methodRef, bool platformChanged)
        {
            return platformChanged
                && methodRef.DeclaringType.FullName == typeof(SpriteBatch).FullName
                && this.HasMatchingSignature(typeof(SpriteBatchRewriter.WrapperMethods), methodRef);
        }

        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction which calls the method.</param>
        /// <param name="methodRef">The method reference invoked by the <paramref name="instruction"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        protected override void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, MethodReference methodRef, PlatformAssemblyMap assemblyMap)
        {
            methodRef.DeclaringType = module.Import(typeof(SpriteBatchRewriter.WrapperMethods));
        }


        /*********
        ** Wrapper methods
        *********/
        /// <summary>Wraps <see cref="SpriteBatch"/> methods that are incompatible when converting compiled code between MonoGame and XNA.</summary>
        public class WrapperMethods : SpriteBatch
        {
            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            public WrapperMethods(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }


            /****
            ** MonoGame signatures
            ****/
            [SuppressMessage("ReSharper", "CS0109", Justification = "The 'new' modifier applies when compiled on Linux/Mac.")]
            public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix? matrix)
            {
                base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix ?? Matrix.Identity);
            }

            /****
            ** XNA signatures
            ****/
            [SuppressMessage("ReSharper", "CS0109", Justification = "The 'new' modifier applies when compiled on Windows.")]
            public new void Begin()
            {
                base.Begin();
            }

            [SuppressMessage("ReSharper", "CS0109", Justification = "The 'new' modifier applies when compiled on Windows.")]
            public new void Begin(SpriteSortMode sortMode, BlendState blendState)
            {
                base.Begin(sortMode, blendState);
            }

            [SuppressMessage("ReSharper", "CS0109", Justification = "The 'new' modifier applies when compiled on Windows.")]
            public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
            {
                base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState);
            }

            [SuppressMessage("ReSharper", "CS0109", Justification = "The 'new' modifier applies when compiled on Windows.")]
            public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
            {
                base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect);
            }

            [SuppressMessage("ReSharper", "CS0109", Justification = "The 'new' modifier applies when compiled on Windows.")]
            public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
            {
                base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
            }
        }
    }
}