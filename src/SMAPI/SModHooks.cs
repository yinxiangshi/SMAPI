using System;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace StardewModdingAPI
{
    /// <summary>Intercepts predefined Stardew Valley mod hooks.</summary>
    internal class SModHooks : ModHooks
    {
        /*********
        ** Delegates
        *********/
        /// <summary>A delegate invoked by the <see cref="SModHooks.OnGame1_UpdateControlInput"/> hook.</summary>
        /// <param name="keyboardState">The game's keyboard state for the current tick.</param>
        /// <param name="mouseState">The game's mouse state for the current tick.</param>
        /// <param name="gamePadState">The game's controller state for the current tick.</param>
        /// <param name="action">The game's default logic.</param>
        public delegate void UpdateControlInputDelegate(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, Action action);


        /*********
        ** Properties
        *********/
        /// <summary>The callback for <see cref="OnGame1_UpdateControlInput"/>.</summary>
        private readonly UpdateControlInputDelegate UpdateControlInputHandler;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="updateControlInputHandler">The callback for <see cref="OnGame1_UpdateControlInput"/>.</param>
        public SModHooks(UpdateControlInputDelegate updateControlInputHandler)
        {
            this.UpdateControlInputHandler = updateControlInputHandler;
        }

        /// <summary>A hook invoked before the game processes player input.</summary>
        /// <param name="keyboardState">The game's keyboard state for the current tick.</param>
        /// <param name="mouseState">The game's mouse state for the current tick.</param>
        /// <param name="gamePadState">The game's controller state for the current tick.</param>
        /// <param name="action">The game's default logic.</param>
        public override void OnGame1_UpdateControlInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, Action action)
        {
            this.UpdateControlInputHandler(ref keyboardState, ref mouseState, ref gamePadState, action);
        }
    }
}
