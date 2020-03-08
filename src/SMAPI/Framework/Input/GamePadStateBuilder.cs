using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Framework.Input
{
    /// <summary>Manipulates controller state.</summary>
    internal class GamePadStateBuilder
    {
        /*********
        ** Fields
        *********/
        /// <summary>The current button states.</summary>
        private readonly IDictionary<SButton, ButtonState> ButtonStates;

        /// <summary>The left trigger value.</summary>
        private float LeftTrigger;

        /// <summary>The right trigger value.</summary>
        private float RightTrigger;

        /// <summary>The left thumbstick position.</summary>
        private Vector2 LeftStickPos;

        /// <summary>The left thumbstick position.</summary>
        private Vector2 RightStickPos;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="state">The initial state.</param>
        public GamePadStateBuilder(GamePadState state)
        {
            this.ButtonStates = new Dictionary<SButton, ButtonState>
            {
                [SButton.DPadUp] = state.DPad.Up,
                [SButton.DPadDown] = state.DPad.Down,
                [SButton.DPadLeft] = state.DPad.Left,
                [SButton.DPadRight] = state.DPad.Right,

                [SButton.ControllerA] = state.Buttons.A,
                [SButton.ControllerB] = state.Buttons.B,
                [SButton.ControllerX] = state.Buttons.X,
                [SButton.ControllerY] = state.Buttons.Y,
                [SButton.LeftStick] = state.Buttons.LeftStick,
                [SButton.RightStick] = state.Buttons.RightStick,
                [SButton.LeftShoulder] = state.Buttons.LeftShoulder,
                [SButton.RightShoulder] = state.Buttons.RightShoulder,
                [SButton.ControllerBack] = state.Buttons.Back,
                [SButton.ControllerStart] = state.Buttons.Start,
                [SButton.BigButton] = state.Buttons.BigButton
            };
            this.LeftTrigger = state.Triggers.Left;
            this.RightTrigger = state.Triggers.Right;
            this.LeftStickPos = state.ThumbSticks.Left;
            this.RightStickPos = state.ThumbSticks.Right;
        }

        /// <summary>Override the states for a set of buttons.</summary>
        /// <param name="overrides">The button state overrides.</param>
        public GamePadStateBuilder OverrideButtons(IDictionary<SButton, SButtonState> overrides)
        {
            foreach (var pair in overrides)
            {
                bool isDown = pair.Value.IsDown();
                switch (pair.Key)
                {
                    // left thumbstick
                    case SButton.LeftThumbstickUp:
                        this.LeftStickPos.Y = isDown ? 1 : 0;
                        break;
                    case SButton.LeftThumbstickDown:
                        this.LeftStickPos.Y = isDown ? 1 : 0;
                        break;
                    case SButton.LeftThumbstickLeft:
                        this.LeftStickPos.X = isDown ? 1 : 0;
                        break;
                    case SButton.LeftThumbstickRight:
                        this.LeftStickPos.X = isDown ? 1 : 0;
                        break;

                    // right thumbstick
                    case SButton.RightThumbstickUp:
                        this.RightStickPos.Y = isDown ? 1 : 0;
                        break;
                    case SButton.RightThumbstickDown:
                        this.RightStickPos.Y = isDown ? 1 : 0;
                        break;
                    case SButton.RightThumbstickLeft:
                        this.RightStickPos.X = isDown ? 1 : 0;
                        break;
                    case SButton.RightThumbstickRight:
                        this.RightStickPos.X = isDown ? 1 : 0;
                        break;

                    // triggers
                    case SButton.LeftTrigger:
                        this.LeftTrigger = isDown ? 1 : 0;
                        break;
                    case SButton.RightTrigger:
                        this.RightTrigger = isDown ? 1 : 0;
                        break;

                    // buttons
                    default:
                        if (this.ButtonStates.ContainsKey(pair.Key))
                            this.ButtonStates[pair.Key] = isDown ? ButtonState.Pressed : ButtonState.Released;
                        break;
                }
            }

            return this;
        }

        /// <summary>Construct an equivalent state.</summary>
        public GamePadState ToState()
        {
            return new GamePadState(
                leftThumbStick: this.LeftStickPos,
                rightThumbStick: this.RightStickPos,
                leftTrigger: this.LeftTrigger,
                rightTrigger: this.RightTrigger,
                buttons: this.GetBitmask(this.GetPressedButtons()) // MonoDevelop requires one bitmask here; don't specify multiple values
            );
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get all pressed buttons.</summary>
        private IEnumerable<Buttons> GetPressedButtons()
        {
            foreach (var pair in this.ButtonStates)
            {
                if (pair.Value == ButtonState.Pressed && pair.Key.TryGetController(out Buttons button))
                    yield return button;
            }
        }

        /// <summary>Get a bitmask representing the given buttons.</summary>
        /// <param name="buttons">The buttons to represent.</param>
        private Buttons GetBitmask(IEnumerable<Buttons> buttons)
        {
            Buttons flag = 0;
            foreach (Buttons button in buttons)
                flag |= button;
            return flag;
        }
    }
}
