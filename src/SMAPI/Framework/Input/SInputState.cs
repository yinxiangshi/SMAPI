using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;

#pragma warning disable 809 // obsolete override of non-obsolete method (this is deliberate)
namespace StardewModdingAPI.Framework.Input
{
    /// <summary>Manages the game's input state.</summary>
    internal sealed class SInputState : InputState
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The maximum amount of direction to ignore for the left thumbstick.</summary>
        private const float LeftThumbstickDeadZone = 0.2f;

        /// <summary>The cursor position on the screen adjusted for the zoom level.</summary>
        private CursorPosition CursorPositionImpl;

        /// <summary>The player's last known tile position.</summary>
        private Vector2? LastPlayerTile;

        /// <summary>The buttons to press until the game next handles input.</summary>
        private readonly HashSet<SButton> CustomPressedKeys = new HashSet<SButton>();

        /// <summary>The buttons to consider released until the actual button is released.</summary>
        private readonly HashSet<SButton> CustomReleasedKeys = new HashSet<SButton>();

        /// <summary>The buttons which were actually down as of the last update, ignoring overrides.</summary>
        private HashSet<SButton> LastRealButtonPresses = new HashSet<SButton>();


        /*********
        ** Accessors
        *********/
        /// <summary>The controller state as of the last update.</summary>
        public GamePadState LastController { get; private set; }

        /// <summary>The keyboard state as of the last update.</summary>
        public KeyboardState LastKeyboard { get; private set; }

        /// <summary>The mouse state as of the last update.</summary>
        public MouseState LastMouse { get; private set; }

        /// <summary>The buttons which were pressed, held, or released as of the last update.</summary>
        public IDictionary<SButton, SButtonState> LastButtonStates { get; private set; } = new Dictionary<SButton, SButtonState>();

        /// <summary>The cursor position on the screen adjusted for the zoom level.</summary>
        public ICursorPosition CursorPosition => this.CursorPositionImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Get a copy of the current state.</summary>
        public SInputState Clone()
        {
            return new SInputState
            {
                LastButtonStates = this.LastButtonStates,
                LastController = this.LastController,
                LastKeyboard = this.LastKeyboard,
                LastMouse = this.LastMouse,
                CursorPositionImpl = this.CursorPositionImpl
            };
        }

        /// <summary>Override the state for a button.</summary>
        /// <param name="button">The button to override.</param>
        /// <param name="setDown">Whether to mark it pressed; else mark it released.</param>
        public void OverrideButton(SButton button, bool setDown)
        {
            if (setDown)
            {
                this.CustomPressedKeys.Add(button);
                this.CustomReleasedKeys.Remove(button);
            }
            else
            {
                this.CustomPressedKeys.Remove(button);
                this.CustomReleasedKeys.Add(button);
            }
        }

        /// <summary>Get whether a mod has indicated the key was already handled, so the game shouldn't handle it.</summary>
        /// <param name="button">The button to check.</param>
        public bool IsSuppressed(SButton button)
        {
            return this.CustomReleasedKeys.Contains(button);
        }

        /// <summary>This method is called by the game, and does nothing since SMAPI will already have updated by that point.</summary>
        [Obsolete("This method should only be called by the game itself.")]
        public override void Update() { }

        /// <summary>Update the current button states for the given tick.</summary>
        public void TrueUpdate()
        {
            try
            {
                float zoomMultiplier = (1f / Game1.options.zoomLevel);

                // get real values
                GamePadState controller = GamePad.GetState(PlayerIndex.One);
                KeyboardState keyboard = Keyboard.GetState();
                MouseState mouse = Mouse.GetState();
                Vector2 cursorAbsolutePos = new Vector2((mouse.X * zoomMultiplier) + Game1.viewport.X, (mouse.Y * zoomMultiplier) + Game1.viewport.Y);
                Vector2? playerTilePos = Context.IsPlayerFree ? Game1.player.getTileLocation() : (Vector2?)null;
                HashSet<SButton> reallyDown = new HashSet<SButton>(this.GetPressedButtons(keyboard, mouse, controller));

                // apply overrides
                bool hasOverrides = false;
                if (this.CustomPressedKeys.Count > 0 || this.CustomReleasedKeys.Count > 0)
                {
                    // reset overrides that no longer apply
                    this.CustomPressedKeys.RemoveWhere(key => reallyDown.Contains(key));
                    this.CustomReleasedKeys.RemoveWhere(key => !reallyDown.Contains(key));

                    // apply overrides
                    if (this.ApplyOverrides(this.CustomPressedKeys, this.CustomReleasedKeys, ref keyboard, ref mouse, ref controller))
                        hasOverrides = true;

                    // remove pressed keys
                    this.CustomPressedKeys.Clear();
                }

                // get button states
                var pressedButtons = hasOverrides
                    ? new HashSet<SButton>(this.GetPressedButtons(keyboard, mouse, controller))
                    : reallyDown;
                var activeButtons = this.DeriveStates(this.LastButtonStates, pressedButtons, keyboard, mouse, controller);

                // update
                this.LastController = controller;
                this.LastKeyboard = keyboard;
                this.LastMouse = mouse;
                this.LastButtonStates = activeButtons;
                this.LastRealButtonPresses = reallyDown;
                if (cursorAbsolutePos != this.CursorPositionImpl?.AbsolutePixels || playerTilePos != this.LastPlayerTile)
                {
                    this.LastPlayerTile = playerTilePos;
                    this.CursorPositionImpl = this.GetCursorPosition(mouse, cursorAbsolutePos, zoomMultiplier);
                }
            }
            catch (InvalidOperationException)
            {
                // GetState() may crash for some players if window doesn't have focus but game1.IsActive == true
            }
        }

        /// <summary>Apply input overrides to the current state.</summary>
        public void ApplyOverrides()
        {
            GamePadState newController = this.LastController;
            KeyboardState newKeyboard = this.LastKeyboard;
            MouseState newMouse = this.LastMouse;

            this.ApplyOverrides(pressed: this.CustomPressedKeys, released: this.CustomReleasedKeys, ref newKeyboard, ref newMouse, ref newController);

            this.LastController = newController;
            this.LastKeyboard = newKeyboard;
            this.LastMouse = newMouse;
        }

        /// <summary>Get the gamepad state visible to the game.</summary>
        [Obsolete("This method should only be called by the game itself.")]
        public override GamePadState GetGamePadState()
        {
            if (Game1.options.gamepadMode == Options.GamepadModes.ForceOff)
                return new GamePadState();

            return this.LastController;
        }

        /// <summary>Get the keyboard state visible to the game.</summary>
        [Obsolete("This method should only be called by the game itself.")]
        public override KeyboardState GetKeyboardState()
        {
            return this.LastKeyboard;
        }

        /// <summary>Get the keyboard state visible to the game.</summary>
        [Obsolete("This method should only be called by the game itself.")]
        public override MouseState GetMouseState()
        {
            return this.LastMouse;
        }

        /// <summary>Get whether a given button was pressed or held.</summary>
        /// <param name="button">The button to check.</param>
        public bool IsDown(SButton button)
        {
            return this.GetState(this.LastButtonStates, button).IsDown();
        }

        /// <summary>Get whether any of the given buttons were pressed or held.</summary>
        /// <param name="buttons">The buttons to check.</param>
        public bool IsAnyDown(InputButton[] buttons)
        {
            return buttons.Any(button => this.IsDown(button.ToSButton()));
        }

        /// <summary>Get the state of a button.</summary>
        /// <param name="button">The button to check.</param>
        public SButtonState GetState(SButton button)
        {
            return this.GetState(this.LastButtonStates, button);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the current cursor position.</summary>
        /// <param name="mouseState">The current mouse state.</param>
        /// <param name="absolutePixels">The absolute pixel position relative to the map, adjusted for pixel zoom.</param>
        /// <param name="zoomMultiplier">The multiplier applied to pixel coordinates to adjust them for pixel zoom.</param>
        private CursorPosition GetCursorPosition(MouseState mouseState, Vector2 absolutePixels, float zoomMultiplier)
        {
            Vector2 screenPixels = new Vector2(mouseState.X * zoomMultiplier, mouseState.Y * zoomMultiplier);
            Vector2 tile = new Vector2((int)((Game1.viewport.X + screenPixels.X) / Game1.tileSize), (int)((Game1.viewport.Y + screenPixels.Y) / Game1.tileSize));
            Vector2 grabTile = (Game1.mouseCursorTransparency > 0 && Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, 1, Game1.player)) // derived from Game1.pressActionButton
                ? tile
                : Game1.player.GetGrabTile();
            return new CursorPosition(absolutePixels, screenPixels, tile, grabTile);
        }

        /// <summary>Apply input overrides to the given states.</summary>
        /// <param name="pressed">The buttons to mark pressed.</param>
        /// <param name="released">The buttons to mark released.</param>
        /// <param name="keyboardState">The game's keyboard state for the current tick.</param>
        /// <param name="mouseState">The game's mouse state for the current tick.</param>
        /// <param name="gamePadState">The game's controller state for the current tick.</param>
        /// <returns>Returns whether any overrides were applied.</returns>
        private bool ApplyOverrides(ISet<SButton> pressed, ISet<SButton> released, ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState)
        {
            if (pressed.Count == 0 && released.Count == 0)
                return false;

            // group keys by type
            IDictionary<SButton, SButtonState> keyboardOverrides = new Dictionary<SButton, SButtonState>();
            IDictionary<SButton, SButtonState> controllerOverrides = new Dictionary<SButton, SButtonState>();
            IDictionary<SButton, SButtonState> mouseOverrides = new Dictionary<SButton, SButtonState>();
            foreach (var button in pressed.Concat(released))
            {
                var newState = this.DeriveState(
                    oldState: this.GetState(button),
                    isDown: pressed.Contains(button)
                );

                if (button == SButton.MouseLeft || button == SButton.MouseMiddle || button == SButton.MouseRight || button == SButton.MouseX1 || button == SButton.MouseX2)
                    mouseOverrides[button] = newState;
                else if (button.TryGetKeyboard(out Keys _))
                    keyboardOverrides[button] = newState;
                else if (gamePadState.IsConnected && button.TryGetController(out Buttons _))
                    controllerOverrides[button] = newState;
            }

            // override states
            if (keyboardOverrides.Any())
                keyboardState = new KeyboardStateBuilder(keyboardState).OverrideButtons(keyboardOverrides).ToState();
            if (gamePadState.IsConnected && controllerOverrides.Any())
                gamePadState = new GamePadStateBuilder(gamePadState).OverrideButtons(controllerOverrides).ToState();
            if (mouseOverrides.Any())
                mouseState = new MouseStateBuilder(mouseState).OverrideButtons(mouseOverrides).ToMouseState();

            return true;
        }

        /// <summary>Get the state of all pressed or released buttons relative to their previous state.</summary>
        /// <param name="previousStates">The previous button states.</param>
        /// <param name="pressedButtons">The currently pressed buttons.</param>
        /// <param name="keyboard">The keyboard state.</param>
        /// <param name="mouse">The mouse state.</param>
        /// <param name="controller">The controller state.</param>
        private IDictionary<SButton, SButtonState> DeriveStates(IDictionary<SButton, SButtonState> previousStates, HashSet<SButton> pressedButtons, KeyboardState keyboard, MouseState mouse, GamePadState controller)
        {
            IDictionary<SButton, SButtonState> activeButtons = new Dictionary<SButton, SButtonState>();

            // handle pressed keys
            foreach (SButton button in pressedButtons)
                activeButtons[button] = this.DeriveState(this.GetState(previousStates, button), isDown: true);

            // handle released keys
            foreach (KeyValuePair<SButton, SButtonState> prev in previousStates)
            {
                if (prev.Value.IsDown() && !activeButtons.ContainsKey(prev.Key))
                    activeButtons[prev.Key] = SButtonState.Released;
            }

            return activeButtons;
        }

        /// <summary>Get the state of a button relative to its previous state.</summary>
        /// <param name="oldState">The previous button state.</param>
        /// <param name="isDown">Whether the button is currently down.</param>
        private SButtonState DeriveState(SButtonState oldState, bool isDown)
        {
            if (isDown && oldState.IsDown())
                return SButtonState.Held;
            if (isDown)
                return SButtonState.Pressed;
            return SButtonState.Released;
        }

        /// <summary>Get the state of a button.</summary>
        /// <param name="activeButtons">The current button states to check.</param>
        /// <param name="button">The button to check.</param>
        private SButtonState GetState(IDictionary<SButton, SButtonState> activeButtons, SButton button)
        {
            return activeButtons.TryGetValue(button, out SButtonState state) ? state : SButtonState.None;
        }

        /// <summary>Get the buttons pressed in the given stats.</summary>
        /// <param name="keyboard">The keyboard state.</param>
        /// <param name="mouse">The mouse state.</param>
        /// <param name="controller">The controller state.</param>
        /// <remarks>Thumbstick direction logic derived from <see cref="ButtonCollection"/>.</remarks>
        private IEnumerable<SButton> GetPressedButtons(KeyboardState keyboard, MouseState mouse, GamePadState controller)
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
                // main buttons
                if (controller.Buttons.A == ButtonState.Pressed)
                    yield return SButton.ControllerA;
                if (controller.Buttons.B == ButtonState.Pressed)
                    yield return SButton.ControllerB;
                if (controller.Buttons.X == ButtonState.Pressed)
                    yield return SButton.ControllerX;
                if (controller.Buttons.Y == ButtonState.Pressed)
                    yield return SButton.ControllerY;
                if (controller.Buttons.LeftStick == ButtonState.Pressed)
                    yield return SButton.LeftStick;
                if (controller.Buttons.RightStick == ButtonState.Pressed)
                    yield return SButton.RightStick;
                if (controller.Buttons.Start == ButtonState.Pressed)
                    yield return SButton.ControllerStart;

                // directional pad
                if (controller.DPad.Up == ButtonState.Pressed)
                    yield return SButton.DPadUp;
                if (controller.DPad.Down == ButtonState.Pressed)
                    yield return SButton.DPadDown;
                if (controller.DPad.Left == ButtonState.Pressed)
                    yield return SButton.DPadLeft;
                if (controller.DPad.Right == ButtonState.Pressed)
                    yield return SButton.DPadRight;

                // secondary buttons
                if (controller.Buttons.Back == ButtonState.Pressed)
                    yield return SButton.ControllerBack;
                if (controller.Buttons.BigButton == ButtonState.Pressed)
                    yield return SButton.BigButton;

                // shoulders
                if (controller.Buttons.LeftShoulder == ButtonState.Pressed)
                    yield return SButton.LeftShoulder;
                if (controller.Buttons.RightShoulder == ButtonState.Pressed)
                    yield return SButton.RightShoulder;

                // triggers
                if (controller.Triggers.Left > 0.2f)
                    yield return SButton.LeftTrigger;
                if (controller.Triggers.Right > 0.2f)
                    yield return SButton.RightTrigger;

                // left thumbstick direction
                if (controller.ThumbSticks.Left.Y > SInputState.LeftThumbstickDeadZone)
                    yield return SButton.LeftThumbstickUp;
                if (controller.ThumbSticks.Left.Y < -SInputState.LeftThumbstickDeadZone)
                    yield return SButton.LeftThumbstickDown;
                if (controller.ThumbSticks.Left.X > SInputState.LeftThumbstickDeadZone)
                    yield return SButton.LeftThumbstickRight;
                if (controller.ThumbSticks.Left.X < -SInputState.LeftThumbstickDeadZone)
                    yield return SButton.LeftThumbstickLeft;

                // right thumbstick direction
                if (this.IsRightThumbstickOutsideDeadZone(controller.ThumbSticks.Right))
                {
                    if (controller.ThumbSticks.Right.Y > 0)
                        yield return SButton.RightThumbstickUp;
                    if (controller.ThumbSticks.Right.Y < 0)
                        yield return SButton.RightThumbstickDown;
                    if (controller.ThumbSticks.Right.X > 0)
                        yield return SButton.RightThumbstickRight;
                    if (controller.ThumbSticks.Right.X < 0)
                        yield return SButton.RightThumbstickLeft;
                }
            }
        }

        /// <summary>Get whether the right thumbstick should be considered outside the dead zone.</summary>
        /// <param name="direction">The right thumbstick value.</param>
        private bool IsRightThumbstickOutsideDeadZone(Vector2 direction)
        {
            return direction.Length() > 0.9f;
        }
    }
}
