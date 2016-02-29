using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;

namespace StardewModdingAPI.Inheritance
{
    public class SGame : Game1
    {
        public static FieldInfo[] StaticFields { get { return Thing(); } }

        public static FieldInfo[] Thing()
        {
            return typeof(Game1).GetFields();
        }

        public KeyboardState KStateNow { get; private set; }
        public KeyboardState KStatePrior { get; private set; }

        public Keys[] CurrentlyPressedKeys { get; private set; }
        public Keys[] PreviouslyPressedKeys { get; private set; }

        public Keys[] FramePressedKeys 
        { 
            get { return CurrentlyPressedKeys.Where(x => !PreviouslyPressedKeys.Contains(x)).ToArray(); }
        }

        protected override void Initialize()
        {
            Program.Log("XNA Initialize");
            Events.InvokeInitialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Program.Log("XNA LoadContent");
            Events.InvokeLoadContent();
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            KStateNow = Keyboard.GetState();
            CurrentlyPressedKeys = KStateNow.GetPressedKeys();

            foreach (Keys k in FramePressedKeys)
                Events.InvokeKeyPressed(k);

            if (KStateNow != KStatePrior)
            {
                Events.InvokeKeyboardChanged(KStateNow);
            }

            Events.InvokeUpdateTick();
            base.Update(gameTime);

            KStatePrior = KStateNow;
            PreviouslyPressedKeys = CurrentlyPressedKeys;
        }

        protected override void Draw(GameTime gameTime)
        {
            Events.InvokeDrawTick();
            base.Draw(gameTime);
        }
    }
}