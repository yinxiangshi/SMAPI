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
    public class SGame : Game1
    {
        public static Dictionary<Int32, SObject> ModItems { get; private set; }
        public const Int32 LowestModItemID = 1000;

        public static FieldInfo[] StaticFields { get { return GetStaticFields(); } }

        public static FieldInfo[] GetStaticFields()
        {
            return typeof(Game1).GetFields();
        }

        public KeyboardState KStateNow { get; private set; }
        public KeyboardState KStatePrior { get; private set; }

        public MouseState MStateNow { get; private set; }
        public MouseState MStatePrior { get; private set; }

        public Keys[] CurrentlyPressedKeys => KStateNow.GetPressedKeys();
        public Keys[] PreviouslyPressedKeys => KStatePrior.GetPressedKeys();

        public Keys[] FramePressedKeys 
        { 
            get { return CurrentlyPressedKeys.Except(PreviouslyPressedKeys).ToArray(); }
        }
        public Keys[] FrameReleasedKeys
        {
            get { return PreviouslyPressedKeys.Except(CurrentlyPressedKeys).ToArray(); }
        }
        
        public Buttons[][] PreviouslyPressedButtons;

        private bool WasButtonJustPressed(Buttons button, ButtonState buttonState, PlayerIndex stateIndex)
        {
            return buttonState == ButtonState.Pressed && !PreviouslyPressedButtons[(int)stateIndex].Contains(button);
        }

        private bool WasButtonJustReleased(Buttons button, ButtonState buttonState, PlayerIndex stateIndex)
        {
            return buttonState == ButtonState.Released && PreviouslyPressedButtons[(int)stateIndex].Contains(button);
        }

        private bool WasButtonJustPressed(Buttons button, float value, PlayerIndex stateIndex)
        {
            return WasButtonJustPressed(button, value > 0.2f ? ButtonState.Pressed : ButtonState.Released, stateIndex);
        }

        private bool WasButtonJustReleased(Buttons button, float value, PlayerIndex stateIndex)
        {
            return WasButtonJustReleased(button, value > 0.2f ? ButtonState.Pressed : ButtonState.Released, stateIndex);
        }

        public bool PreviouslyLoadedGame { get; private set; }
        private bool FireLoadedGameEvent;

        public Buttons[] GetButtonsDown(PlayerIndex index)
        {
            GamePadState state = GamePad.GetState(index);
            List<Buttons> buttons = new List<Buttons>();
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

        public Buttons[] GetFramePressedButtons(PlayerIndex index)
        {     
            GamePadState state = GamePad.GetState(index);
            List<Buttons> buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (WasButtonJustPressed(Buttons.A, state.Buttons.A, index))                    buttons.Add(Buttons.A);
                if (WasButtonJustPressed(Buttons.B, state.Buttons.B, index))                    buttons.Add(Buttons.B);
                if (WasButtonJustPressed(Buttons.Back, state.Buttons.Back, index))              buttons.Add(Buttons.Back);
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

        public Buttons[] GetFrameReleasedButtons(PlayerIndex index)
        {
            GamePadState state = GamePad.GetState(index);
            List<Buttons> buttons = new List<Buttons>();
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

        public int PreviousGameLocations { get; private set; }
        public int PreviousLocationObjects { get; private set; }
        public Dictionary<Item, int> PreviousItems { get; private set; }

        public int PreviousCombatLevel { get; private set; }
        public int PreviousFarmingLevel { get; private set; }
        public int PreviousFishingLevel { get; private set; }
        public int PreviousForagingLevel { get; private set; }
        public int PreviousMiningLevel { get; private set; }
        public int PreviousLuckLevel { get; private set; }

        public GameLocation PreviousGameLocation { get; private set; }
        public IClickableMenu PreviousActiveMenu { get; private set; }

        public Int32 PreviousTimeOfDay { get; private set; }
        public Int32 PreviousDayOfMonth { get; private set; }
        public String PreviousSeasonOfYear { get; private set; }
        public Int32 PreviousYearOfGame { get; private set; }

        public Farmer PreviousFarmer { get; private set; }

        public Int32 CurrentUpdateTick { get; private set; }
        public bool FirstUpdate { get; private set; }

        public RenderTarget2D Screen
        {
            get { return typeof (Game1).GetBaseFieldValue<RenderTarget2D>(Program.gamePtr, "screen"); }
            set { typeof (Game1).SetBaseFieldValue<RenderTarget2D>(this, "screen", value); }
        }

        private static SGame instance;
        public static SGame Instance => instance;

        public static float FramesPerSecond { get; private set; }
        public static bool Debug { get; private set; }

        public Farmer CurrentFarmer => player;

        public SGame()
        {
            instance = this;
            FirstUpdate = true;
        }

        protected override void Initialize()
        {
            Log.Verbose("XNA Initialize");
            ModItems = new Dictionary<Int32, SObject>();
            PreviouslyPressedButtons = new Buttons[4][];
            for (int i = 0; i < 4; ++i) PreviouslyPressedButtons[i] = new Buttons[0];

            base.Initialize();
            GameEvents.InvokeInitialize();
        }

        protected override void LoadContent()
        {
            Log.Verbose("XNA LoadContent");
            base.LoadContent();
            GameEvents.InvokeLoadContent();
        }

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
                Log.Error("An error occured in the base update loop: " + ex);
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

            for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                PreviouslyPressedButtons[(int)i] = GetButtonsDown(i);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            FramesPerSecond = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

            try
            {
                base.Draw(gameTime);
            }
            catch (Exception ex)
            {
                Log.Error("An error occured in the base draw loop: " + ex);
                Console.ReadKey();
            }

            GraphicsEvents.InvokeDrawTick();

            if (Debug)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(dialogueFont, "FPS: " + FramesPerSecond, Vector2.Zero, Color.CornflowerBlue);
                spriteBatch.End();
            }
        }

        public static Int32 RegisterModItem(SObject modItem)
        {
            if (modItem.HasBeenRegistered)
            {
                Log.Error("The item {0} has already been registered with ID {1}", modItem.Name, modItem.RegisteredId);
                return modItem.RegisteredId;
            }
            Int32 newId = LowestModItemID;
            if (ModItems.Count > 0)
                newId = Math.Max(LowestModItemID, ModItems.OrderBy(x => x.Key).First().Key + 1);
            ModItems.Add(newId, modItem);
            modItem.HasBeenRegistered = true;
            modItem.RegisteredId = newId;
            return newId;
        }

        public static SObject PullModItemFromDict(Int32 id, bool isIndex)
        {
            if (isIndex)
            {
                if (ModItems.ElementAtOrDefault(id).Value != null)
                {
                    return ModItems.ElementAt(id).Value.Clone();
                }
                Log.Error("ModItem Dictionary does not contain index: " + id);
                return null;
            }
            if (ModItems.ContainsKey(id))
            {
                return ModItems[id].Clone();
            }
            Log.Error("ModItem Dictionary does not contain ID: " + id);
            return null;
        }
        
        public void UpdateEventCalls()
        {
            KStateNow = Keyboard.GetState();

            MStateNow = Mouse.GetState();

            foreach (Keys k in FramePressedKeys)
                ControlEvents.InvokeKeyPressed(k);

            foreach (Keys k in FrameReleasedKeys)
                ControlEvents.InvokeKeyReleased(k);

            for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                Buttons[] buttons = GetFramePressedButtons(i);
                foreach (Buttons b in buttons)
                {
                    if(b == Buttons.LeftTrigger || b == Buttons.RightTrigger)
                    {
                        ControlEvents.InvokeTriggerPressed(i, b, b == Buttons.LeftTrigger ? GamePad.GetState(i).Triggers.Left : GamePad.GetState(i).Triggers.Right);
                    }
                    else
                    {
                        ControlEvents.InvokeButtonPressed(i, b);
                    }
                }
            }

            for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                foreach (Buttons b in GetFrameReleasedButtons(i))
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
            if(objectHash != null && PreviousLocationObjects != objectHash)
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
        }

        private bool HasInventoryChanged(List<Item> items, out List<ItemStackChange> changedItems)
        {
            changedItems = new List<ItemStackChange>();
            IEnumerable<Item> actualItems = items.Where(n => n != null)?.ToArray();
            foreach (var item in actualItems)
            {
                if (PreviousItems != null && PreviousItems.ContainsKey(item))
                {
                    if(PreviousItems[item] != item.Stack)
                    {
                        changedItems.Add(new ItemStackChange { Item = item, StackChange = item.Stack - PreviousItems[item], ChangeType = ChangeType.StackChange });
                    }
                }
                else
                {
                    changedItems.Add(new ItemStackChange { Item = item, StackChange = item.Stack, ChangeType = ChangeType.Added });
                }
            }

            if (PreviousItems != null)
            {
                changedItems.AddRange(PreviousItems.Where(n => actualItems.All(i => i != n.Key)).Select(n =>
                    new ItemStackChange { Item = n.Key, StackChange = -n.Key.Stack, ChangeType = ChangeType.Removed }));
            }

            return (changedItems.Any());                        
        }
    }
}