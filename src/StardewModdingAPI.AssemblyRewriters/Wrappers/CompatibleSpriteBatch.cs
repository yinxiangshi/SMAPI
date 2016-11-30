using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#pragma warning disable CS0109 // Member does not hide an inherited member; new keyword is not required
namespace StardewModdingAPI.AssemblyRewriters.Wrappers
{
    /// <summary>Wraps <see cref="SpriteBatch"/> methods that are incompatible when converting compiled code between MonoGame and XNA.</summary>
    public class CompatibleSpriteBatch : SpriteBatch
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public CompatibleSpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }

        /****
        ** MonoGame signatures
        ****/
        public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix? matrix)
        {
            base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix ?? Matrix.Identity);
        }

        /****
        ** XNA signatures
        ****/
        public new void Begin()
        {
            base.Begin();
        }

        public new void Begin(SpriteSortMode sortMode, BlendState blendState)
        {
            base.Begin(sortMode, blendState);
        }

        public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
        {
            base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState);
        }

        public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
        {
            base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect);
        }

        public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
        {
            base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
        }
    }
}