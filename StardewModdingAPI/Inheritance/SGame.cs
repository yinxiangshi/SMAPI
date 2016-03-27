using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewModdingAPI.Inheritance
{
    /// <summary>
    /// The 'SGame' class.
    /// This summary, and many others, only exists because XML doc tags.
    /// </summary>
    public class SGame : Game1
    {
        /// <summary>
        /// Useless right now.
        /// </summary>
        public const int LowestModItemID = 1000;

        private bool FireLoadedGameEvent;

        /// <summary>
        /// Gets a jagged array of all buttons pressed on the gamepad the prior frame.
        /// </summary>
        public Buttons[][] PreviouslyPressedButtons;

        internal SGame()
        {
            Instance = this;
            FirstUpdate = true;
        }

        /// <summary>
        /// Useless at this time.
        /// </summary>
        [Obsolete]
        public static Dictionary<int, SObject> ModItems { get; private set; }

        /// <summary>
        /// The current KeyboardState
        /// </summary>
        public KeyboardState KStateNow { get; private set; }
        /// <summary>
        /// The prior KeyboardState
        /// </summary>
        public KeyboardState KStatePrior { get; private set; }

        /// <summary>
        /// The current MouseState
        /// </summary>
        public MouseState MStateNow { get; private set; }

        /// <summary>
        /// The prior MouseState
        /// </summary>
        public MouseState MStatePrior { get; private set; }

        /// <summary>
        /// All keys pressed on the current frame
        /// </summary>
        public Keys[] CurrentlyPressedKeys => KStateNow.GetPressedKeys();

        /// <summary>
        /// All keys pressed on the prior frame
        /// </summary>
        public Keys[] PreviouslyPressedKeys => KStatePrior.GetPressedKeys();

        /// <summary>
        /// All keys pressed on this frame except for the ones pressed on the prior frame
        /// </summary>
        public Keys[] FramePressedKeys => CurrentlyPressedKeys.Except(PreviouslyPressedKeys).ToArray();

        /// <summary>
        /// All keys pressed on the prior frame except for the ones pressed on the current frame
        /// </summary>
        public Keys[] FrameReleasedKeys => PreviouslyPressedKeys.Except(CurrentlyPressedKeys).ToArray();

        /// <summary>
        /// Whether or not a save was tagged as 'Loaded' the prior frame.
        /// </summary>
        public bool PreviouslyLoadedGame { get; private set; }

        /// <summary>
        /// The list of GameLocations on the prior frame
        /// </summary>
        public int PreviousGameLocations { get; private set; }

        /// <summary>
        /// The list of GameObjects on the prior frame
        /// </summary>
        public int PreviousLocationObjects { get; private set; }

        /// <summary>
        /// The list of Items in the player's inventory on the prior frame
        /// </summary>
        public Dictionary<Item, int> PreviousItems { get; private set; }

        /// <summary>
        /// The player's Combat level on the prior frame
        /// </summary>
        public int PreviousCombatLevel { get; private set; }
        /// <summary>
        /// The player's Farming level on the prior frame
        /// </summary>
        public int PreviousFarmingLevel { get; private set; }
        /// <summary>
        /// The player's Fishing level on the prior frame
        /// </summary>
        public int PreviousFishingLevel { get; private set; }
        /// <summary>
        /// The player's Foraging level on the prior frame
        /// </summary>
        public int PreviousForagingLevel { get; private set; }
        /// <summary>
        /// The player's Mining level on the prior frame
        /// </summary>
        public int PreviousMiningLevel { get; private set; }
        /// <summary>
        /// The player's Luck level on the prior frame
        /// </summary>
        public int PreviousLuckLevel { get; private set; }

        //Kill me now comments are so boring

        /// <summary>
        /// The player's previous game location
        /// </summary>
        public GameLocation PreviousGameLocation { get; private set; }

        /// <summary>
        /// The previous ActiveGameMenu in Game1
        /// </summary>
        public IClickableMenu PreviousActiveMenu { get; private set; }

        /// <summary>
        /// The previous mine level
        /// </summary>
        public int PreviousMineLevel { get; private set; }

        /// <summary>
        /// The previous TimeOfDay (Int32 between 600 and 2400?)
        /// </summary>
        public int PreviousTimeOfDay { get; private set; }

        /// <summary>
        /// The previous DayOfMonth (Int32 between 1 and 28?)
        /// </summary>
        public int PreviousDayOfMonth { get; private set; }

        /// <summary>
        /// The previous Season (String as follows: "winter", "spring", "summer", "fall")
        /// </summary>
        public string PreviousSeasonOfYear { get; private set; }

        /// <summary>
        /// The previous Year
        /// </summary>
        public int PreviousYearOfGame { get; private set; }

        /// <summary>
        /// The previous 'Farmer' (Player)
        /// </summary>
        public Farmer PreviousFarmer { get; private set; }

        /// <summary>
        /// The current index of the update tick. Recycles every 60th tick to 0. (Int32 between 0 and 59)
        /// </summary>
        public int CurrentUpdateTick { get; private set; }

        /// <summary>
        /// Whether or not this update frame is the very first of the entire game
        /// </summary>
        public bool FirstUpdate { get; private set; }

        /// <summary>
        /// The current RenderTarget in Game1 (Private field, uses reflection)
        /// </summary>
        public RenderTarget2D Screen
        {
            get { return typeof (Game1).GetBaseFieldValue<RenderTarget2D>(Program.gamePtr, "screen"); }
            set { typeof (Game1).SetBaseFieldValue<RenderTarget2D>(this, "screen", value); }
        }

        /// <summary>
        /// Static accessor for an Instance of the class SGame
        /// </summary>
        public static SGame Instance { get; private set; }

        /// <summary>
        /// The game's FPS. Re-determined every Draw update.
        /// </summary>
        public static float FramesPerSecond { get; private set; }

        /// <summary>
        /// Whether or not we're in a pseudo 'debug' mode. Mostly for displaying information like FPS.
        /// </summary>
        public static bool Debug { get; private set; }

        /// <summary>
        /// The current player (equal to Farmer.Player)
        /// </summary>
        [Obsolete("Use Farmer.Player instead")]
        public Farmer CurrentFarmer => player;

        /// <summary>
        /// Gets ALL static fields that belong to 'Game1'
        /// </summary>
        public static FieldInfo[] GetStaticFields => typeof (Game1).GetFields();
        
        /// <summary>
        /// Whether or not a button was just pressed on the controller
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonState"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private bool WasButtonJustPressed(Buttons button, ButtonState buttonState, PlayerIndex stateIndex)
        {
            return buttonState == ButtonState.Pressed && !PreviouslyPressedButtons[(int) stateIndex].Contains(button);
        }

        /// <summary>
        /// Whether or not a button was just released on the controller
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonState"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private bool WasButtonJustReleased(Buttons button, ButtonState buttonState, PlayerIndex stateIndex)
        {
            return buttonState == ButtonState.Released && PreviouslyPressedButtons[(int) stateIndex].Contains(button);
        }

        /// <summary>
        /// Whether or not an analog button was just pressed on the controller
        /// </summary>
        /// <param name="button"></param>
        /// <param name="value"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private bool WasButtonJustPressed(Buttons button, float value, PlayerIndex stateIndex)
        {
            return WasButtonJustPressed(button, value > 0.2f ? ButtonState.Pressed : ButtonState.Released, stateIndex);
        }
        
        /// <summary>
        /// Whether or not an analog button was just released on the controller
        /// </summary>
        /// <param name="button"></param>
        /// <param name="value"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private bool WasButtonJustReleased(Buttons button, float value, PlayerIndex stateIndex)
        {
            return WasButtonJustReleased(button, value > 0.2f ? ButtonState.Pressed : ButtonState.Released, stateIndex);
        }

        /// <summary>
        /// Gets an array of all Buttons pressed on a joystick
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Buttons[] GetButtonsDown(PlayerIndex index)
        {
            var state = GamePad.GetState(index);
            var buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (state.Buttons.A == ButtonState.Pressed) buttons.Add(Buttons.A);
                if (state.Buttons.B == ButtonState.Pressed) buttons.Add(Buttons.B);
                if (state.Buttons.Back == ButtonState.Pressed) buttons.Add(Buttons.Back);
                if (state.Buttons.BigButton == ButtonState.Pressed) buttons.Add(Buttons.BigButton);
                if (state.Buttons.LeftShoulder == ButtonState.Pressed) buttons.Add(Buttons.LeftShoulder);
                if (state.Buttons.LeftStick == ButtonState.Pressed) buttons.Add(Buttons.LeftStick);
                if (state.Buttons.RightShoulder == ButtonState.Pressed) buttons.Add(Buttons.RightShoulder);
                if (state.Buttons.RightStick == ButtonState.Pressed) buttons.Add(Buttons.RightStick);
                if (state.Buttons.Start == ButtonState.Pressed) buttons.Add(Buttons.Start);
                if (state.Buttons.X == ButtonState.Pressed) buttons.Add(Buttons.X);
                if (state.Buttons.Y == ButtonState.Pressed) buttons.Add(Buttons.Y);
                if (state.DPad.Up == ButtonState.Pressed) buttons.Add(Buttons.DPadUp);
                if (state.DPad.Down == ButtonState.Pressed) buttons.Add(Buttons.DPadDown);
                if (state.DPad.Left == ButtonState.Pressed) buttons.Add(Buttons.DPadLeft);
                if (state.DPad.Right == ButtonState.Pressed) buttons.Add(Buttons.DPadRight);
                if (state.Triggers.Left > 0.2f) buttons.Add(Buttons.LeftTrigger);
                if (state.Triggers.Right > 0.2f) buttons.Add(Buttons.RightTrigger);
            }
            return buttons.ToArray();
        }

        /// <summary>
        /// Gets all buttons that were pressed on the current frame of a joystick
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Buttons[] GetFramePressedButtons(PlayerIndex index)
        {
            var state = GamePad.GetState(index);
            var buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (WasButtonJustPressed(Buttons.A, state.Buttons.A, index)) buttons.Add(Buttons.A);
                if (WasButtonJustPressed(Buttons.B, state.Buttons.B, index)) buttons.Add(Buttons.B);
                if (WasButtonJustPressed(Buttons.Back, state.Buttons.Back, index)) buttons.Add(Buttons.Back);
                if (WasButtonJustPressed(Buttons.BigButton, state.Buttons.BigButton, index)) buttons.Add(Buttons.BigButton);
                if (WasButtonJustPressed(Buttons.LeftShoulder, state.Buttons.LeftShoulder, index)) buttons.Add(Buttons.LeftShoulder);
                if (WasButtonJustPressed(Buttons.LeftStick, state.Buttons.LeftStick, index)) buttons.Add(Buttons.LeftStick);
                if (WasButtonJustPressed(Buttons.RightShoulder, state.Buttons.RightShoulder, index)) buttons.Add(Buttons.RightShoulder);
                if (WasButtonJustPressed(Buttons.RightStick, state.Buttons.RightStick, index)) buttons.Add(Buttons.RightStick);
                if (WasButtonJustPressed(Buttons.Start, state.Buttons.Start, index)) buttons.Add(Buttons.Start);
                if (WasButtonJustPressed(Buttons.X, state.Buttons.X, index)) buttons.Add(Buttons.X);
                if (WasButtonJustPressed(Buttons.Y, state.Buttons.Y, index)) buttons.Add(Buttons.Y);
                if (WasButtonJustPressed(Buttons.DPadUp, state.DPad.Up, index)) buttons.Add(Buttons.DPadUp);
                if (WasButtonJustPressed(Buttons.DPadDown, state.DPad.Down, index)) buttons.Add(Buttons.DPadDown);
                if (WasButtonJustPressed(Buttons.DPadLeft, state.DPad.Left, index)) buttons.Add(Buttons.DPadLeft);
                if (WasButtonJustPressed(Buttons.DPadRight, state.DPad.Right, index)) buttons.Add(Buttons.DPadRight);
                if (WasButtonJustPressed(Buttons.LeftTrigger, state.Triggers.Left, index)) buttons.Add(Buttons.LeftTrigger);
                if (WasButtonJustPressed(Buttons.RightTrigger, state.Triggers.Right, index)) buttons.Add(Buttons.RightTrigger);
            }
            return buttons.ToArray();
        }

        /// <summary>
        /// Gets all buttons that were released on the current frame of a joystick
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Buttons[] GetFrameReleasedButtons(PlayerIndex index)
        {
            var state = GamePad.GetState(index);
            var buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (WasButtonJustReleased(Buttons.A, state.Buttons.A, index)) buttons.Add(Buttons.A);
                if (WasButtonJustReleased(Buttons.B, state.Buttons.B, index)) buttons.Add(Buttons.B);
                if (WasButtonJustReleased(Buttons.Back, state.Buttons.Back, index)) buttons.Add(Buttons.Back);
                if (WasButtonJustReleased(Buttons.BigButton, state.Buttons.BigButton, index)) buttons.Add(Buttons.BigButton);
                if (WasButtonJustReleased(Buttons.LeftShoulder, state.Buttons.LeftShoulder, index)) buttons.Add(Buttons.LeftShoulder);
                if (WasButtonJustReleased(Buttons.LeftStick, state.Buttons.LeftStick, index)) buttons.Add(Buttons.LeftStick);
                if (WasButtonJustReleased(Buttons.RightShoulder, state.Buttons.RightShoulder, index)) buttons.Add(Buttons.RightShoulder);
                if (WasButtonJustReleased(Buttons.RightStick, state.Buttons.RightStick, index)) buttons.Add(Buttons.RightStick);
                if (WasButtonJustReleased(Buttons.Start, state.Buttons.Start, index)) buttons.Add(Buttons.Start);
                if (WasButtonJustReleased(Buttons.X, state.Buttons.X, index)) buttons.Add(Buttons.X);
                if (WasButtonJustReleased(Buttons.Y, state.Buttons.Y, index)) buttons.Add(Buttons.Y);
                if (WasButtonJustReleased(Buttons.DPadUp, state.DPad.Up, index)) buttons.Add(Buttons.DPadUp);
                if (WasButtonJustReleased(Buttons.DPadDown, state.DPad.Down, index)) buttons.Add(Buttons.DPadDown);
                if (WasButtonJustReleased(Buttons.DPadLeft, state.DPad.Left, index)) buttons.Add(Buttons.DPadLeft);
                if (WasButtonJustReleased(Buttons.DPadRight, state.DPad.Right, index)) buttons.Add(Buttons.DPadRight);
                if (WasButtonJustReleased(Buttons.LeftTrigger, state.Triggers.Left, index)) buttons.Add(Buttons.LeftTrigger);
                if (WasButtonJustReleased(Buttons.RightTrigger, state.Triggers.Right, index)) buttons.Add(Buttons.RightTrigger);
            }
            return buttons.ToArray();
        }

        /// <summary>
        /// XNA Init Method
        /// </summary>
        protected override void Initialize()
        {
            Log.AsyncY("XNA Initialize");
            //ModItems = new Dictionary<int, SObject>();
            PreviouslyPressedButtons = new Buttons[4][];
            for (var i = 0; i < 4; ++i) PreviouslyPressedButtons[i] = new Buttons[0];

            base.Initialize();
            GameEvents.InvokeInitialize();
        }

        /// <summary>
        /// XNA LC Method
        /// </summary>
        protected override void LoadContent()
        {
            Log.AsyncY("XNA LoadContent");
            base.LoadContent();
            GameEvents.InvokeLoadContent();
        }

        /// <summary>
        /// XNA Update Method
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            UpdateEventCalls();

            if (FramePressedKeys.Contains(Keys.F3))
            {
                Debug = !Debug;
            }

            try
            {
                base.Update(gameTime);
            }
            catch (Exception ex)
            {
                Log.AsyncR("An error occured in the base update loop: " + ex);
                Console.ReadKey();
            }

            GameEvents.InvokeUpdateTick();
            if (FirstUpdate)
            {
                GameEvents.InvokeFirstUpdateTick();
                FirstUpdate = false;
            }

            if (CurrentUpdateTick % 2 == 0)
                GameEvents.InvokeSecondUpdateTick();

            if (CurrentUpdateTick % 4 == 0)
                GameEvents.InvokeFourthUpdateTick();

            if (CurrentUpdateTick % 8 == 0)
                GameEvents.InvokeEighthUpdateTick();

            if (CurrentUpdateTick % 15 == 0)
                GameEvents.InvokeQuarterSecondTick();

            if (CurrentUpdateTick % 30 == 0)
                GameEvents.InvokeHalfSecondTick();

            if (CurrentUpdateTick % 60 == 0)
                GameEvents.InvokeOneSecondTick();

            CurrentUpdateTick += 1;
            if (CurrentUpdateTick >= 60)
                CurrentUpdateTick = 0;

            if (KStatePrior != KStateNow)
                KStatePrior = KStateNow;

            for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                PreviouslyPressedButtons[(int) i] = GetButtonsDown(i);
            }
        }

        /// <summary>
        /// XNA Draw Method
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            FramesPerSecond = 1 / (float) gameTime.ElapsedGameTime.TotalSeconds;

            try
            {
                base.Draw(gameTime);
            }
            catch (Exception ex)
            {
                Log.AsyncR("An error occured in the base draw loop: " + ex);
                Console.ReadKey();
            }

            GraphicsEvents.InvokeDrawTick();

            if (Constants.EnableDrawingIntoRenderTarget)
            {
                if (!options.zoomLevel.Equals(1.0f))
                {
                    if (Screen.RenderTargetUsage == RenderTargetUsage.DiscardContents)
                    {
                        Screen = new RenderTarget2D(graphics.GraphicsDevice, Math.Min(4096, (int) (Window.ClientBounds.Width * (1.0 / options.zoomLevel))),
                            Math.Min(4096, (int) (Window.ClientBounds.Height * (1.0 / options.zoomLevel))),
                            false, SurfaceFormat.Color, DepthFormat.Depth16, 1, RenderTargetUsage.PreserveContents);
                    }
                    GraphicsDevice.SetRenderTarget(Screen);
                }

                // Not beginning the batch due to inconsistancies with the standard draw tick...
                //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                GraphicsEvents.InvokeDrawInRenderTargetTick();

                //spriteBatch.End();

                //Re-draw the HUD
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if ((displayHUD || eventUp) && currentBillboard == 0 && gameMode == 3 && !freezeControls && !panMode)
                    typeof (Game1).GetMethod("drawHUD", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(Program.gamePtr, null);
                spriteBatch.End();

                if (!options.zoomLevel.Equals(1.0f))
                {
                    GraphicsDevice.SetRenderTarget(null);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    spriteBatch.Draw(Screen, Vector2.Zero, Screen.Bounds, Color.White, 0.0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
                    spriteBatch.End();
                }
            }

            if (Debug)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(dialogueFont, "FPS: " + FramesPerSecond, Vector2.Zero, Color.CornflowerBlue);
                spriteBatch.End();
            }
        }

        [Obsolete("Do not use at this time.")]
        private static int RegisterModItem(SObject modItem)
        {
            if (modItem.HasBeenRegistered)
            {
                Log.AsyncR($"The item {modItem.Name} has already been registered with ID {modItem.RegisteredId}");
                return modItem.RegisteredId;
            }
            var newId = LowestModItemID;
            if (ModItems.Count > 0)
                newId = Math.Max(LowestModItemID, ModItems.OrderBy(x => x.Key).First().Key + 1);
            ModItems.Add(newId, modItem);
            modItem.HasBeenRegistered = true;
            modItem.RegisteredId = newId;
            return newId;
        }

        [Obsolete("Do not use at this time.")]
        private static SObject PullModItemFromDict(int id, bool isIndex)
        {
            if (isIndex)
            {
                if (ModItems.ElementAtOrDefault(id).Value != null)
                {
                    return ModItems.ElementAt(id).Value.Clone();
                }
                Log.AsyncR("ModItem Dictionary does not contain index: " + id);
                return null;
            }
            if (ModItems.ContainsKey(id))
            {
                return ModItems[id].Clone();
            }
            Log.AsyncR("ModItem Dictionary does not contain ID: " + id);
            return null;
        }

        private void UpdateEventCalls()
        {
            KStateNow = Keyboard.GetState();

            MStateNow = Mouse.GetState();

            foreach (var k in FramePressedKeys)
                ControlEvents.InvokeKeyPressed(k);

            foreach (var k in FrameReleasedKeys)
                ControlEvents.InvokeKeyReleased(k);

            for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                var buttons = GetFramePressedButtons(i);
                foreach (var b in buttons)
                {
                    if (b == Buttons.LeftTrigger || b == Buttons.RightTrigger)
                    {
                        ControlEvents.InvokeTriggerPressed(i, b, b == Buttons.LeftTrigger ? GamePad.GetState(i).Triggers.Left : GamePad.GetState(i).Triggers.Right);
                    }
                    else
                    {
                        ControlEvents.InvokeButtonPressed(i, b);
                    }
                }
            }

            for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                foreach (var b in GetFrameReleasedButtons(i))
                {
                    if (b == Buttons.LeftTrigger || b == Buttons.RightTrigger)
                    {
                        ControlEvents.InvokeTriggerReleased(i, b, b == Buttons.LeftTrigger ? GamePad.GetState(i).Triggers.Left : GamePad.GetState(i).Triggers.Right);
                    }
                    else
                    {
                        ControlEvents.InvokeButtonReleased(i, b);
                    }
                }
            }


            if (KStateNow != KStatePrior)
            {
                ControlEvents.InvokeKeyboardChanged(KStatePrior, KStateNow);
            }

            if (MStateNow != MStatePrior)
            {
                ControlEvents.InvokeMouseChanged(MStatePrior, MStateNow);
                MStatePrior = MStateNow;
            }

            if (activeClickableMenu != null && activeClickableMenu != PreviousActiveMenu)
            {
                MenuEvents.InvokeMenuChanged(PreviousActiveMenu, activeClickableMenu);
                PreviousActiveMenu = activeClickableMenu;
            }

            if (locations.GetHash() != PreviousGameLocations)
            {
                LocationEvents.InvokeLocationsChanged(locations);
                PreviousGameLocations = locations.GetHash();
            }

            if (currentLocation != PreviousGameLocation)
            {
                LocationEvents.InvokeCurrentLocationChanged(PreviousGameLocation, currentLocation);
                PreviousGameLocation = currentLocation;
            }

            if (player != null && player != PreviousFarmer)
            {
                PlayerEvents.InvokeFarmerChanged(PreviousFarmer, player);
                PreviousFarmer = player;
            }

            if (player != null && player.combatLevel != PreviousCombatLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Combat, player.combatLevel);
                PreviousCombatLevel = player.combatLevel;
            }

            if (player != null && player.farmingLevel != PreviousFarmingLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Farming, player.farmingLevel);
                PreviousFarmingLevel = player.farmingLevel;
            }

            if (player != null && player.fishingLevel != PreviousFishingLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Fishing, player.fishingLevel);
                PreviousFishingLevel = player.fishingLevel;
            }

            if (player != null && player.foragingLevel != PreviousForagingLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Foraging, player.foragingLevel);
                PreviousForagingLevel = player.foragingLevel;
            }

            if (player != null && player.miningLevel != PreviousMiningLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Mining, player.miningLevel);
                PreviousMiningLevel = player.miningLevel;
            }

            if (player != null && player.luckLevel != PreviousLuckLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Luck, player.luckLevel);
                PreviousLuckLevel = player.luckLevel;
            }

            List<ItemStackChange> changedItems;
            if (player != null && HasInventoryChanged(player.items, out changedItems))
            {
                PlayerEvents.InvokeInventoryChanged(player.items, changedItems);
                PreviousItems = player.items.Where(n => n != null).ToDictionary(n => n, n => n.Stack);
            }

            var objectHash = currentLocation?.objects?.GetHash();
            if (objectHash != null && PreviousLocationObjects != objectHash)
            {
                LocationEvents.InvokeOnNewLocationObject(currentLocation.objects);
                PreviousLocationObjects = objectHash ?? -1;
            }

            if (timeOfDay != PreviousTimeOfDay)
            {
                TimeEvents.InvokeTimeOfDayChanged(PreviousTimeOfDay, timeOfDay);
                PreviousTimeOfDay = timeOfDay;
            }

            if (dayOfMonth != PreviousDayOfMonth)
            {
                TimeEvents.InvokeDayOfMonthChanged(PreviousDayOfMonth, dayOfMonth);
                PreviousDayOfMonth = dayOfMonth;
            }

            if (currentSeason != PreviousSeasonOfYear)
            {
                TimeEvents.InvokeSeasonOfYearChanged(PreviousSeasonOfYear, currentSeason);
                PreviousSeasonOfYear = currentSeason;
            }

            if (year != PreviousYearOfGame)
            {
                TimeEvents.InvokeYearOfGameChanged(PreviousYearOfGame, year);
                PreviousYearOfGame = year;
            }

            //NOTE THAT THIS MUST CHECK BEFORE SETTING IT TO TRUE BECAUSE OF SOME SILLY ISSUES
            if (FireLoadedGameEvent)
            {
                PlayerEvents.InvokeLoadedGame(new EventArgsLoadedGameChanged(hasLoadedGame));
                FireLoadedGameEvent = false;
            }

            if (hasLoadedGame != PreviouslyLoadedGame)
            {
                FireLoadedGameEvent = true;
                PreviouslyLoadedGame = hasLoadedGame;
            }

            if (mine != null && PreviousMineLevel != mine.mineLevel)
            {
                MineEvents.InvokeMineLevelChanged(PreviousMineLevel, mine.mineLevel);
                PreviousMineLevel = mine.mineLevel;
            }
        }

        private bool HasInventoryChanged(List<Item> items, out List<ItemStackChange> changedItems)
        {
            changedItems = new List<ItemStackChange>();
            IEnumerable<Item> actualItems = items.Where(n => n != null)?.ToArray();
            foreach (var item in actualItems)
            {
                if (PreviousItems != null && PreviousItems.ContainsKey(item))
                {
                    if (PreviousItems[item] != item.Stack)
                    {
                        changedItems.Add(new ItemStackChange {Item = item, StackChange = item.Stack - PreviousItems[item], ChangeType = ChangeType.StackChange});
                    }
                }
                else
                {
                    changedItems.Add(new ItemStackChange {Item = item, StackChange = item.Stack, ChangeType = ChangeType.Added});
                }
            }

            if (PreviousItems != null)
            {
                changedItems.AddRange(PreviousItems.Where(n => actualItems.All(i => i != n.Key)).Select(n =>
                    new ItemStackChange {Item = n.Key, StackChange = -n.Key.Stack, ChangeType = ChangeType.Removed}));
            }

            return changedItems.Any();
        }
    }
}