using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace StardewModdingAPI.Framework.Input
{
    /// <summary>A summary of input changes during an update frame.</summary>
    internal class InputState
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying controller state.</summary>
        public GamePadState ControllerState { get; }

        /// <summary>The underlying keyboard state.</summary>
        public KeyboardState KeyboardState { get; }

        /// <summary>The underlying mouse state.</summary>
        public MouseState MouseState { get; }

        /// <summary>The mouse position on the screen adjusted for the zoom level.</summary>
        public Point MousePosition { get; }

        /// <summary>The buttons which were pressed, held, or released.</summary>
        public IDictionary<SButton, InputStatus> ActiveButtons { get; } = new Dictionary<SButton, InputStatus>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an empty instance.</summary>
        public InputState() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="previousState">The previous input state.</param>
        /// <param name="controllerState">The current controller state.</param>
        /// <param name="keyboardState">The current keyboard state.</param>
        /// <param name="mouseState">The current mouse state.</param>
        public InputState(InputState previousState, GamePadState controllerState, KeyboardState keyboardState, MouseState mouseState)
        {
            // init properties
            this.ControllerState = controllerState;
            this.KeyboardState = keyboardState;
            this.MouseState = mouseState;
            this.MousePosition = new Point((int)(mouseState.X * (1.0 / Game1.options.zoomLevel)), (int)(mouseState.Y * (1.0 / Game1.options.zoomLevel))); // derived from Game1::getMouseX

            // get button states
            SButton[] down = InputState.GetPressedButtons(keyboardState, mouseState, controllerState).ToArray();
            foreach (SButton button in down)
                this.ActiveButtons[button] = this.GetStatus(previousState.GetStatus(button), isDown: true);
            foreach (KeyValuePair<SButton, InputStatus> prev in previousState.ActiveButtons)
            {
                if (prev.Value.IsDown() && !this.ActiveButtons.ContainsKey(prev.Key))
                    this.ActiveButtons[prev.Key] = InputStatus.Released;
            }
        }

        /// <summary>Get the status of a button.</summary>
        /// <param name="button">The button to check.</param>
        public InputStatus GetStatus(SButton button)
        {
            return this.ActiveButtons.TryGetValue(button, out InputStatus status) ? status : InputStatus.None;
        }

        /// <summary>Get whether a given button was pressed or held.</summary>
        /// <param name="button">The button to check.</param>
        public bool IsDown(SButton button)
        {
            return this.GetStatus(button).IsDown();
        }

        /// <summary>Get the current input state.</summary>
        /// <param name="previousState">The previous input state.</param>
        public static InputState GetState(InputState previousState)
        {
            GamePadState controllerState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            return new InputState(previousState, controllerState, keyboardState, mouseState);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get the status of a button.</summary>
        /// <param name="oldStatus">The previous button status.</param>
        /// <param name="isDown">Whether the button is currently down.</param>
        public InputStatus GetStatus(InputStatus oldStatus, bool isDown)
        {
            if (isDown && oldStatus.IsDown())
                return InputStatus.Held;
            if (isDown)
                return InputStatus.Pressed;
            return InputStatus.Released;
        }

        /// <summary>Get the buttons pressed in the given stats.</summary>
        /// <param name="keyboard">The keyboard state.</param>
        /// <param name="mouse">The mouse state.</param>
        /// <param name="controller">The controller state.</param>
        private static IEnumerable<SButton> GetPressedButtons(KeyboardState keyboard, MouseState mouse, GamePadState controller)
        {
            // keyboard
            foreach (Keys key in keyboard.GetPressedKeys())
                yield return key.ToSButton();

            // mouse
            if (mouse.LeftButton == ButtonState.Pressed)
                yield return SButton.MouseLeft;
            if (mouse.RightButton == ButtonState.Pressed)
                yield return SButton.MouseRight;
            if (mouse.MiddleButton == ButtonState.Pressed)
                yield return SButton.MouseMiddle;
            if (mouse.XButton1 == ButtonState.Pressed)
                yield return SButton.MouseX1;
            if (mouse.XButton2 == ButtonState.Pressed)
                yield return SButton.MouseX2;

            // controller
            if (controller.IsConnected)
            {
                if (controller.Buttons.A == ButtonState.Pressed)
                    yield return SButton.ControllerA;
                if (controller.Buttons.B == ButtonState.Pressed)
                    yield return SButton.ControllerB;
                if (controller.Buttons.Back == ButtonState.Pressed)
                    yield return SButton.ControllerBack;
                if (controller.Buttons.BigButton == ButtonState.Pressed)
                    yield return SButton.BigButton;
                if (controller.Buttons.LeftShoulder == ButtonState.Pressed)
                    yield return SButton.LeftShoulder;
                if (controller.Buttons.LeftStick == ButtonState.Pressed)
                    yield return SButton.LeftStick;
                if (controller.Buttons.RightShoulder == ButtonState.Pressed)
                    yield return SButton.RightShoulder;
                if (controller.Buttons.RightStick == ButtonState.Pressed)
                    yield return SButton.RightStick;
                if (controller.Buttons.Start == ButtonState.Pressed)
                    yield return SButton.ControllerStart;
                if (controller.Buttons.X == ButtonState.Pressed)
                    yield return SButton.ControllerX;
                if (controller.Buttons.Y == ButtonState.Pressed)
                    yield return SButton.ControllerY;
                if (controller.DPad.Up == ButtonState.Pressed)
                    yield return SButton.DPadUp;
                if (controller.DPad.Down == ButtonState.Pressed)
                    yield return SButton.DPadDown;
                if (controller.DPad.Left == ButtonState.Pressed)
                    yield return SButton.DPadLeft;
                if (controller.DPad.Right == ButtonState.Pressed)
                    yield return SButton.DPadRight;
                if (controller.Triggers.Left > 0.2f)
                    yield return SButton.LeftTrigger;
                if (controller.Triggers.Right > 0.2f)
                    yield return SButton.RightTrigger;
            }
        }
    }
}
