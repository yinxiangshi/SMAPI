using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Framework.Input
{
    /// <summary>Manipulates mouse state.</summary>
    internal class MouseStateBuilder
    {
        /*********
        ** Fields
        *********/
        /// <summary>The current button states.</summary>
        private readonly IDictionary<SButton, ButtonState> ButtonStates;

        /// <summary>The X cursor position.</summary>
        private readonly int X;

        /// <summary>The Y cursor position.</summary>
        private readonly int Y;

        /// <summary>The mouse wheel scroll value.</summary>
        private readonly int ScrollWheelValue;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="state">The initial state.</param>
        public MouseStateBuilder(MouseState state)
        {
            this.ButtonStates = new Dictionary<SButton, ButtonState>
            {
                [SButton.MouseLeft] = state.LeftButton,
                [SButton.MouseMiddle] = state.MiddleButton,
                [SButton.MouseRight] = state.RightButton,
                [SButton.MouseX1] = state.XButton1,
                [SButton.MouseX2] = state.XButton2
            };
            this.X = state.X;
            this.Y = state.Y;
            this.ScrollWheelValue = state.ScrollWheelValue;
        }

        /// <summary>Override the states for a set of buttons.</summary>
        /// <param name="overrides">The button state overrides.</param>
        public MouseStateBuilder OverrideButtons(IDictionary<SButton, SButtonState> overrides)
        {
            foreach (var pair in overrides)
            {
                bool isDown = pair.Value.IsDown();
                if (this.ButtonStates.ContainsKey(pair.Key))
                    this.ButtonStates[pair.Key] = isDown ? ButtonState.Pressed : ButtonState.Released;
            }

            return this;
        }

        /// <summary>Construct an equivalent mouse state.</summary>
        public MouseState ToMouseState()
        {
            return new MouseState(
                x: this.X,
                y: this.Y,
                scrollWheel: this.ScrollWheelValue,
                leftButton: this.ButtonStates[SButton.MouseLeft],
                middleButton: this.ButtonStates[SButton.MouseMiddle],
                rightButton: this.ButtonStates[SButton.MouseRight],
                xButton1: this.ButtonStates[SButton.MouseX1],
                xButton2: this.ButtonStates[SButton.MouseX2]
            );
        }
    }
}
