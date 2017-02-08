using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SFarmer = StardewValley.Farmer;

namespace StardewModdingAPI.Framework
{
    /// <summary>SMAPI's extension of the game's core <see cref="Game1"/>, used to inject events.</summary>
    internal class SGame : Game1
    {
        /*********
        ** Properties
        *********/
        /// <summary>The number of ticks until SMAPI should notify mods when <see cref="Game1.hasLoadedGame"/> is set.</summary>
        /// <remarks>Skipping a few frames ensures the game finishes initialising the world before mods try to change it.</remarks>
        private int AfterLoadTimer = 5;

        /// <summary>Whether the player has loaded a save and the world has finished initialising.</summary>
        private bool IsWorldReady => this.AfterLoadTimer < 0;

        /// <summary>The debug messages to add to the next debug output.</summary>
        internal static Queue<string> DebugMessageQueue { get; private set; }

        /// <summary>Whether the game's zoom level is at 100% (i.e. nothing should be scaled).</summary>
        public bool ZoomLevelIsOne => Game1.options.zoomLevel.Equals(1.0f);

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Accessors
        *********/
        /// <summary>Arrays of pressed controller buttons indexed by <see cref="PlayerIndex"/>.</summary>
        public Buttons[][] PreviouslyPressedButtons;

        /// <summary>A record of the keyboard state (i.e. the up/down state for each button) as of the latest tick.</summary>
        public KeyboardState KStateNow { get; private set; }

        /// <summary>A record of the keyboard state (i.e. the up/down state for each button) as of the previous tick.</summary>
        public KeyboardState KStatePrior { get; private set; }

        /// <summary>A record of the mouse state (i.e. the cursor position, scroll amount, and the up/down state for each button) as of the latest tick.</summary>
        public MouseState MStateNow { get; private set; }

        /// <summary>A record of the mouse state (i.e. the cursor position, scroll amount, and the up/down state for each button) as of the previous tick.</summary>
        public MouseState MStatePrior { get; private set; }

        /// <summary>The current mouse position on the screen adjusted for the zoom level.</summary>
        public Point MPositionNow { get; private set; }

        /// <summary>The previous mouse position on the screen adjusted for the zoom level.</summary>
        public Point MPositionPrior { get; private set; }

        /// <summary>The keys that were pressed as of the latest tick.</summary>
        public Keys[] CurrentlyPressedKeys => this.KStateNow.GetPressedKeys();

        /// <summary>The keys that were pressed as of the previous tick.</summary>
        public Keys[] PreviouslyPressedKeys => this.KStatePrior.GetPressedKeys();

        /// <summary>The keys that just entered the down state.</summary>
        public Keys[] FramePressedKeys => this.CurrentlyPressedKeys.Except(this.PreviouslyPressedKeys).ToArray();

        /// <summary>The keys that just entered the up state.</summary>
        public Keys[] FrameReleasedKeys => this.PreviouslyPressedKeys.Except(this.CurrentlyPressedKeys).ToArray();

        /// <summary>Whether a save is currently loaded at last check.</summary>
        public bool PreviouslyLoadedGame { get; private set; }

        /// <summary>A hash of <see cref="Game1.locations"/> at last check.</summary>
        public int PreviousGameLocations { get; private set; }

        /// <summary>A hash of the current location's <see cref="GameLocation.objects"/> at last check.</summary>
        public int PreviousLocationObjects { get; private set; }

        /// <summary>The player's inventory at last check.</summary>
        public Dictionary<Item, int> PreviousItems { get; private set; }

        /// <summary>The player's combat skill level at last check.</summary>
        public int PreviousCombatLevel { get; private set; }

        /// <summary>The player's farming skill level at last check.</summary>
        public int PreviousFarmingLevel { get; private set; }

        /// <summary>The player's fishing skill level at last check.</summary>
        public int PreviousFishingLevel { get; private set; }

        /// <summary>The player's foraging skill level at last check.</summary>
        public int PreviousForagingLevel { get; private set; }

        /// <summary>The player's mining skill level at last check.</summary>
        public int PreviousMiningLevel { get; private set; }

        /// <summary>The player's luck skill level at last check.</summary>
        public int PreviousLuckLevel { get; private set; }

        /// <summary>The player's location at last check.</summary>
        public GameLocation PreviousGameLocation { get; private set; }

        /// <summary>The active game menu at last check.</summary>
        public IClickableMenu PreviousActiveMenu { get; private set; }

        /// <summary>The mine level at last check.</summary>
        public int PreviousMineLevel { get; private set; }

        /// <summary>The time of day (in 24-hour military format) at last check.</summary>
        public int PreviousTimeOfDay { get; private set; }

        /// <summary>The day of month (1–28) at last check.</summary>
        public int PreviousDayOfMonth { get; private set; }

        /// <summary>The season name (winter, spring, summer, or fall) at last check.</summary>
        public string PreviousSeasonOfYear { get; private set; }

        /// <summary>The year number at last check.</summary>
        public int PreviousYearOfGame { get; private set; }

        /// <summary>Whether the game was transitioning to a new day at last check.</summary>
        public bool PreviousIsNewDay { get; private set; }

        /// <summary>The player character at last check.</summary>
        public SFarmer PreviousFarmer { get; private set; }

        /// <summary>An index incremented on every tick and reset every 60th tick (0–59).</summary>
        public int CurrentUpdateTick { get; private set; }

        /// <summary>Whether this is the very first update tick since the game started.</summary>
        public bool FirstUpdate { get; private set; }

        /// <summary>The game's current render target.</summary>
        public RenderTarget2D Screen
        {
            get { return this.GetBaseFieldValue<RenderTarget2D>("screen"); }
            set { this.SetBaseFieldValue<RenderTarget2D>("screen", value); }
        }

        /// <summary>The game's current background color.</summary>
        public Color BgColour
        {
            get { return (Color)this.GetBaseFieldValue<object>("bgColor"); }
            set { this.SetBaseFieldValue<object>("bgColor", value); }
        }

        /// <summary>The current game instance.</summary>
        public static SGame Instance { get; private set; }

        /// <summary>The game's current frame rate, recalculated on each draw update.</summary>
        public static float FramesPerSecond { get; private set; }

        /// <summary>Whether we're in pseudo-debug mode, which shows information like FPS.</summary>
        public static bool Debug { get; private set; }

        /// <summary>The current player.</summary>
        [Obsolete("Use Game1.player instead")]
        public SFarmer CurrentFarmer => Game1.player;

        /// <summary>The game method which draws the farm buildings.</summary>
        public static MethodInfo DrawFarmBuildings = typeof(Game1).GetMethod("drawFarmBuildings", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>The game method which draws the game HUD.</summary>
        public static MethodInfo DrawHUD = typeof(Game1).GetMethod("drawHUD", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>The game method which draws the current dialogue box, if any.</summary>
        public static MethodInfo DrawDialogueBox = typeof(Game1).GetMethod("drawDialogueBox", BindingFlags.NonPublic | BindingFlags.Instance);


        /*********
        ** Public methods
        *********/
        /// <summary>Get the controller buttons which are currently pressed.</summary>
        /// <param name="index">The controller to check.</param>
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

        /// <summary>Get the controller buttons which were pressed after the last update.</summary>
        /// <param name="index">The controller to check.</param>
        public Buttons[] GetFramePressedButtons(PlayerIndex index)
        {
            var state = GamePad.GetState(index);
            var buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (this.WasButtonJustPressed(Buttons.A, state.Buttons.A, index)) buttons.Add(Buttons.A);
                if (this.WasButtonJustPressed(Buttons.B, state.Buttons.B, index)) buttons.Add(Buttons.B);
                if (this.WasButtonJustPressed(Buttons.Back, state.Buttons.Back, index)) buttons.Add(Buttons.Back);
                if (this.WasButtonJustPressed(Buttons.BigButton, state.Buttons.BigButton, index)) buttons.Add(Buttons.BigButton);
                if (this.WasButtonJustPressed(Buttons.LeftShoulder, state.Buttons.LeftShoulder, index)) buttons.Add(Buttons.LeftShoulder);
                if (this.WasButtonJustPressed(Buttons.LeftStick, state.Buttons.LeftStick, index)) buttons.Add(Buttons.LeftStick);
                if (this.WasButtonJustPressed(Buttons.RightShoulder, state.Buttons.RightShoulder, index)) buttons.Add(Buttons.RightShoulder);
                if (this.WasButtonJustPressed(Buttons.RightStick, state.Buttons.RightStick, index)) buttons.Add(Buttons.RightStick);
                if (this.WasButtonJustPressed(Buttons.Start, state.Buttons.Start, index)) buttons.Add(Buttons.Start);
                if (this.WasButtonJustPressed(Buttons.X, state.Buttons.X, index)) buttons.Add(Buttons.X);
                if (this.WasButtonJustPressed(Buttons.Y, state.Buttons.Y, index)) buttons.Add(Buttons.Y);
                if (this.WasButtonJustPressed(Buttons.DPadUp, state.DPad.Up, index)) buttons.Add(Buttons.DPadUp);
                if (this.WasButtonJustPressed(Buttons.DPadDown, state.DPad.Down, index)) buttons.Add(Buttons.DPadDown);
                if (this.WasButtonJustPressed(Buttons.DPadLeft, state.DPad.Left, index)) buttons.Add(Buttons.DPadLeft);
                if (this.WasButtonJustPressed(Buttons.DPadRight, state.DPad.Right, index)) buttons.Add(Buttons.DPadRight);
                if (this.WasButtonJustPressed(Buttons.LeftTrigger, state.Triggers.Left, index)) buttons.Add(Buttons.LeftTrigger);
                if (this.WasButtonJustPressed(Buttons.RightTrigger, state.Triggers.Right, index)) buttons.Add(Buttons.RightTrigger);
            }
            return buttons.ToArray();
        }

        /// <summary>Get the controller buttons which were released after the last update.</summary>
        /// <param name="index">The controller to check.</param>
        public Buttons[] GetFrameReleasedButtons(PlayerIndex index)
        {
            var state = GamePad.GetState(index);
            var buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (this.WasButtonJustReleased(Buttons.A, state.Buttons.A, index)) buttons.Add(Buttons.A);
                if (this.WasButtonJustReleased(Buttons.B, state.Buttons.B, index)) buttons.Add(Buttons.B);
                if (this.WasButtonJustReleased(Buttons.Back, state.Buttons.Back, index)) buttons.Add(Buttons.Back);
                if (this.WasButtonJustReleased(Buttons.BigButton, state.Buttons.BigButton, index)) buttons.Add(Buttons.BigButton);
                if (this.WasButtonJustReleased(Buttons.LeftShoulder, state.Buttons.LeftShoulder, index)) buttons.Add(Buttons.LeftShoulder);
                if (this.WasButtonJustReleased(Buttons.LeftStick, state.Buttons.LeftStick, index)) buttons.Add(Buttons.LeftStick);
                if (this.WasButtonJustReleased(Buttons.RightShoulder, state.Buttons.RightShoulder, index)) buttons.Add(Buttons.RightShoulder);
                if (this.WasButtonJustReleased(Buttons.RightStick, state.Buttons.RightStick, index)) buttons.Add(Buttons.RightStick);
                if (this.WasButtonJustReleased(Buttons.Start, state.Buttons.Start, index)) buttons.Add(Buttons.Start);
                if (this.WasButtonJustReleased(Buttons.X, state.Buttons.X, index)) buttons.Add(Buttons.X);
                if (this.WasButtonJustReleased(Buttons.Y, state.Buttons.Y, index)) buttons.Add(Buttons.Y);
                if (this.WasButtonJustReleased(Buttons.DPadUp, state.DPad.Up, index)) buttons.Add(Buttons.DPadUp);
                if (this.WasButtonJustReleased(Buttons.DPadDown, state.DPad.Down, index)) buttons.Add(Buttons.DPadDown);
                if (this.WasButtonJustReleased(Buttons.DPadLeft, state.DPad.Left, index)) buttons.Add(Buttons.DPadLeft);
                if (this.WasButtonJustReleased(Buttons.DPadRight, state.DPad.Right, index)) buttons.Add(Buttons.DPadRight);
                if (this.WasButtonJustReleased(Buttons.LeftTrigger, state.Triggers.Left, index)) buttons.Add(Buttons.LeftTrigger);
                if (this.WasButtonJustReleased(Buttons.RightTrigger, state.Triggers.Right, index)) buttons.Add(Buttons.RightTrigger);
            }
            return buttons.ToArray();
        }

        /// <summary>Queue a message to be added to the debug output.</summary>
        /// <param name="message">The message to add.</param>
        /// <returns>Returns whether the message was successfully queued.</returns>
        public static bool QueueDebugMessage(string message)
        {
            if (!SGame.Debug)
                return false;
            if (SGame.DebugMessageQueue.Count > 32)
                return false;

            SGame.DebugMessageQueue.Enqueue(message);
            return true;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal SGame(IMonitor monitor)
        {
            this.Monitor = monitor;
            this.FirstUpdate = true;
            SGame.Instance = this;
        }

        /// <summary>The method called during game launch after configuring XNA or MonoGame. The game window hasn't been opened by this point.</summary>
        protected override void Initialize()
        {
            SGame.DebugMessageQueue = new Queue<string>();
            this.PreviouslyPressedButtons = new Buttons[4][];
            for (var i = 0; i < 4; ++i)
                this.PreviouslyPressedButtons[i] = new Buttons[0];

            base.Initialize();
            GameEvents.InvokeInitialize(this.Monitor);
        }

        /// <summary>The method called before XNA or MonoGame loads or reloads graphics resources.</summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            GameEvents.InvokeLoadContent(this.Monitor);
        }

        /// <summary>The method called when the game is updating its state. This happens roughly 60 times per second.</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        protected override void Update(GameTime gameTime)
        {
            // add FPS to debug output
            SGame.QueueDebugMessage($"FPS: {SGame.FramesPerSecond}");

            // raise game loaded
            if (this.FirstUpdate)
                GameEvents.InvokeGameLoaded(this.Monitor);

            // update SMAPI events
            this.UpdateEventCalls();

            // toggle debug output
            if (this.FramePressedKeys.Contains(Keys.F3))
                SGame.Debug = !SGame.Debug;

            // let game update
            try
            {
                base.Update(gameTime);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"An error occured in the base update loop: {ex.GetLogSummary()}", LogLevel.Error);
                Console.ReadKey();
            }

            // raise update events
            GameEvents.InvokeUpdateTick(this.Monitor);
            if (this.FirstUpdate)
            {
                GameEvents.InvokeFirstUpdateTick(this.Monitor);
                this.FirstUpdate = false;
            }
            if (this.CurrentUpdateTick % 2 == 0)
                GameEvents.InvokeSecondUpdateTick(this.Monitor);
            if (this.CurrentUpdateTick % 4 == 0)
                GameEvents.InvokeFourthUpdateTick(this.Monitor);
            if (this.CurrentUpdateTick % 8 == 0)
                GameEvents.InvokeEighthUpdateTick(this.Monitor);
            if (this.CurrentUpdateTick % 15 == 0)
                GameEvents.InvokeQuarterSecondTick(this.Monitor);
            if (this.CurrentUpdateTick % 30 == 0)
                GameEvents.InvokeHalfSecondTick(this.Monitor);
            if (this.CurrentUpdateTick % 60 == 0)
                GameEvents.InvokeOneSecondTick(this.Monitor);
            this.CurrentUpdateTick += 1;
            if (this.CurrentUpdateTick >= 60)
                this.CurrentUpdateTick = 0;

            // track keyboard state
            if (this.KStatePrior != this.KStateNow)
                this.KStatePrior = this.KStateNow;

            // track controller button state
            for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
                this.PreviouslyPressedButtons[(int)i] = this.GetButtonsDown(i);
        }

        /// <summary>The method called to draw everything to the screen.</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        /// <remarks>This implementation is identical to <see cref="Game1.Draw"/>, except for try..catch around menu draw code, minor formatting, and added events.</remarks>
        protected override void Draw(GameTime gameTime)
        {
            // track frame rate
            SGame.FramesPerSecond = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

            try
            {
                if (!this.ZoomLevelIsOne)
                    this.GraphicsDevice.SetRenderTarget(this.Screen);

                this.GraphicsDevice.Clear(this.BgColour);
                if (Game1.options.showMenuBackground && Game1.activeClickableMenu != null && Game1.activeClickableMenu.showWithoutTransparencyIfOptionIsSet())
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    try
                    {
                        Game1.activeClickableMenu.drawBackground(Game1.spriteBatch);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing its background. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                    GraphicsEvents.InvokeOnPreRenderGuiEvent(this.Monitor);
                    try
                    {
                        Game1.activeClickableMenu.draw(Game1.spriteBatch);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                    GraphicsEvents.InvokeOnPostRenderGuiEvent(this.Monitor);
                    Game1.spriteBatch.End();
                    if (!this.ZoomLevelIsOne)
                    {
                        this.GraphicsDevice.SetRenderTarget(null);
                        this.GraphicsDevice.Clear(this.BgColour);
                        Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        Game1.spriteBatch.Draw(this.Screen, Vector2.Zero, this.Screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                        Game1.spriteBatch.End();
                    }
                    return;
                }
                if (Game1.gameMode == 11)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Game1.spriteBatch.DrawString(Game1.smoothFont, "Stardew Valley has crashed...", new Vector2(16f, 16f), Color.HotPink);
                    Game1.spriteBatch.DrawString(Game1.smoothFont, "Please send the error report or a screenshot of this message to @ConcernedApe. (http://stardewvalley.net/contact/)", new Vector2(16f, 32f), new Color(0, 255, 0));
                    Game1.spriteBatch.DrawString(Game1.smoothFont, Game1.parseText(Game1.errorMessage, Game1.smoothFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
                    Game1.spriteBatch.End();
                    return;
                }
                if (Game1.currentMinigame != null)
                {
                    Game1.currentMinigame.draw(Game1.spriteBatch);
                    if (Game1.globalFade && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause))
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
                        Game1.spriteBatch.End();
                    }
                    if (!this.ZoomLevelIsOne)
                    {
                        this.GraphicsDevice.SetRenderTarget(null);
                        this.GraphicsDevice.Clear(this.BgColour);
                        Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        Game1.spriteBatch.Draw(this.Screen, Vector2.Zero, this.Screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                        Game1.spriteBatch.End();
                    }
                    return;
                }
                if (Game1.showingEndOfNightStuff)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    try
                    {
                        Game1.activeClickableMenu?.draw(Game1.spriteBatch);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                    Game1.spriteBatch.End();
                    if (!this.ZoomLevelIsOne)
                    {
                        this.GraphicsDevice.SetRenderTarget(null);
                        this.GraphicsDevice.Clear(this.BgColour);
                        Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        Game1.spriteBatch.Draw(this.Screen, Vector2.Zero, this.Screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                        Game1.spriteBatch.End();
                    }
                    return;
                }
                if (Game1.gameMode == 6)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    string text = "";
                    int num = 0;
                    while (num < gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0)
                    {
                        text += ".";
                        num++;
                    }
                    SpriteText.drawString(Game1.spriteBatch, "Loading" + text, 64, Game1.graphics.GraphicsDevice.Viewport.Height - 64, 999, -1, 999, 1f, 1f, false, 0, "Loading...");
                    Game1.spriteBatch.End();
                    if (!this.ZoomLevelIsOne)
                    {
                        this.GraphicsDevice.SetRenderTarget(null);
                        this.GraphicsDevice.Clear(this.BgColour);
                        Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        Game1.spriteBatch.Draw(this.Screen, Vector2.Zero, this.Screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                        Game1.spriteBatch.End();
                    }
                    return;
                }
                if (Game1.gameMode == 0)
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                else
                {
                    if (Game1.drawLighting)
                    {
                        this.GraphicsDevice.SetRenderTarget(Game1.lightmap);
                        this.GraphicsDevice.Clear(Color.White * 0f);
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                        Game1.spriteBatch.Draw(Game1.staminaRect, Game1.lightmap.Bounds, Game1.currentLocation.name.Equals("UndergroundMine") ? Game1.mine.getLightingColor(gameTime) : ((!Game1.ambientLight.Equals(Color.White) && (!Game1.isRaining || !Game1.currentLocation.isOutdoors)) ? Game1.ambientLight : Game1.outdoorLight));
                        for (int i = 0; i < Game1.currentLightSources.Count; i++)
                        {
                            if (Utility.isOnScreen(Game1.currentLightSources.ElementAt(i).position, (int)(Game1.currentLightSources.ElementAt(i).radius * Game1.tileSize * 4f)))
                                Game1.spriteBatch.Draw(Game1.currentLightSources.ElementAt(i).lightTexture, Game1.GlobalToLocal(Game1.viewport, Game1.currentLightSources.ElementAt(i).position) / Game1.options.lightingQuality, Game1.currentLightSources.ElementAt(i).lightTexture.Bounds, Game1.currentLightSources.ElementAt(i).color, 0f, new Vector2(Game1.currentLightSources.ElementAt(i).lightTexture.Bounds.Center.X, Game1.currentLightSources.ElementAt(i).lightTexture.Bounds.Center.Y), Game1.currentLightSources.ElementAt(i).radius / Game1.options.lightingQuality, SpriteEffects.None, 0.9f);
                        }
                        Game1.spriteBatch.End();
                        this.GraphicsDevice.SetRenderTarget(this.ZoomLevelIsOne ? null : this.Screen);
                    }
                    if (Game1.bloomDay)
                        Game1.bloom?.BeginDraw();
                    this.GraphicsDevice.Clear(this.BgColour);
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    GraphicsEvents.InvokeOnPreRenderEvent(this.Monitor);
                    Game1.background?.draw(Game1.spriteBatch);
                    Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                    Game1.currentLocation.Map.GetLayer("Back").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, Game1.pixelZoom);
                    Game1.currentLocation.drawWater(Game1.spriteBatch);
                    if (Game1.CurrentEvent == null)
                    {
                        using (List<NPC>.Enumerator enumerator = Game1.currentLocation.characters.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                NPC current = enumerator.Current;
                                if (current != null && !current.swimming && !current.hideShadow && !current.IsMonster && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(current.getTileLocation()))
                                    Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, current.position + new Vector2(current.sprite.spriteWidth * Game1.pixelZoom / 2f, current.GetBoundingBox().Height + (current.IsMonster ? 0 : (Game1.pixelZoom * 3)))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (Game1.pixelZoom + current.yJumpOffset / 40f) * current.scale, SpriteEffects.None, Math.Max(0f, current.getStandingY() / 10000f) - 1E-06f);
                            }
                            goto IL_B30;
                        }
                    }
                    foreach (NPC current2 in Game1.CurrentEvent.actors)
                    {
                        if (!current2.swimming && !current2.hideShadow && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(current2.getTileLocation()))
                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, current2.position + new Vector2(current2.sprite.spriteWidth * Game1.pixelZoom / 2f, current2.GetBoundingBox().Height + (current2.IsMonster ? 0 : (Game1.pixelZoom * 3)))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (Game1.pixelZoom + current2.yJumpOffset / 40f) * current2.scale, SpriteEffects.None, Math.Max(0f, current2.getStandingY() / 10000f) - 1E-06f);
                    }
                    IL_B30:
                    if (!Game1.player.swimming && !Game1.player.isRidingHorse() && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(Game1.player.getTileLocation()))
                        Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.player.position + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f - (((Game1.player.running || Game1.player.usingTool) && Game1.player.FarmerSprite.indexInCurrentAnimation > 1) ? (Math.Abs(FarmerRenderer.featureYOffsetPerFrame[Game1.player.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, 0f);
                    Game1.currentLocation.Map.GetLayer("Buildings").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, Game1.pixelZoom);
                    Game1.mapDisplayDevice.EndScene();
                    Game1.spriteBatch.End();
                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    if (Game1.CurrentEvent == null)
                    {
                        using (List<NPC>.Enumerator enumerator3 = Game1.currentLocation.characters.GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                NPC current3 = enumerator3.Current;
                                if (current3 != null && !current3.swimming && !current3.hideShadow && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(current3.getTileLocation()))
                                    Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, current3.position + new Vector2(current3.sprite.spriteWidth * Game1.pixelZoom / 2f, current3.GetBoundingBox().Height + (current3.IsMonster ? 0 : (Game1.pixelZoom * 3)))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (Game1.pixelZoom + current3.yJumpOffset / 40f) * current3.scale, SpriteEffects.None, Math.Max(0f, current3.getStandingY() / 10000f) - 1E-06f);
                            }
                            goto IL_F5F;
                        }
                    }
                    foreach (NPC current4 in Game1.CurrentEvent.actors)
                    {
                        if (!current4.swimming && !current4.hideShadow && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(current4.getTileLocation()))
                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, current4.position + new Vector2(current4.sprite.spriteWidth * Game1.pixelZoom / 2f, current4.GetBoundingBox().Height + (current4.IsMonster ? 0 : (Game1.pixelZoom * 3)))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (Game1.pixelZoom + current4.yJumpOffset / 40f) * current4.scale, SpriteEffects.None, Math.Max(0f, current4.getStandingY() / 10000f) - 1E-06f);
                    }
                    IL_F5F:
                    if (!Game1.player.swimming && !Game1.player.isRidingHorse() && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(Game1.player.getTileLocation()))
                        Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.player.position + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f - (((Game1.player.running || Game1.player.usingTool) && Game1.player.FarmerSprite.indexInCurrentAnimation > 1) ? (Math.Abs(FarmerRenderer.featureYOffsetPerFrame[Game1.player.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, Math.Max(0.0001f, Game1.player.getStandingY() / 10000f + 0.00011f) - 0.0001f);
                    if (Game1.displayFarmer)
                        Game1.player.draw(Game1.spriteBatch);
                    if ((Game1.eventUp || Game1.killScreen) && !Game1.killScreen)
                        Game1.currentLocation.currentEvent?.draw(Game1.spriteBatch);
                    if (Game1.player.currentUpgrade != null && Game1.player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && Game1.currentLocation.Name.Equals("Farm"))
                        Game1.spriteBatch.Draw(Game1.player.currentUpgrade.workerTexture, Game1.GlobalToLocal(Game1.viewport, Game1.player.currentUpgrade.positionOfCarpenter), Game1.player.currentUpgrade.getSourceRectangle(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (Game1.player.currentUpgrade.positionOfCarpenter.Y + Game1.tileSize * 3 / 4) / 10000f);
                    Game1.currentLocation.draw(Game1.spriteBatch);
                    if (Game1.eventUp && Game1.currentLocation.currentEvent?.messageToScreen != null)
                        Game1.drawWithBorder(Game1.currentLocation.currentEvent.messageToScreen, Color.Black, Color.White, new Vector2(Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Width / 2 - Game1.borderFont.MeasureString(Game1.currentLocation.currentEvent.messageToScreen).X / 2f, Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Height - Game1.tileSize), 0f, 1f, 0.999f);
                    if (Game1.player.ActiveObject == null && (Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && (!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool))
                        Game1.drawTool(Game1.player);
                    if (Game1.currentLocation.Name.Equals("Farm"))
                        SGame.DrawFarmBuildings.Invoke(Program.gamePtr, null);
                    if (Game1.tvStation >= 0)
                        Game1.spriteBatch.Draw(Game1.tvStationTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(6 * Game1.tileSize + Game1.tileSize / 4, 2 * Game1.tileSize + Game1.tileSize / 2)), new Rectangle(Game1.tvStation * 24, 0, 24, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
                    if (Game1.panMode)
                    {
                        Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle((int)Math.Floor((Game1.getOldMouseX() + Game1.viewport.X) / (double)Game1.tileSize) * Game1.tileSize - Game1.viewport.X, (int)Math.Floor((Game1.getOldMouseY() + Game1.viewport.Y) / (double)Game1.tileSize) * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize), Color.Lime * 0.75f);
                        foreach (Warp current5 in Game1.currentLocation.warps)
                            Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(current5.X * Game1.tileSize - Game1.viewport.X, current5.Y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize), Color.Red * 0.75f);
                    }
                    Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                    Game1.currentLocation.Map.GetLayer("Front").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, Game1.pixelZoom);
                    Game1.mapDisplayDevice.EndScene();
                    Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
                    Game1.spriteBatch.End();
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    if (Game1.currentLocation.Name.Equals("Farm") && Game1.stats.SeedsSown >= 200u)
                    {
                        Game1.spriteBatch.Draw(Game1.debrisSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(3 * Game1.tileSize + Game1.tileSize / 4, Game1.tileSize + Game1.tileSize / 3)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 16), Color.White);
                        Game1.spriteBatch.Draw(Game1.debrisSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(4 * Game1.tileSize + Game1.tileSize, 2 * Game1.tileSize + Game1.tileSize)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 16), Color.White);
                        Game1.spriteBatch.Draw(Game1.debrisSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(5 * Game1.tileSize, 2 * Game1.tileSize)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 16), Color.White);
                        Game1.spriteBatch.Draw(Game1.debrisSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(3 * Game1.tileSize + Game1.tileSize / 2, 3 * Game1.tileSize)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 16), Color.White);
                        Game1.spriteBatch.Draw(Game1.debrisSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(5 * Game1.tileSize - Game1.tileSize / 4, Game1.tileSize)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 16), Color.White);
                        Game1.spriteBatch.Draw(Game1.debrisSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(4 * Game1.tileSize, 3 * Game1.tileSize + Game1.tileSize / 6)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 16), Color.White);
                        Game1.spriteBatch.Draw(Game1.debrisSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(4 * Game1.tileSize + Game1.tileSize / 5, 2 * Game1.tileSize + Game1.tileSize / 3)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 16), Color.White);
                    }
                    if (Game1.displayFarmer && Game1.player.ActiveObject != null && Game1.player.ActiveObject.bigCraftable && this.checkBigCraftableBoundariesForFrontLayer() && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)
                        Game1.drawPlayerHeldObject(Game1.player);
                    else if (Game1.displayFarmer && Game1.player.ActiveObject != null && ((Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location((int)Game1.player.position.X, (int)Game1.player.position.Y - Game1.tileSize * 3 / 5), Game1.viewport.Size) != null && !Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location((int)Game1.player.position.X, (int)Game1.player.position.Y - Game1.tileSize * 3 / 5), Game1.viewport.Size).TileIndexProperties.ContainsKey("FrontAlways")) || (Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.GetBoundingBox().Right, (int)Game1.player.position.Y - Game1.tileSize * 3 / 5), Game1.viewport.Size) != null && !Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.GetBoundingBox().Right, (int)Game1.player.position.Y - Game1.tileSize * 3 / 5), Game1.viewport.Size).TileIndexProperties.ContainsKey("FrontAlways"))))
                        Game1.drawPlayerHeldObject(Game1.player);
                    if ((Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && (!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool) && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), (int)Game1.player.position.Y - Game1.tileSize * 3 / 5), Game1.viewport.Size) != null && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)
                        Game1.drawTool(Game1.player);
                    if (Game1.currentLocation.Map.GetLayer("AlwaysFront") != null)
                    {
                        Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                        Game1.currentLocation.Map.GetLayer("AlwaysFront").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, Game1.pixelZoom);
                        Game1.mapDisplayDevice.EndScene();
                    }
                    if (Game1.toolHold > 400f && Game1.player.CurrentTool.UpgradeLevel >= 1 && Game1.player.canReleaseTool)
                    {
                        Color color = Color.White;
                        switch ((int)(Game1.toolHold / 600f) + 2)
                        {
                            case 1:
                                color = Tool.copperColor;
                                break;
                            case 2:
                                color = Tool.steelColor;
                                break;
                            case 3:
                                color = Tool.goldColor;
                                break;
                            case 4:
                                color = Tool.iridiumColor;
                                break;
                        }
                        Game1.spriteBatch.Draw(Game1.littleEffect, new Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - 2, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.player.CurrentTool.Name.Equals("Watering Can") ? 0 : Game1.tileSize) - 2, (int)(Game1.toolHold % 600f * 0.08f) + 4, Game1.tileSize / 8 + 4), Color.Black);
                        Game1.spriteBatch.Draw(Game1.littleEffect, new Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.player.CurrentTool.Name.Equals("Watering Can") ? 0 : Game1.tileSize), (int)(Game1.toolHold % 600f * 0.08f), Game1.tileSize / 8), color);
                    }
                    if (Game1.isDebrisWeather && Game1.currentLocation.IsOutdoors && !Game1.currentLocation.ignoreDebrisWeather && !Game1.currentLocation.Name.Equals("Desert") && Game1.viewport.X > -10)
                    {
                        foreach (WeatherDebris current6 in Game1.debrisWeather)
                            current6.draw(Game1.spriteBatch);
                    }
                    Game1.farmEvent?.draw(Game1.spriteBatch);
                    if (Game1.currentLocation.LightLevel > 0f && Game1.timeOfDay < 2000)
                        Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * Game1.currentLocation.LightLevel);
                    if (Game1.screenGlow)
                        Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
                    Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);
                    if (Game1.player.CurrentTool is FishingRod && ((Game1.player.CurrentTool as FishingRod).isTimingCast || (Game1.player.CurrentTool as FishingRod).castingChosenCountdown > 0f || (Game1.player.CurrentTool as FishingRod).fishCaught || (Game1.player.CurrentTool as FishingRod).showingTreasure))
                        Game1.player.CurrentTool.draw(Game1.spriteBatch);
                    if (Game1.isRaining && Game1.currentLocation.IsOutdoors && !Game1.currentLocation.Name.Equals("Desert") && !(Game1.currentLocation is Summit) && (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2(Game1.viewport.X / Game1.tileSize, Game1.viewport.Y / Game1.tileSize))))
                    {
                        for (int j = 0; j < Game1.rainDrops.Length; j++)
                            Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[j].position, Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[j].frame), Color.White);
                    }

                    Game1.spriteBatch.End();

                    //base.Draw(gameTime);

                    Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                    {
                        foreach (NPC current7 in Game1.currentLocation.currentEvent.actors)
                        {
                            if (current7.isEmoting)
                            {
                                Vector2 localPosition = current7.getLocalPosition(Game1.viewport);
                                localPosition.Y -= Game1.tileSize * 2 + Game1.pixelZoom * 3;
                                if (current7.age == 2)
                                    localPosition.Y += Game1.tileSize / 2;
                                else if (current7.gender == 1)
                                    localPosition.Y += Game1.tileSize / 6;
                                Game1.spriteBatch.Draw(Game1.emoteSpriteSheet, localPosition, new Rectangle(current7.CurrentEmoteIndex * (Game1.tileSize / 4) % Game1.emoteSpriteSheet.Width, current7.CurrentEmoteIndex * (Game1.tileSize / 4) / Game1.emoteSpriteSheet.Width * (Game1.tileSize / 4), Game1.tileSize / 4, Game1.tileSize / 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, current7.getStandingY() / 10000f);
                            }
                        }
                    }
                    Game1.spriteBatch.End();
                    if (Game1.drawLighting)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, new BlendState
                        {
                            ColorBlendFunction = BlendFunction.ReverseSubtract,
                            ColorDestinationBlend = Blend.One,
                            ColorSourceBlend = Blend.SourceColor
                        }, SamplerState.LinearClamp, null, null);
                        Game1.spriteBatch.Draw(Game1.lightmap, Vector2.Zero, Game1.lightmap.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.lightingQuality, SpriteEffects.None, 1f);
                        if (Game1.isRaining && Game1.currentLocation.isOutdoors && !(Game1.currentLocation is Desert))
                        {
                            Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.OrangeRed * 0.45f);
                        }
                        Game1.spriteBatch.End();
                    }
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    if (Game1.drawGrid)
                    {
                        int num2 = -Game1.viewport.X % Game1.tileSize;
                        float num3 = -(float)Game1.viewport.Y % Game1.tileSize;
                        for (int k = num2; k < Game1.graphics.GraphicsDevice.Viewport.Width; k += Game1.tileSize)
                            Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(k, (int)num3, 1, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
                        for (float num4 = num3; num4 < (float)Game1.graphics.GraphicsDevice.Viewport.Height; num4 += (float)Game1.tileSize)
                            Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(num2, (int)num4, Game1.graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
                    }
                    if (Game1.currentBillboard != 0)
                        this.drawBillboard();

                    GraphicsEvents.InvokeOnPreRenderHudEventNoCheck(this.Monitor);
                    if ((Game1.displayHUD || Game1.eventUp) && Game1.currentBillboard == 0 && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode)
                    {
                        GraphicsEvents.InvokeOnPreRenderHudEvent(this.Monitor);
                        SGame.DrawHUD.Invoke(Program.gamePtr, null);
                        GraphicsEvents.InvokeOnPostRenderHudEvent(this.Monitor);
                    }
                    else if (Game1.activeClickableMenu == null && Game1.farmEvent == null)
                        Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
                    GraphicsEvents.InvokeOnPostRenderHudEventNoCheck(this.Monitor);

                    if (Game1.hudMessages.Any() && (!Game1.eventUp || Game1.isFestival()))
                    {
                        for (int l = Game1.hudMessages.Count - 1; l >= 0; l--)
                            Game1.hudMessages[l].draw(Game1.spriteBatch, l);
                    }
                }
                Game1.farmEvent?.draw(Game1.spriteBatch);
                if (Game1.dialogueUp && !Game1.nameSelectUp && !Game1.messagePause && !(Game1.activeClickableMenu is DialogueBox))
                    SGame.DrawDialogueBox.Invoke(Program.gamePtr, null);
                if (Game1.progressBar)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle((Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Width - Game1.dialogueWidth) / 2, Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom - Game1.tileSize * 2, Game1.dialogueWidth, Game1.tileSize / 2), Color.LightGray);
                    Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle((Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Width - Game1.dialogueWidth) / 2, Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom - Game1.tileSize * 2, (int)(Game1.pauseAccumulator / Game1.pauseTime * Game1.dialogueWidth), Game1.tileSize / 2), Color.DimGray);
                }
                if (Game1.eventUp)
                    Game1.currentLocation.currentEvent?.drawAfterMap(Game1.spriteBatch);
                if (Game1.isRaining && Game1.currentLocation.isOutdoors && !(Game1.currentLocation is Desert))
                    Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Blue * 0.2f);
                if ((Game1.fadeToBlack || Game1.globalFade) && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause))
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
                else if (Game1.flashAlpha > 0f)
                {
                    if (Game1.options.screenFlash)
                        Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, Game1.flashAlpha));
                    Game1.flashAlpha -= 0.1f;
                }
                if ((Game1.messagePause || Game1.globalFade) && Game1.dialogueUp)
                    SGame.DrawDialogueBox.Invoke(Program.gamePtr, null);
                foreach (TemporaryAnimatedSprite current8 in Game1.screenOverlayTempSprites)
                    current8.draw(Game1.spriteBatch, true);
                if (Game1.debugMode)
                {
                    Game1.spriteBatch.DrawString(Game1.smallFont, string.Concat(new object[]
                    {
                            Game1.panMode ? ((Game1.getOldMouseX() + Game1.viewport.X) / Game1.tileSize + "," + (Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize) : string.Concat("aplayer: ", Game1.player.getStandingX() / Game1.tileSize, ", ", Game1.player.getStandingY() / Game1.tileSize),
                            Environment.NewLine,
                            "debugOutput: ",
                            Game1.debugOutput
                    }), new Vector2(this.GraphicsDevice.Viewport.TitleSafeArea.X, this.GraphicsDevice.Viewport.TitleSafeArea.Y), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
                }
                /*if (inputMode)
                {
                    spriteBatch.DrawString(smallFont, "Input: " + debugInput, new Vector2(tileSize, tileSize * 3), Color.Purple);
                }*/
                if (Game1.showKeyHelp)
                    Game1.spriteBatch.DrawString(Game1.smallFont, Game1.keyHelpString, new Vector2(Game1.tileSize, Game1.viewport.Height - Game1.tileSize - (Game1.dialogueUp ? (Game1.tileSize * 3 + (Game1.isQuestion ? (Game1.questionChoices.Count * Game1.tileSize) : 0)) : 0) - Game1.smallFont.MeasureString(Game1.keyHelpString).Y), Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);

                GraphicsEvents.InvokeOnPreRenderGuiEventNoCheck(this.Monitor);
                if (Game1.activeClickableMenu != null)
                {
                    GraphicsEvents.InvokeOnPreRenderGuiEvent(this.Monitor);
                    try
                    {
                        Game1.activeClickableMenu.draw(Game1.spriteBatch);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                    GraphicsEvents.InvokeOnPostRenderGuiEvent(this.Monitor);
                }
                else
                    Game1.farmEvent?.drawAboveEverything(Game1.spriteBatch);
                GraphicsEvents.InvokeOnPostRenderGuiEventNoCheck(this.Monitor);

                GraphicsEvents.InvokeOnPostRenderEvent(this.Monitor);
                Game1.spriteBatch.End();

                GraphicsEvents.InvokeDrawInRenderTargetTick(this.Monitor);

                if (!this.ZoomLevelIsOne)
                {
                    this.GraphicsDevice.SetRenderTarget(null);
                    this.GraphicsDevice.Clear(this.BgColour);
                    Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    Game1.spriteBatch.Draw(this.Screen, Vector2.Zero, this.Screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                    Game1.spriteBatch.End();
                }

                GraphicsEvents.InvokeDrawTick(this.Monitor);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"An error occured in the overridden draw loop: {ex.GetLogSummary()}", LogLevel.Error);
            }

            if (SGame.Debug)
            {
                Game1.spriteBatch.Begin();

                int i = 0;
                while (SGame.DebugMessageQueue.Any())
                {
                    string message = SGame.DebugMessageQueue.Dequeue();
                    Game1.spriteBatch.DrawString(Game1.smoothFont, message, new Vector2(0, i * 14), Color.CornflowerBlue);
                    i++;
                }
                GraphicsEvents.InvokeDrawDebug(this.Monitor);

                Game1.spriteBatch.End();
            }
            else
                SGame.DebugMessageQueue.Clear();
        }

        /// <summary>Get whether a controller button was pressed since the last check.</summary>
        /// <param name="button">The controller button to check.</param>
        /// <param name="buttonState">The last known state.</param>
        /// <param name="stateIndex">The player whose controller to check.</param>
        private bool WasButtonJustPressed(Buttons button, ButtonState buttonState, PlayerIndex stateIndex)
        {
            return buttonState == ButtonState.Pressed && !this.PreviouslyPressedButtons[(int)stateIndex].Contains(button);
        }

        /// <summary>Get whether a controller button was released since the last check.</summary>
        /// <param name="button">The controller button to check.</param>
        /// <param name="buttonState">The last known state.</param>
        /// <param name="stateIndex">The player whose controller to check.</param>
        private bool WasButtonJustReleased(Buttons button, ButtonState buttonState, PlayerIndex stateIndex)
        {
            return buttonState == ButtonState.Released && this.PreviouslyPressedButtons[(int)stateIndex].Contains(button);
        }

        /// <summary>Get whether an analogue controller button was pressed since the last check.</summary>
        /// <param name="button">The controller button to check.</param>
        /// <param name="value">The last known value.</param>
        /// <param name="stateIndex">The player whose controller to check.</param>
        private bool WasButtonJustPressed(Buttons button, float value, PlayerIndex stateIndex)
        {
            return this.WasButtonJustPressed(button, value > 0.2f ? ButtonState.Pressed : ButtonState.Released, stateIndex);
        }

        /// <summary>Get whether an analogue controller button was released since the last check.</summary>
        /// <param name="button">The controller button to check.</param>
        /// <param name="value">The last known value.</param>
        /// <param name="stateIndex">The player whose controller to check.</param>
        private bool WasButtonJustReleased(Buttons button, float value, PlayerIndex stateIndex)
        {
            return this.WasButtonJustReleased(button, value > 0.2f ? ButtonState.Pressed : ButtonState.Released, stateIndex);
        }

        /// <summary>Detect changes since the last update ticket and trigger mod events.</summary>
        private void UpdateEventCalls()
        {
            // save loaded event
            if (Game1.hasLoadedGame && this.AfterLoadTimer >= 0)
            {
                if (this.AfterLoadTimer == 0)
                {
                    SaveEvents.InvokeAfterLoad(this.Monitor);
                    PlayerEvents.InvokeLoadedGame(this.Monitor, new EventArgsLoadedGameChanged(Game1.hasLoadedGame));
                }
                this.AfterLoadTimer--;
            }

            // input events
            {
                // get latest state
                this.KStateNow = Keyboard.GetState();
                this.MStateNow = Mouse.GetState();
                this.MPositionNow = new Point(Game1.getMouseX(), Game1.getMouseY());

                // raise key pressed
                foreach (var key in this.FramePressedKeys)
                    ControlEvents.InvokeKeyPressed(this.Monitor, key);

                // raise key released
                foreach (var key in this.FrameReleasedKeys)
                    ControlEvents.InvokeKeyReleased(this.Monitor, key);

                // raise controller button pressed
                for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
                {
                    var buttons = this.GetFramePressedButtons(i);
                    foreach (var button in buttons)
                    {
                        if (button == Buttons.LeftTrigger || button == Buttons.RightTrigger)
                            ControlEvents.InvokeTriggerPressed(this.Monitor, i, button, button == Buttons.LeftTrigger ? GamePad.GetState(i).Triggers.Left : GamePad.GetState(i).Triggers.Right);
                        else
                            ControlEvents.InvokeButtonPressed(this.Monitor, i, button);
                    }
                }

                // raise controller button released
                for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
                {
                    foreach (var button in this.GetFrameReleasedButtons(i))
                    {
                        if (button == Buttons.LeftTrigger || button == Buttons.RightTrigger)
                            ControlEvents.InvokeTriggerReleased(this.Monitor, i, button, button == Buttons.LeftTrigger ? GamePad.GetState(i).Triggers.Left : GamePad.GetState(i).Triggers.Right);
                        else
                            ControlEvents.InvokeButtonReleased(this.Monitor, i, button);
                    }
                }

                // raise keyboard state changed
                if (this.KStateNow != this.KStatePrior)
                    ControlEvents.InvokeKeyboardChanged(this.Monitor, this.KStatePrior, this.KStateNow);

                // raise mouse state changed
                if (this.MStateNow != this.MStatePrior)
                {
                    ControlEvents.InvokeMouseChanged(this.Monitor, this.MStatePrior, this.MStateNow, this.MPositionPrior, this.MPositionNow);
                    this.MStatePrior = this.MStateNow;
                    this.MPositionPrior = this.MPositionPrior;
                }
            }

            // menu events
            if (Game1.activeClickableMenu != this.PreviousActiveMenu)
            {
                IClickableMenu previousMenu = this.PreviousActiveMenu;
                IClickableMenu newMenu = Game1.activeClickableMenu;

                // raise save events
                // (saving is performed by SaveGameMenu; on days when the player shipping something, ShippingMenu wraps SaveGameMenu)
                if (newMenu is SaveGameMenu || newMenu is ShippingMenu)
                    SaveEvents.InvokeBeforeSave(this.Monitor);
                else if (previousMenu is SaveGameMenu || previousMenu is ShippingMenu)
                    SaveEvents.InvokeAfterSave(this.Monitor);

                // raise menu events
                if (newMenu != null)
                    MenuEvents.InvokeMenuChanged(this.Monitor, previousMenu, newMenu);
                else
                    MenuEvents.InvokeMenuClosed(this.Monitor, previousMenu);

                // update previous menu
                // (if the menu was changed in one of the handlers, deliberately defer detection until the next update so mods can be notified of the new menu change)
                this.PreviousActiveMenu = newMenu;
            }

            // world & player events
            if (this.IsWorldReady)
            {
                // raise location list changed
                if (this.GetHash(Game1.locations) != this.PreviousGameLocations)
                {
                    LocationEvents.InvokeLocationsChanged(this.Monitor, Game1.locations);
                    this.PreviousGameLocations = this.GetHash(Game1.locations);
                }

                // raise current location changed
                if (Game1.currentLocation != this.PreviousGameLocation)
                {
                    LocationEvents.InvokeCurrentLocationChanged(this.Monitor, this.PreviousGameLocation, Game1.currentLocation);
                    this.PreviousGameLocation = Game1.currentLocation;
                }

                // raise player changed
                if (Game1.player != this.PreviousFarmer)
                {
                    PlayerEvents.InvokeFarmerChanged(this.Monitor, this.PreviousFarmer, Game1.player);
                    this.PreviousFarmer = Game1.player;
                }

                // raise player leveled up a skill
                if (Game1.player.combatLevel != this.PreviousCombatLevel)
                {
                    PlayerEvents.InvokeLeveledUp(this.Monitor, EventArgsLevelUp.LevelType.Combat, Game1.player.combatLevel);
                    this.PreviousCombatLevel = Game1.player.combatLevel;
                }
                if (Game1.player.farmingLevel != this.PreviousFarmingLevel)
                {
                    PlayerEvents.InvokeLeveledUp(this.Monitor, EventArgsLevelUp.LevelType.Farming, Game1.player.farmingLevel);
                    this.PreviousFarmingLevel = Game1.player.farmingLevel;
                }
                if (Game1.player.fishingLevel != this.PreviousFishingLevel)
                {
                    PlayerEvents.InvokeLeveledUp(this.Monitor, EventArgsLevelUp.LevelType.Fishing, Game1.player.fishingLevel);
                    this.PreviousFishingLevel = Game1.player.fishingLevel;
                }
                if (Game1.player.foragingLevel != this.PreviousForagingLevel)
                {
                    PlayerEvents.InvokeLeveledUp(this.Monitor, EventArgsLevelUp.LevelType.Foraging, Game1.player.foragingLevel);
                    this.PreviousForagingLevel = Game1.player.foragingLevel;
                }
                if (Game1.player.miningLevel != this.PreviousMiningLevel)
                {
                    PlayerEvents.InvokeLeveledUp(this.Monitor, EventArgsLevelUp.LevelType.Mining, Game1.player.miningLevel);
                    this.PreviousMiningLevel = Game1.player.miningLevel;
                }
                if (Game1.player.luckLevel != this.PreviousLuckLevel)
                {
                    PlayerEvents.InvokeLeveledUp(this.Monitor, EventArgsLevelUp.LevelType.Luck, Game1.player.luckLevel);
                    this.PreviousLuckLevel = Game1.player.luckLevel;
                }

                // raise player inventory changed
                ItemStackChange[] changedItems = this.GetInventoryChanges(Game1.player.items, this.PreviousItems).ToArray();
                if (changedItems.Any())
                {
                    PlayerEvents.InvokeInventoryChanged(this.Monitor, Game1.player.items, changedItems);
                    this.PreviousItems = Game1.player.items.Where(n => n != null).ToDictionary(n => n, n => n.Stack);
                }

                // raise current location's object list changed
                {
                    int? objectHash = Game1.currentLocation?.objects != null ? this.GetHash(Game1.currentLocation.objects) : (int?)null;
                    if (objectHash != null && this.PreviousLocationObjects != objectHash)
                    {
                        LocationEvents.InvokeOnNewLocationObject(this.Monitor, Game1.currentLocation.objects);
                        this.PreviousLocationObjects = objectHash.Value;
                    }
                }

                // raise time changed
                if (Game1.timeOfDay != this.PreviousTimeOfDay)
                {
                    TimeEvents.InvokeTimeOfDayChanged(this.Monitor, this.PreviousTimeOfDay, Game1.timeOfDay);
                    this.PreviousTimeOfDay = Game1.timeOfDay;
                }
                if (Game1.dayOfMonth != this.PreviousDayOfMonth)
                {
                    TimeEvents.InvokeDayOfMonthChanged(this.Monitor, this.PreviousDayOfMonth, Game1.dayOfMonth);
                    this.PreviousDayOfMonth = Game1.dayOfMonth;
                }
                if (Game1.currentSeason != this.PreviousSeasonOfYear)
                {
                    TimeEvents.InvokeSeasonOfYearChanged(this.Monitor, this.PreviousSeasonOfYear, Game1.currentSeason);
                    this.PreviousSeasonOfYear = Game1.currentSeason;
                }
                if (Game1.year != this.PreviousYearOfGame)
                {
                    TimeEvents.InvokeYearOfGameChanged(this.Monitor, this.PreviousYearOfGame, Game1.year);
                    this.PreviousYearOfGame = Game1.year;
                }

                // raise mine level changed
                if (Game1.mine != null && Game1.mine.mineLevel != this.PreviousMineLevel)
                {
                    MineEvents.InvokeMineLevelChanged(this.Monitor, this.PreviousMineLevel, Game1.mine.mineLevel);
                    this.PreviousMineLevel = Game1.mine.mineLevel;
                }
            }

            // raise game day transition event (obsolete)
            if (Game1.newDay != this.PreviousIsNewDay)
            {
                TimeEvents.InvokeOnNewDay(this.Monitor, this.PreviousDayOfMonth, Game1.dayOfMonth, Game1.newDay);
                this.PreviousIsNewDay = Game1.newDay;
            }
        }

        /// <summary>Get the player inventory changes between two states.</summary>
        /// <param name="current">The player's current inventory.</param>
        /// <param name="previous">The player's previous inventory.</param>
        private IEnumerable<ItemStackChange> GetInventoryChanges(IEnumerable<Item> current, IDictionary<Item, int> previous)
        {
            current = current.Where(n => n != null).ToArray();
            foreach (Item item in current)
            {
                // stack size changed
                if (previous != null && previous.ContainsKey(item))
                {
                    if (previous[item] != item.Stack)
                        yield return new ItemStackChange { Item = item, StackChange = item.Stack - previous[item], ChangeType = ChangeType.StackChange };
                }

                // new item
                else
                    yield return new ItemStackChange { Item = item, StackChange = item.Stack, ChangeType = ChangeType.Added };
            }

            // removed items
            if (previous != null)
            {
                foreach (var entry in previous)
                {
                    if (current.Any(i => i == entry.Key))
                        continue;

                    yield return new ItemStackChange { Item = entry.Key, StackChange = -entry.Key.Stack, ChangeType = ChangeType.Removed };
                }
            }
        }

        /// <summary>Get a hash value for an enumeration.</summary>
        /// <param name="enumerable">The enumeration of items to hash.</param>
        private int GetHash(IEnumerable enumerable)
        {
            var hash = 0;
            foreach (var v in enumerable)
                hash ^= v.GetHashCode();
            return hash;
        }

        /// <summary>Get reflection metadata for a private <see cref="Game1"/> field.</summary>
        /// <param name="name">The field name.</param>
        private FieldInfo GetBaseFieldInfo(string name)
        {
            return typeof(Game1).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        }

        /// <summary>Get the value of a private <see cref="Game1"/> field.</summary>
        /// <typeparam name="TValue">The expected value type.</typeparam>
        /// <param name="name">The field name.</param>
        private TValue GetBaseFieldValue<TValue>(string name) where TValue : class
        {
            return this.GetBaseFieldInfo(name).GetValue(Program.gamePtr) as TValue;
        }

        /// <summary>Set the value of a private <see cref="Game1"/> field.</summary>
        /// <typeparam name="TValue">The expected value type.</typeparam>
        /// <param name="name">The field name.</param>
        /// <param name="value">The value to set.</param>
        public void SetBaseFieldValue<TValue>(string name, object value) where TValue : class
        {
            this.GetBaseFieldInfo(name).SetValue(Program.gamePtr, value as TValue);
        }
    }
}
