using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Framework.Input
{
    /// <summary>Manipulates keyboard state.</summary>
    internal class KeyboardStateBuilder
    {
        /*********
        ** Fields
        *********/
        /// <summary>The pressed buttons.</summary>
        private readonly HashSet<Keys> PressedButtons;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="state">The initial state.</param>
        public KeyboardStateBuilder(KeyboardState state)
        {
            this.PressedButtons = new HashSet<Keys>(state.GetPressedKeys());
        }

        /// <summary>Override the states for a set of buttons.</summary>
        /// <param name="overrides">The button state overrides.</param>
        public KeyboardStateBuilder OverrideButtons(IDictionary<SButton, SButtonState> overrides)
        {
            foreach (var pair in overrides)
            {
                if (pair.Key.TryGetKeyboard(out Keys key))
                {
                    if (pair.Value.IsDown())
                        this.PressedButtons.Add(key);
                    else
                        this.PressedButtons.Remove(key);
                }
            }

            return this;
        }

        /// <summary>Build an equivalent state.</summary>
        public KeyboardState ToState()
        {
            return new KeyboardState(this.PressedButtons.ToArray());
        }
    }
}
