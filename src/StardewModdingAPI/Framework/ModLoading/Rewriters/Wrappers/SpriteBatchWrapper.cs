using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.Wrappers
{
    /// <summary>Wraps <see cref="SpriteBatch"/> methods that are incompatible when converting compiled code between MonoGame and XNA.</summary>
    internal class SpriteBatchWrapper : SpriteBatch
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SpriteBatchWrapper(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }


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
