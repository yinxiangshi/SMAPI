using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.Events;
using StardewModdingAPI.Framework.Input;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Framework.StateTracking.Snapshots;
using StardewModdingAPI.Framework.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewModdingAPI.Framework
{
    /// <summary>SMAPI's extension of the game's core <see cref="Game1"/>, used to inject events.</summary>
    internal class SGame : Game1
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging for SMAPI.</summary>
        private readonly Monitor Monitor;

        /// <summary>Manages SMAPI events for mods.</summary>
        private readonly EventManager Events;

        /// <summary>The maximum number of consecutive attempts SMAPI should make to recover from a draw error.</summary>
        private readonly Countdown DrawCrashTimer = new Countdown(60); // 60 ticks = roughly one second

        /// <summary>Simplifies access to private game code.</summary>
        private readonly Reflector Reflection;

        /// <summary>Immediately exit the game without saving. This should only be invoked when an irrecoverable fatal error happens that risks save corruption or game-breaking bugs.</summary>
        private readonly Action<string> ExitGameImmediately;

        /// <summary>The initial override for <see cref="Input"/>. This value is null after initialization.</summary>
        private SInputState InitialInput;

        /// <summary>The initial override for <see cref="Multiplayer"/>. This value is null after initialization.</summary>
        private SMultiplayer InitialMultiplayer;

        /// <summary>Raised when the instance is updating its state (roughly 60 times per second).</summary>
        private readonly Action<SGame, GameTime, Action> OnUpdating;


        /*********
        ** Accessors
        *********/
        /// <summary>Manages input visible to the game.</summary>
        public SInputState Input => (SInputState)Game1.input;

        /// <summary>The game background task which initializes a new day.</summary>
        public Task NewDayTask => Game1._newDayTask;

        /// <summary>Monitors the entire game state for changes.</summary>
        public WatcherCore Watchers { get; private set; }

        /// <summary>A snapshot of the current <see cref="Watchers"/> state.</summary>
        public WatcherSnapshot WatcherSnapshot { get; } = new WatcherSnapshot();

        /// <summary>Whether the current update tick is the first one for this instance.</summary>
        public bool IsFirstTick = true;

        /// <summary>The number of ticks until SMAPI should notify mods that the game has loaded.</summary>
        /// <remarks>Skipping a few frames ensures the game finishes initializing the world before mods try to change it.</remarks>
        public Countdown AfterLoadTimer { get; } = new Countdown(5);

        /// <summary>Whether the game is saving and SMAPI has already raised <see cref="IGameLoopEvents.Saving"/>.</summary>
        public bool IsBetweenSaveEvents { get; set; }

        /// <summary>Whether the game is creating the save file and SMAPI has already raised <see cref="IGameLoopEvents.SaveCreating"/>.</summary>
        public bool IsBetweenCreateEvents { get; set; }

        /// <summary>Construct a content manager to read game content files.</summary>
        /// <remarks>This must be static because the game accesses it before the <see cref="SGame"/> constructor is called.</remarks>
        [NonInstancedStatic]
        public static Func<IServiceProvider, string, LocalizedContentManager> CreateContentManagerImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="playerIndex">The player index.</param>
        /// <param name="instanceIndex">The instance index.</param>
        /// <param name="monitor">Encapsulates monitoring and logging for SMAPI.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="eventManager">Manages SMAPI events for mods.</param>
        /// <param name="input">Manages the game's input state.</param>
        /// <param name="modHooks">Handles mod hooks provided by the game.</param>
        /// <param name="multiplayer">The core multiplayer logic.</param>
        /// <param name="exitGameImmediately">Immediately exit the game without saving. This should only be invoked when an irrecoverable fatal error happens that risks save corruption or game-breaking bugs.</param>
        /// <param name="onUpdating">Raised when the instance is updating its state (roughly 60 times per second).</param>
        public SGame(PlayerIndex playerIndex, int instanceIndex, Monitor monitor, Reflector reflection, EventManager eventManager, SInputState input, SModHooks modHooks, SMultiplayer multiplayer, Action<string> exitGameImmediately, Action<SGame, GameTime, Action> onUpdating)
            : base(playerIndex, instanceIndex)
        {
            // init XNA
            Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;

            // hook into game
            Game1.input = this.InitialInput = input;
            Game1.multiplayer = this.InitialMultiplayer = multiplayer;
            Game1.hooks = modHooks;
            this._locations = new ObservableCollection<GameLocation>();

            // init SMAPI
            this.Monitor = monitor;
            this.Events = eventManager;
            this.Reflection = reflection;
            this.ExitGameImmediately = exitGameImmediately;
            this.OnUpdating = onUpdating;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct a content manager to read game content files.</summary>
        /// <param name="serviceProvider">The service provider to use to locate services.</param>
        /// <param name="rootDirectory">The root directory to search for content.</param>
        protected override LocalizedContentManager CreateContentManager(IServiceProvider serviceProvider, string rootDirectory)
        {
            if (SGame.CreateContentManagerImpl == null)
                throw new InvalidOperationException($"The {nameof(SGame)}.{nameof(SGame.CreateContentManagerImpl)} must be set.");

            return SGame.CreateContentManagerImpl(serviceProvider, rootDirectory);
        }

        /// <summary>Initialize the instance when the game starts.</summary>
        protected override void Initialize()
        {
            base.Initialize();

            // The game resets public static fields after the class is constructed (see
            // GameRunner.SetInstanceDefaults), so SMAPI needs to re-override them here.
            Game1.input = this.InitialInput;
            Game1.multiplayer = this.InitialMultiplayer;

            // The Initial* fields should no longer be used after this point, since mods may
            // further override them after initialization.
            this.InitialInput = null;
            this.InitialMultiplayer = null;
        }

        /// <summary>The method called when the instance is updating its state (roughly 60 times per second).</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        protected override void Update(GameTime gameTime)
        {
            // set initial state
            if (this.IsFirstTick)
            {
                this.Input.TrueUpdate();
                this.Watchers = new WatcherCore(this.Input, (ObservableCollection<GameLocation>)this._locations);
            }

            // update
            try
            {
                this.OnUpdating(this, gameTime, () => base.Update(gameTime));
            }
            finally
            {
                this.IsFirstTick = false;
            }
        }

        /// <summary>The method called to draw everything to the screen.</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        /// <param name="target_screen">The render target, if any.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "copied from game code as-is")]
        protected override void _draw(GameTime gameTime, RenderTarget2D target_screen)
        {
            Context.IsInDrawLoop = true;
            try
            {
                this.DrawImpl(gameTime, target_screen);
                this.DrawCrashTimer.Reset();
            }
            catch (Exception ex)
            {
                // log error
                this.Monitor.Log($"An error occurred in the overridden draw loop: {ex.GetLogSummary()}", LogLevel.Error);

                // exit if irrecoverable
                if (!this.DrawCrashTimer.Decrement())
                {
                    this.ExitGameImmediately("The game crashed when drawing, and SMAPI was unable to recover the game.");
                    return;
                }

                // recover draw state
                try
                {
                    if (Game1.spriteBatch.IsOpen(this.Reflection))
                    {
                        this.Monitor.Log("Recovering sprite batch from error...");
                        Game1.spriteBatch.End();
                    }
                }
                catch (Exception innerEx)
                {
                    this.Monitor.Log($"Could not recover sprite batch state: {innerEx.GetLogSummary()}", LogLevel.Error);
                }
            }
            Context.IsInDrawLoop = false;
        }

        /// <summary>Replicate the game's draw logic with some changes for SMAPI.</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        /// <param name="target_screen">The render target, if any.</param>
        /// <remarks>This implementation is identical to <see cref="Game1.Draw"/>, except for try..catch around menu draw code, private field references replaced by wrappers, and added events.</remarks>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "LocalVariableHidesMember", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "RedundantCast", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "RedundantExplicitNullableCreation", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod", Justification = "copied from game code as-is")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField", Justification = "copied from game code as-is")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast", Justification = "copied from game code as-is")]
        private void DrawImpl(GameTime gameTime, RenderTarget2D target_screen)
        {
            var events = this.Events;

            Game1.showingHealthBar = false;
            if (Game1._newDayTask != null)
            {
                base.GraphicsDevice.Clear(Game1.bgColor);
                return;
            }
            if (target_screen != null)
            {
                base.GraphicsDevice.SetRenderTarget(target_screen);
            }
            if (this.IsSaving)
            {
                base.GraphicsDevice.Clear(Game1.bgColor);
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu != null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    events.Rendering.RaiseEmpty();
                    try
                    {
                        events.RenderingActiveMenu.RaiseEmpty();
                        menu.draw(Game1.spriteBatch);
                        events.RenderedActiveMenu.RaiseEmpty();
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"The {activeClickableMenu.GetType().FullName} menu crashed while drawing itself during save. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        activeClickableMenu.exitThisMenu();
                    }
                    events.Rendered.RaiseEmpty();
                    Game1.spriteBatch.End();
                }
                if (Game1.overlayMenu != null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Game1.overlayMenu.draw(Game1.spriteBatch);
                    Game1.spriteBatch.End();
                }
                this.renderScreenBuffer(target_screen);
                return;
            }
            base.GraphicsDevice.Clear(Game1.bgColor);
            if (Game1.activeClickableMenu != null && Game1.options.showMenuBackground && Game1.activeClickableMenu.showWithoutTransparencyIfOptionIsSet() && !this.takingMapScreenshot)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                events.Rendering.RaiseEmpty();
                try
                {
                    Game1.activeClickableMenu.drawBackground(Game1.spriteBatch);
                    events.RenderingActiveMenu.RaiseEmpty();
                    Game1.activeClickableMenu.draw(Game1.spriteBatch);
                    events.RenderedActiveMenu.RaiseEmpty();
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                    Game1.activeClickableMenu.exitThisMenu();
                }
                events.Rendered.RaiseEmpty();
                Game1.spriteBatch.End();
                this.drawOverlays(Game1.spriteBatch);
                if (target_screen != null)
                {
                    base.GraphicsDevice.SetRenderTarget(null);
                    base.GraphicsDevice.Clear(Game1.bgColor);
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    Game1.spriteBatch.Draw(target_screen, Vector2.Zero, target_screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                    Game1.spriteBatch.End();
                }
                if (Game1.overlayMenu != null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Game1.overlayMenu.draw(Game1.spriteBatch);
                    Game1.spriteBatch.End();
                }
                return;
            }
            if (Game1.gameMode == 11)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                events.Rendering.RaiseEmpty();
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Color.HotPink);
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Color(0, 255, 0));
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.parseText(Game1.errorMessage, Game1.dialogueFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
                events.Rendered.RaiseEmpty();
                Game1.spriteBatch.End();
                return;
            }
            if (Game1.currentMinigame != null)
            {
                if (events.Rendering.HasListeners())
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    events.Rendering.RaiseEmpty();
                    Game1.spriteBatch.End();
                }

                Game1.currentMinigame.draw(Game1.spriteBatch);
                if (Game1.globalFade && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause))
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
                    Game1.spriteBatch.End();
                }
                this.drawOverlays(Game1.spriteBatch);
                if (target_screen != null)
                {
                    base.GraphicsDevice.SetRenderTarget(null);
                    base.GraphicsDevice.Clear(Game1.bgColor);
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    Game1.spriteBatch.Draw(target_screen, Vector2.Zero, target_screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                    events.Rendered.RaiseEmpty();
                    Game1.spriteBatch.End();
                }
                else
                {
                    if (events.Rendered.HasListeners())
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        events.Rendered.RaiseEmpty();
                        Game1.spriteBatch.End();
                    }
                }
                return;
            }
            if (Game1.showingEndOfNightStuff)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                events.Rendering.RaiseEmpty();
                if (Game1.activeClickableMenu != null)
                {
                    try
                    {
                        events.RenderingActiveMenu.RaiseEmpty();
                        Game1.activeClickableMenu.draw(Game1.spriteBatch);
                        events.RenderedActiveMenu.RaiseEmpty();
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself during end-of-night-stuff. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                }
                events.Rendered.RaiseEmpty();
                Game1.spriteBatch.End();
                this.drawOverlays(Game1.spriteBatch);
                if (target_screen != null)
                {
                    base.GraphicsDevice.SetRenderTarget(null);
                    base.GraphicsDevice.Clear(Game1.bgColor);
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    Game1.spriteBatch.Draw(target_screen, Vector2.Zero, target_screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                    Game1.spriteBatch.End();
                }
                return;
            }
            if (Game1.gameMode == 6 || (Game1.gameMode == 3 && Game1.currentLocation == null))
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                events.Rendering.RaiseEmpty();
                string addOn = "";
                for (int i = 0; (double)i < gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0; i++)
                {
                    addOn += ".";
                }
                string str = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
                string msg = str + addOn;
                string largestMessage = str + "... ";
                int msgw = SpriteText.getWidthOfString(largestMessage);
                int msgh = 64;
                int msgx = 64;
                int msgy = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - msgh;
                SpriteText.drawString(Game1.spriteBatch, msg, msgx, msgy, 999999, msgw, msgh, 1f, 0.88f, junimoText: false, 0, largestMessage);
                events.Rendered.RaiseEmpty();
                Game1.spriteBatch.End();
                this.drawOverlays(Game1.spriteBatch);
                if (target_screen != null)
                {
                    base.GraphicsDevice.SetRenderTarget(null);
                    base.GraphicsDevice.Clear(Game1.bgColor);
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    Game1.spriteBatch.Draw(target_screen, Vector2.Zero, target_screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                    Game1.spriteBatch.End();
                }
                if (Game1.overlayMenu != null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Game1.overlayMenu.draw(Game1.spriteBatch);
                    Game1.spriteBatch.End();
                }
                //base.Draw(gameTime);
                return;
            }
            byte batchOpens = 0; // used for rendering event
            if (Game1.gameMode == 0)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (++batchOpens == 1)
                    events.Rendering.RaiseEmpty();
            }
            else
            {
                if (Game1.drawLighting)
                {
                    base.GraphicsDevice.SetRenderTarget(Game1.lightmap);
                    base.GraphicsDevice.Clear(Color.White * 0f);
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                    if (++batchOpens == 1)
                        events.Rendering.RaiseEmpty();
                    Color lighting = (Game1.currentLocation.Name.StartsWith("UndergroundMine") && Game1.currentLocation is MineShaft) ? (Game1.currentLocation as MineShaft).getLightingColor(gameTime) : ((Game1.ambientLight.Equals(Color.White) || (Game1.isRaining && (bool)Game1.currentLocation.isOutdoors)) ? Game1.outdoorLight : Game1.ambientLight);
                    Game1.spriteBatch.Draw(Game1.staminaRect, Game1.lightmap.Bounds, lighting);
                    foreach (LightSource lightSource in Game1.currentLightSources)
                    {
                        if ((Game1.isRaining || Game1.isDarkOut()) && lightSource.lightContext.Value == LightSource.LightContext.WindowLight)
                        {
                            continue;
                        }
                        if (lightSource.PlayerID != 0L && lightSource.PlayerID != Game1.player.UniqueMultiplayerID)
                        {
                            Farmer farmer = Game1.getFarmerMaybeOffline(lightSource.PlayerID);
                            if (farmer == null || (farmer.currentLocation != null && farmer.currentLocation.Name != Game1.currentLocation.Name) || (bool)farmer.hidden)
                            {
                                continue;
                            }
                        }
                        if (Utility.isOnScreen(lightSource.position, (int)((float)lightSource.radius * 64f * 4f)))
                        {
                            Game1.spriteBatch.Draw(lightSource.lightTexture, Game1.GlobalToLocal(Game1.viewport, lightSource.position) / (Game1.options.lightingQuality / 2), lightSource.lightTexture.Bounds, lightSource.color, 0f, new Vector2(lightSource.lightTexture.Bounds.Center.X, lightSource.lightTexture.Bounds.Center.Y), (float)lightSource.radius / (float)(Game1.options.lightingQuality / 2), SpriteEffects.None, 0.9f);
                        }
                    }
                    Game1.spriteBatch.End();
                    base.GraphicsDevice.SetRenderTarget(target_screen);
                }
                if (Game1.bloomDay && Game1.bloom != null)
                {
                    Game1.bloom.BeginDraw();
                }
                base.GraphicsDevice.Clear(Game1.bgColor);
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (++batchOpens == 1)
                    events.Rendering.RaiseEmpty();
                events.RenderingWorld.RaiseEmpty();
                if (Game1.background != null)
                {
                    Game1.background.draw(Game1.spriteBatch);
                }
                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                Game1.currentLocation.Map.GetLayer("Back").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
                Game1.currentLocation.drawWater(Game1.spriteBatch);
                this._farmerShadows.Clear();
                if (Game1.currentLocation.currentEvent != null && !Game1.currentLocation.currentEvent.isFestival && Game1.currentLocation.currentEvent.farmerActors.Count > 0)
                {
                    foreach (Farmer f in Game1.currentLocation.currentEvent.farmerActors)
                    {
                        if ((f.IsLocalPlayer && Game1.displayFarmer) || !f.hidden)
                        {
                            this._farmerShadows.Add(f);
                        }
                    }
                }
                else
                {
                    foreach (Farmer f2 in Game1.currentLocation.farmers)
                    {
                        if ((f2.IsLocalPlayer && Game1.displayFarmer) || !f2.hidden)
                        {
                            this._farmerShadows.Add(f2);
                        }
                    }
                }
                if (!Game1.currentLocation.shouldHideCharacters())
                {
                    if (Game1.CurrentEvent == null)
                    {
                        foreach (NPC k in Game1.currentLocation.characters)
                        {
                            if (!k.swimming && !k.HideShadow && !k.IsInvisible && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(k.getTileLocation()))
                            {
                                Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, k.Position + new Vector2((float)(k.Sprite.SpriteWidth * 4) / 2f, k.GetBoundingBox().Height + ((!k.IsMonster) ? 12 : 0))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (4f + (float)k.yJumpOffset / 40f) * (float)k.scale, SpriteEffects.None, Math.Max(0f, (float)k.getStandingY() / 10000f) - 1E-06f);
                            }
                        }
                    }
                    else
                    {
                        foreach (NPC l in Game1.CurrentEvent.actors)
                        {
                            if (!l.swimming && !l.HideShadow && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(l.getTileLocation()))
                            {
                                Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, l.Position + new Vector2((float)(l.Sprite.SpriteWidth * 4) / 2f, l.GetBoundingBox().Height + ((!l.IsMonster) ? ((l.Sprite.SpriteHeight <= 16) ? (-4) : 12) : 0))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (4f + (float)l.yJumpOffset / 40f) * (float)l.scale, SpriteEffects.None, Math.Max(0f, (float)l.getStandingY() / 10000f) - 1E-06f);
                            }
                        }
                    }
                    foreach (Farmer f3 in this._farmerShadows)
                    {
                        if (!Game1.multiplayer.isDisconnecting(f3.UniqueMultiplayerID) && !f3.swimming && !f3.isRidingHorse() && (Game1.currentLocation == null || !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(f3.getTileLocation())))
                        {
                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(f3.Position + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f - (((f3.running || f3.UsingTool) && f3.FarmerSprite.currentAnimationIndex > 1) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[f3.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, 0f);
                        }
                    }
                }
                Layer building_layer = Game1.currentLocation.Map.GetLayer("Buildings");
                building_layer.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
                Game1.mapDisplayDevice.EndScene();
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (!Game1.currentLocation.shouldHideCharacters())
                {
                    if (Game1.CurrentEvent == null)
                    {
                        foreach (NPC n in Game1.currentLocation.characters)
                        {
                            if (!n.swimming && !n.HideShadow && !n.isInvisible && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(n.getTileLocation()))
                            {
                                Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, n.Position + new Vector2((float)(n.Sprite.SpriteWidth * 4) / 2f, n.GetBoundingBox().Height + ((!n.IsMonster) ? 12 : 0))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (4f + (float)n.yJumpOffset / 40f) * (float)n.scale, SpriteEffects.None, Math.Max(0f, (float)n.getStandingY() / 10000f) - 1E-06f);
                            }
                        }
                    }
                    else
                    {
                        foreach (NPC n2 in Game1.CurrentEvent.actors)
                        {
                            if (!n2.swimming && !n2.HideShadow && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(n2.getTileLocation()))
                            {
                                Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, n2.Position + new Vector2((float)(n2.Sprite.SpriteWidth * 4) / 2f, n2.GetBoundingBox().Height + ((!n2.IsMonster) ? 12 : 0))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (4f + (float)n2.yJumpOffset / 40f) * (float)n2.scale, SpriteEffects.None, Math.Max(0f, (float)n2.getStandingY() / 10000f) - 1E-06f);
                            }
                        }
                    }
                    foreach (Farmer f4 in this._farmerShadows)
                    {
                        float draw_layer = Math.Max(0.0001f, f4.getDrawLayer() + 0.00011f) - 0.0001f;
                        if (!f4.swimming && !f4.isRidingHorse() && Game1.currentLocation != null && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(f4.getTileLocation()))
                        {
                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(f4.Position + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f - (((f4.running || f4.UsingTool) && f4.FarmerSprite.currentAnimationIndex > 1) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[f4.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, draw_layer);
                        }
                    }
                }
                if ((Game1.eventUp || Game1.killScreen) && !Game1.killScreen && Game1.currentLocation.currentEvent != null)
                {
                    Game1.currentLocation.currentEvent.draw(Game1.spriteBatch);
                }
                if (Game1.player.currentUpgrade != null && Game1.player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && Game1.currentLocation.Name.Equals("Farm"))
                {
                    Game1.spriteBatch.Draw(Game1.player.currentUpgrade.workerTexture, Game1.GlobalToLocal(Game1.viewport, Game1.player.currentUpgrade.positionOfCarpenter), Game1.player.currentUpgrade.getSourceRectangle(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (Game1.player.currentUpgrade.positionOfCarpenter.Y + 48f) / 10000f);
                }
                Game1.currentLocation.draw(Game1.spriteBatch);
                foreach (Vector2 tile_position in Game1.crabPotOverlayTiles.Keys)
                {
                    Tile tile = building_layer.Tiles[(int)tile_position.X, (int)tile_position.Y];
                    if (tile != null)
                    {
                        Vector2 vector_draw_position = Game1.GlobalToLocal(Game1.viewport, tile_position * 64f);
                        Location draw_location = new Location((int)vector_draw_position.X, (int)vector_draw_position.Y);
                        Game1.mapDisplayDevice.DrawTile(tile, draw_location, (tile_position.Y * 64f - 1f) / 10000f);
                    }
                }
                if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                {
                    _ = Game1.currentLocation.currentEvent.messageToScreen;
                }
                if (Game1.player.ActiveObject == null && (Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && (!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool))
                {
                    Game1.drawTool(Game1.player);
                }
                if (Game1.currentLocation.Name.Equals("Farm"))
                {
                    this.drawFarmBuildings();
                }
                if (Game1.tvStation >= 0)
                {
                    Game1.spriteBatch.Draw(Game1.tvStationTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(400f, 160f)), new Microsoft.Xna.Framework.Rectangle(Game1.tvStation * 24, 0, 24, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
                }
                if (Game1.panMode)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(Game1.getOldMouseX() + Game1.viewport.X) / 64.0) * 64 - Game1.viewport.X, (int)Math.Floor((double)(Game1.getOldMouseY() + Game1.viewport.Y) / 64.0) * 64 - Game1.viewport.Y, 64, 64), Color.Lime * 0.75f);
                    foreach (Warp w in Game1.currentLocation.warps)
                    {
                        Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(w.X * 64 - Game1.viewport.X, w.Y * 64 - Game1.viewport.Y, 64, 64), Color.Red * 0.75f);
                    }
                }
                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                Game1.currentLocation.Map.GetLayer("Front").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
                Game1.mapDisplayDevice.EndScene();
                Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (Game1.displayFarmer && Game1.player.ActiveObject != null && (bool)Game1.player.ActiveObject.bigCraftable && this.checkBigCraftableBoundariesForFrontLayer() && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)
                {
                    Game1.drawPlayerHeldObject(Game1.player);
                }
                else if (Game1.displayFarmer && Game1.player.ActiveObject != null && ((Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y - 38), Game1.viewport.Size) != null && !Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y - 38), Game1.viewport.Size).TileIndexProperties.ContainsKey("FrontAlways")) || (Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.GetBoundingBox().Right, (int)Game1.player.Position.Y - 38), Game1.viewport.Size) != null && !Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.GetBoundingBox().Right, (int)Game1.player.Position.Y - 38), Game1.viewport.Size).TileIndexProperties.ContainsKey("FrontAlways"))))
                {
                    Game1.drawPlayerHeldObject(Game1.player);
                }
                if ((Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && (!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool) && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), (int)Game1.player.Position.Y - 38), Game1.viewport.Size) != null && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)
                {
                    Game1.drawTool(Game1.player);
                }
                if (Game1.currentLocation.Map.GetLayer("AlwaysFront") != null)
                {
                    Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                    Game1.currentLocation.Map.GetLayer("AlwaysFront").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
                    Game1.mapDisplayDevice.EndScene();
                }
                if (Game1.toolHold > 400f && Game1.player.CurrentTool.UpgradeLevel >= 1 && Game1.player.canReleaseTool)
                {
                    Color barColor = Color.White;
                    switch ((int)(Game1.toolHold / 600f) + 2)
                    {
                        case 1:
                            barColor = Tool.copperColor;
                            break;
                        case 2:
                            barColor = Tool.steelColor;
                            break;
                        case 3:
                            barColor = Tool.goldColor;
                            break;
                        case 4:
                            barColor = Tool.iridiumColor;
                            break;
                    }
                    Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - 2, (int)Game1.player.getLocalPosition(Game1.viewport).Y - ((!Game1.player.CurrentTool.Name.Equals("Watering Can")) ? 64 : 0) - 2, (int)(Game1.toolHold % 600f * 0.08f) + 4, 12), Color.Black);
                    Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X, (int)Game1.player.getLocalPosition(Game1.viewport).Y - ((!Game1.player.CurrentTool.Name.Equals("Watering Can")) ? 64 : 0), (int)(Game1.toolHold % 600f * 0.08f), 8), barColor);
                }
                this.drawWeather(gameTime, target_screen);
                if (Game1.farmEvent != null)
                {
                    Game1.farmEvent.draw(Game1.spriteBatch);
                }
                if (Game1.currentLocation.LightLevel > 0f && Game1.timeOfDay < 2000)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * Game1.currentLocation.LightLevel);
                }
                if (Game1.screenGlow)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
                }
                Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);
                if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod && ((Game1.player.CurrentTool as FishingRod).isTimingCast || (Game1.player.CurrentTool as FishingRod).castingChosenCountdown > 0f || (Game1.player.CurrentTool as FishingRod).fishCaught || (Game1.player.CurrentTool as FishingRod).showingTreasure))
                {
                    Game1.player.CurrentTool.draw(Game1.spriteBatch);
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                {
                    foreach (NPC m in Game1.currentLocation.currentEvent.actors)
                    {
                        if (m.isEmoting)
                        {
                            Vector2 emotePosition = m.getLocalPosition(Game1.viewport);
                            emotePosition.Y -= 140f;
                            if (m.Age == 2)
                            {
                                emotePosition.Y += 32f;
                            }
                            else if (m.Gender == 1)
                            {
                                emotePosition.Y += 10f;
                            }
                            Game1.spriteBatch.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(m.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, m.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)m.getStandingY() / 10000f);
                        }
                    }
                }
                Game1.spriteBatch.End();
                if (Game1.drawLighting)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, this.lightingBlend, SamplerState.LinearClamp, null, null);
                    Game1.spriteBatch.Draw(Game1.lightmap, Vector2.Zero, Game1.lightmap.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.lightingQuality / 2, SpriteEffects.None, 1f);
                    if (Game1.isRaining && (bool)Game1.currentLocation.isOutdoors && !(Game1.currentLocation is Desert))
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.OrangeRed * 0.45f);
                    }
                    Game1.spriteBatch.End();
                }
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                events.RenderedWorld.RaiseEmpty();
                if (Game1.drawGrid)
                {
                    int startingX = -Game1.viewport.X % 64;
                    float startingY = -Game1.viewport.Y % 64;
                    for (int x = startingX; x < Game1.graphics.GraphicsDevice.Viewport.Width; x += 64)
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(x, (int)startingY, 1, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
                    }
                    for (float y = startingY; y < (float)Game1.graphics.GraphicsDevice.Viewport.Height; y += 64f)
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(startingX, (int)y, Game1.graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
                    }
                }
                if (Game1.currentBillboard != 0 && !this.takingMapScreenshot)
                {
                    this.drawBillboard();
                }
                if (!Game1.eventUp && Game1.farmEvent == null && Game1.currentBillboard == 0 && Game1.gameMode == 3 && !this.takingMapScreenshot && Game1.isOutdoorMapSmallerThanViewport())
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, -Math.Min(Game1.viewport.X, 4096), Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64, 0, Math.Min(4096, Game1.graphics.GraphicsDevice.Viewport.Width - (-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64)), Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
                }
                if ((Game1.displayHUD || Game1.eventUp) && Game1.currentBillboard == 0 && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode && !Game1.HostPaused && !this.takingMapScreenshot)
                {
                    events.RenderingHud.RaiseEmpty();
                    this.drawHUD();
                    events.RenderedHud.RaiseEmpty();
                }
                else if (Game1.activeClickableMenu == null)
                {
                    _ = Game1.farmEvent;
                }
                if (Game1.hudMessages.Count > 0 && !this.takingMapScreenshot)
                {
                    for (int j = Game1.hudMessages.Count - 1; j >= 0; j--)
                    {
                        Game1.hudMessages[j].draw(Game1.spriteBatch, j);
                    }
                }
            }
            if (Game1.farmEvent != null)
            {
                Game1.farmEvent.draw(Game1.spriteBatch);
            }
            if (Game1.dialogueUp && !Game1.nameSelectUp && !Game1.messagePause && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox)) && !this.takingMapScreenshot)
            {
                this.drawDialogueBox();
            }
            if (Game1.progressBar && !this.takingMapScreenshot)
            {
                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 128, Game1.dialogueWidth, 32), Color.LightGray);
                Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 128, (int)(Game1.pauseAccumulator / Game1.pauseTime * (float)Game1.dialogueWidth), 32), Color.DimGray);
            }
            if (Game1.eventUp && Game1.currentLocation != null && Game1.currentLocation.currentEvent != null)
            {
                Game1.currentLocation.currentEvent.drawAfterMap(Game1.spriteBatch);
            }
            if (Game1.isRaining && Game1.currentLocation != null && (bool)Game1.currentLocation.isOutdoors && !(Game1.currentLocation is Desert))
            {
                Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Blue * 0.2f);
            }
            if ((Game1.fadeToBlack || Game1.globalFade) && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause) && !this.takingMapScreenshot)
            {
                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
            }
            else if (Game1.flashAlpha > 0f && !this.takingMapScreenshot)
            {
                if (Game1.options.screenFlash)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, Game1.flashAlpha));
                }
                Game1.flashAlpha -= 0.1f;
            }
            if ((Game1.messagePause || Game1.globalFade) && Game1.dialogueUp && !this.takingMapScreenshot)
            {
                this.drawDialogueBox();
            }
            if (!this.takingMapScreenshot)
            {
                foreach (TemporaryAnimatedSprite screenOverlayTempSprite in Game1.screenOverlayTempSprites)
                {
                    screenOverlayTempSprite.draw(Game1.spriteBatch, localPosition: true);
                }
            }
            if (Game1.debugMode)
            {
                StringBuilder sb = Game1._debugStringBuilder;
                sb.Clear();
                if (Game1.panMode)
                {
                    sb.Append((Game1.getOldMouseX() + Game1.viewport.X) / 64);
                    sb.Append(",");
                    sb.Append((Game1.getOldMouseY() + Game1.viewport.Y) / 64);
                }
                else
                {
                    sb.Append("player: ");
                    sb.Append(Game1.player.getStandingX() / 64);
                    sb.Append(", ");
                    sb.Append(Game1.player.getStandingY() / 64);
                }
                sb.Append(" mouseTransparency: ");
                sb.Append(Game1.mouseCursorTransparency);
                sb.Append(" mousePosition: ");
                sb.Append(Game1.getMouseX());
                sb.Append(",");
                sb.Append(Game1.getMouseY());
                sb.Append(Environment.NewLine);
                sb.Append(" mouseWorldPosition: ");
                sb.Append(Game1.getMouseX() + Game1.viewport.X);
                sb.Append(",");
                sb.Append(Game1.getMouseY() + Game1.viewport.Y);
                sb.Append("  debugOutput: ");
                sb.Append(Game1.debugOutput);
                Game1.spriteBatch.DrawString(Game1.smallFont, sb, new Vector2(base.GraphicsDevice.Viewport.GetTitleSafeArea().X, base.GraphicsDevice.Viewport.GetTitleSafeArea().Y + Game1.smallFont.LineSpacing * 8), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
            }
            if (Game1.showKeyHelp && !this.takingMapScreenshot)
            {
                Game1.spriteBatch.DrawString(Game1.smallFont, Game1.keyHelpString, new Vector2(64f, (float)(Game1.viewport.Height - 64 - (Game1.dialogueUp ? (192 + (Game1.isQuestion ? (Game1.questionChoices.Count * 64) : 0)) : 0)) - Game1.smallFont.MeasureString(Game1.keyHelpString).Y), Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
            }
            if (Game1.activeClickableMenu != null && !this.takingMapScreenshot)
            {
                try
                {
                    events.RenderingActiveMenu.RaiseEmpty();
                    Game1.activeClickableMenu.draw(Game1.spriteBatch);
                    events.RenderedActiveMenu.RaiseEmpty();
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                    Game1.activeClickableMenu.exitThisMenu();
                }
            }
            else if (Game1.farmEvent != null)
            {
                Game1.farmEvent.drawAboveEverything(Game1.spriteBatch);
            }
            if (Game1.emoteMenu != null && !this.takingMapScreenshot)
            {
                Game1.emoteMenu.draw(Game1.spriteBatch);
            }
            if (Game1.HostPaused && !this.takingMapScreenshot)
            {
                string msg2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
                SpriteText.drawStringWithScrollBackground(Game1.spriteBatch, msg2, 96, 32);
            }
            events.Rendered.RaiseEmpty();
            Game1.spriteBatch.End();
            this.drawOverlays(Game1.spriteBatch);
            this.renderScreenBuffer(target_screen);
        }
    }
}
