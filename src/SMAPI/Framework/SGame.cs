using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI.Framework.Events;
using StardewModdingAPI.Framework.Input;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Framework.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Events;
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


        /*********
        ** Accessors
        *********/
        /// <summary>Manages input visible to the game.</summary>
        public static SInputState Input => (SInputState)Game1.input;

        /// <summary>The game's core multiplayer utility.</summary>
        public static SMultiplayer Multiplayer => (SMultiplayer)Game1.multiplayer;

        /// <summary>The game background task which initializes a new day.</summary>
        public static Task NewDayTask => Game1._newDayTask;

        /// <summary>Construct a content manager to read game content files.</summary>
        /// <remarks>This must be static because the game accesses it before the <see cref="SGame"/> constructor is called.</remarks>
        public static Func<IServiceProvider, string, LocalizedContentManager> CreateContentManagerImpl;

        /// <summary>Raised after the game finishes loading its initial content.</summary>
        public event Action OnGameContentLoaded;

        /// <summary>Raised before the game exits.</summary>
        public event Action OnGameExiting;

        /// <summary>Raised when the game is updating its state (roughly 60 times per second).</summary>
        public event Action<GameTime, Action> OnGameUpdating;


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging for SMAPI.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="eventManager">Manages SMAPI events for mods.</param>
        /// <param name="modHooks">Handles mod hooks provided by the game.</param>
        /// <param name="multiplayer">The core multiplayer logic.</param>
        /// <param name="exitGameImmediately">Immediately exit the game without saving. This should only be invoked when an irrecoverable fatal error happens that risks save corruption or game-breaking bugs.</param>
        internal SGame(Monitor monitor, Reflector reflection, EventManager eventManager, SModHooks modHooks, SMultiplayer multiplayer, Action<string> exitGameImmediately)
        {
            // init XNA
            Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;

            // hook into game
            Game1.input = new SInputState();
            Game1.multiplayer = multiplayer;
            Game1.hooks = modHooks;
            Game1.locations = new ObservableCollection<GameLocation>();

            // init SMAPI
            this.Monitor = monitor;
            this.Events = eventManager;
            this.Reflection = reflection;
            this.ExitGameImmediately = exitGameImmediately;
        }

        /// <summary>Load content when the game is launched.</summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            this.OnGameContentLoaded?.Invoke();
        }

        /// <summary>Perform cleanup logic when the game exits.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event args.</param>
        /// <remarks>This overrides the logic in <see cref="Game1.exitEvent"/> to let SMAPI clean up before exit.</remarks>
        protected override void OnExiting(object sender, EventArgs args)
        {
            this.OnGameExiting?.Invoke();
        }

        /// <summary>Construct a content manager to read game content files.</summary>
        /// <param name="serviceProvider">The service provider to use to locate services.</param>
        /// <param name="rootDirectory">The root directory to search for content.</param>
        protected override LocalizedContentManager CreateContentManager(IServiceProvider serviceProvider, string rootDirectory)
        {
            if (SGame.CreateContentManagerImpl == null)
                throw new InvalidOperationException($"The {nameof(SGame)}.{nameof(SGame.CreateContentManagerImpl)} must be set.");

            return SGame.CreateContentManagerImpl(serviceProvider, rootDirectory);
        }

        /// <summary>The method called when the game is updating its state (roughly 60 times per second).</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        protected override void Update(GameTime gameTime)
        {
            this.OnGameUpdating?.Invoke(gameTime, () => base.Update(gameTime));
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
                this.Monitor.Log($"An error occured in the overridden draw loop: {ex.GetLogSummary()}", LogLevel.Error);

                // exit if irrecoverable
                if (!this.DrawCrashTimer.Decrement())
                {
                    this.ExitGameImmediately("The game crashed when drawing, and SMAPI was unable to recover the game.");
                    return;
                }

                // recover sprite batch
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
                this.GraphicsDevice.Clear(Game1.bgColor);
            }
            else
            {
                if (target_screen != null)
                    this.GraphicsDevice.SetRenderTarget(target_screen);
                if (this.IsSaving)
                {
                    this.GraphicsDevice.Clear(Game1.bgColor);
                    IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
                    if (activeClickableMenu != null)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        events.Rendering.RaiseEmpty();
                        try
                        {
                            events.RenderingActiveMenu.RaiseEmpty();
                            activeClickableMenu.draw(Game1.spriteBatch);
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
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        Game1.overlayMenu.draw(Game1.spriteBatch);
                        Game1.spriteBatch.End();
                    }
                    this.renderScreenBuffer(target_screen);
                }
                else
                {
                    this.GraphicsDevice.Clear(Game1.bgColor);
                    if (Game1.activeClickableMenu != null && Game1.options.showMenuBackground && (Game1.activeClickableMenu.showWithoutTransparencyIfOptionIsSet() && !this.takingMapScreenshot))
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);

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
                            this.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                            this.GraphicsDevice.Clear(Game1.bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            Game1.spriteBatch.Draw((Texture2D)target_screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(target_screen.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                        }
                        if (Game1.overlayMenu == null)
                            return;
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        Game1.overlayMenu.draw(Game1.spriteBatch);
                        Game1.spriteBatch.End();
                    }
                    else if (Game1.gameMode == (byte)11)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        events.Rendering.RaiseEmpty();
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Microsoft.Xna.Framework.Color.HotPink);
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Microsoft.Xna.Framework.Color(0, (int)byte.MaxValue, 0));
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.parseText(Game1.errorMessage, Game1.dialogueFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Microsoft.Xna.Framework.Color.White);
                        events.Rendered.RaiseEmpty();
                        Game1.spriteBatch.End();
                    }
                    else if (Game1.currentMinigame != null)
                    {
                        int batchEnds = 0;

                        if (events.Rendering.HasListeners())
                        {
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                            events.Rendering.RaiseEmpty();
                            Game1.spriteBatch.End();
                        }
                        Game1.currentMinigame.draw(Game1.spriteBatch);
                        if (Game1.globalFade && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause))
                        {
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Microsoft.Xna.Framework.Color.Black * (Game1.gameMode == (byte)0 ? 1f - Game1.fadeToBlackAlpha : Game1.fadeToBlackAlpha));
                            Game1.spriteBatch.End();
                        }
                        this.drawOverlays(Game1.spriteBatch);
                        if (target_screen == null)
                        {
                            if (++batchEnds == 1 && events.Rendered.HasListeners())
                            {
                                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                                events.Rendered.RaiseEmpty();
                                Game1.spriteBatch.End();
                            }
                            return;
                        }
                        this.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                        this.GraphicsDevice.Clear(Game1.bgColor);
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        Game1.spriteBatch.Draw((Texture2D)target_screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(target_screen.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                        if (++batchEnds == 1)
                            events.Rendered.RaiseEmpty();
                        Game1.spriteBatch.End();
                    }
                    else if (Game1.showingEndOfNightStuff)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
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
                        if (target_screen == null)
                            return;
                        this.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                        this.GraphicsDevice.Clear(Game1.bgColor);
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        Game1.spriteBatch.Draw((Texture2D)target_screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(target_screen.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                        Game1.spriteBatch.End();
                    }
                    else if (Game1.gameMode == (byte)6 || Game1.gameMode == (byte)3 && Game1.currentLocation == null)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        events.Rendering.RaiseEmpty();
                        string str1 = "";
                        for (int index = 0; (double)index < gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0; ++index)
                            str1 += ".";
                        string str2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
                        string s = str2 + str1;
                        string str3 = str2 + "... ";
                        int widthOfString = SpriteText.getWidthOfString(str3, 999999);
                        int height = 64;
                        int x = 64;
                        int y = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - height;
                        SpriteText.drawString(Game1.spriteBatch, s, x, y, 999999, widthOfString, height, 1f, 0.88f, false, 0, str3, -1, SpriteText.ScrollTextAlignment.Left);
                        events.Rendered.RaiseEmpty();
                        Game1.spriteBatch.End();
                        this.drawOverlays(Game1.spriteBatch);
                        if (target_screen != null)
                        {
                            this.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                            this.GraphicsDevice.Clear(Game1.bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            Game1.spriteBatch.Draw((Texture2D)target_screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(target_screen.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                        }
                        if (Game1.overlayMenu != null)
                        {
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            Game1.overlayMenu.draw(Game1.spriteBatch);
                            Game1.spriteBatch.End();
                        }
                        //base.Draw(gameTime);
                    }
                    else
                    {
                        byte batchOpens = 0; // used for rendering event

                        Microsoft.Xna.Framework.Rectangle rectangle;
                        Viewport viewport;
                        if (Game1.gameMode == (byte)0)
                        {
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (++batchOpens == 1)
                                events.Rendering.RaiseEmpty();
                        }
                        else
                        {
                            if (Game1.drawLighting)
                            {
                                this.GraphicsDevice.SetRenderTarget(Game1.lightmap);
                                this.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.White * 0.0f);
                                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                                if (++batchOpens == 1)
                                    events.Rendering.RaiseEmpty();
                                Microsoft.Xna.Framework.Color color = !Game1.currentLocation.Name.StartsWith("UndergroundMine") || !(Game1.currentLocation is MineShaft) ? (Game1.ambientLight.Equals(Microsoft.Xna.Framework.Color.White) || Game1.isRaining && (bool)(NetFieldBase<bool, NetBool>)Game1.currentLocation.isOutdoors ? Game1.outdoorLight : Game1.ambientLight) : (Game1.currentLocation as MineShaft).getLightingColor(gameTime);
                                Game1.spriteBatch.Draw(Game1.staminaRect, Game1.lightmap.Bounds, color);
                                foreach (LightSource currentLightSource in Game1.currentLightSources)
                                {
                                    if (!Game1.isRaining && !Game1.isDarkOut() || currentLightSource.lightContext.Value != LightSource.LightContext.WindowLight)
                                    {
                                        if (currentLightSource.PlayerID != 0L && currentLightSource.PlayerID != Game1.player.UniqueMultiplayerID)
                                        {
                                            Farmer farmerMaybeOffline = Game1.getFarmerMaybeOffline(currentLightSource.PlayerID);
                                            if (farmerMaybeOffline == null || farmerMaybeOffline.currentLocation != null && farmerMaybeOffline.currentLocation.Name != Game1.currentLocation.Name || (bool)(NetFieldBase<bool, NetBool>)farmerMaybeOffline.hidden)
                                                continue;
                                        }
                                        if (Utility.isOnScreen((Vector2)(NetFieldBase<Vector2, NetVector2>)currentLightSource.position, (int)((double)(float)(NetFieldBase<float, NetFloat>)currentLightSource.radius * 64.0 * 4.0)))
                                            Game1.spriteBatch.Draw(currentLightSource.lightTexture, Game1.GlobalToLocal(Game1.viewport, (Vector2)(NetFieldBase<Vector2, NetVector2>)currentLightSource.position) / (float)(Game1.options.lightingQuality / 2), new Microsoft.Xna.Framework.Rectangle?(currentLightSource.lightTexture.Bounds), (Microsoft.Xna.Framework.Color)(NetFieldBase<Microsoft.Xna.Framework.Color, NetColor>)currentLightSource.color, 0.0f, new Vector2((float)currentLightSource.lightTexture.Bounds.Center.X, (float)currentLightSource.lightTexture.Bounds.Center.Y), (float)(NetFieldBase<float, NetFloat>)currentLightSource.radius / (float)(Game1.options.lightingQuality / 2), SpriteEffects.None, 0.9f);
                                    }
                                }
                                Game1.spriteBatch.End();
                                this.GraphicsDevice.SetRenderTarget(target_screen);
                            }
                            if (Game1.bloomDay && Game1.bloom != null)
                                Game1.bloom.BeginDraw();
                            this.GraphicsDevice.Clear(Game1.bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (++batchOpens == 1)
                                events.Rendering.RaiseEmpty();
                            events.RenderingWorld.RaiseEmpty();
                            if (Game1.background != null)
                                Game1.background.draw(Game1.spriteBatch);
                            Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                            Game1.currentLocation.Map.GetLayer("Back").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
                            Game1.currentLocation.drawWater(Game1.spriteBatch);
                            this._farmerShadows.Clear();
                            if (Game1.currentLocation.currentEvent != null && !Game1.currentLocation.currentEvent.isFestival && Game1.currentLocation.currentEvent.farmerActors.Count > 0)
                            {
                                foreach (Farmer farmerActor in Game1.currentLocation.currentEvent.farmerActors)
                                {
                                    if (farmerActor.IsLocalPlayer && Game1.displayFarmer || !(bool)(NetFieldBase<bool, NetBool>)farmerActor.hidden)
                                        this._farmerShadows.Add(farmerActor);
                                }
                            }
                            else
                            {
                                foreach (Farmer farmer in Game1.currentLocation.farmers)
                                {
                                    if (farmer.IsLocalPlayer && Game1.displayFarmer || !(bool)(NetFieldBase<bool, NetBool>)farmer.hidden)
                                        this._farmerShadows.Add(farmer);
                                }
                            }
                            if (!Game1.currentLocation.shouldHideCharacters())
                            {
                                if (Game1.CurrentEvent == null)
                                {
                                    foreach (NPC character in Game1.currentLocation.characters)
                                    {
                                        if (!(bool)(NetFieldBase<bool, NetBool>)character.swimming && !character.HideShadow && (!character.IsInvisible && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(character.getTileLocation())))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, character.Position + new Vector2((float)(character.Sprite.SpriteWidth * 4) / 2f, (float)(character.GetBoundingBox().Height + (character.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)character.yJumpOffset / 40.0) * (float)(NetFieldBase<float, NetFloat>)character.scale, SpriteEffects.None, Math.Max(0.0f, (float)character.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                else
                                {
                                    foreach (NPC actor in Game1.CurrentEvent.actors)
                                    {
                                        if (!(bool)(NetFieldBase<bool, NetBool>)actor.swimming && !actor.HideShadow && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(actor.getTileLocation()))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, actor.Position + new Vector2((float)(actor.Sprite.SpriteWidth * 4) / 2f, (float)(actor.GetBoundingBox().Height + (actor.IsMonster ? 0 : (actor.Sprite.SpriteHeight <= 16 ? -4 : 12))))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)actor.yJumpOffset / 40.0) * (float)(NetFieldBase<float, NetFloat>)actor.scale, SpriteEffects.None, Math.Max(0.0f, (float)actor.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                foreach (Farmer farmerShadow in this._farmerShadows)
                                {
                                    if (!Game1.multiplayer.isDisconnecting(farmerShadow.UniqueMultiplayerID) && !(bool)(NetFieldBase<bool, NetBool>)farmerShadow.swimming && !farmerShadow.isRidingHorse() && (Game1.currentLocation == null || !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(farmerShadow.getTileLocation())))
                                    {
                                        SpriteBatch spriteBatch = Game1.spriteBatch;
                                        Texture2D shadowTexture = Game1.shadowTexture;
                                        Vector2 local = Game1.GlobalToLocal(farmerShadow.Position + new Vector2(32f, 24f));
                                        Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds);
                                        Microsoft.Xna.Framework.Color white = Microsoft.Xna.Framework.Color.White;
                                        Microsoft.Xna.Framework.Rectangle bounds = Game1.shadowTexture.Bounds;
                                        double x = (double)bounds.Center.X;
                                        bounds = Game1.shadowTexture.Bounds;
                                        double y = (double)bounds.Center.Y;
                                        Vector2 origin = new Vector2((float)x, (float)y);
                                        double num = 4.0 - (!farmerShadow.running && !farmerShadow.UsingTool || farmerShadow.FarmerSprite.currentAnimationIndex <= 1 ? 0.0 : (double)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[farmerShadow.FarmerSprite.CurrentFrame]) * 0.5);
                                        spriteBatch.Draw(shadowTexture, local, sourceRectangle, white, 0.0f, origin, (float)num, SpriteEffects.None, 0.0f);
                                    }
                                }
                            }
                            Layer layer1 = Game1.currentLocation.Map.GetLayer("Buildings");
                            layer1.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
                            Game1.mapDisplayDevice.EndScene();
                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (!Game1.currentLocation.shouldHideCharacters())
                            {
                                if (Game1.CurrentEvent == null)
                                {
                                    foreach (NPC character in Game1.currentLocation.characters)
                                    {
                                        if (!(bool)(NetFieldBase<bool, NetBool>)character.swimming && !character.HideShadow && (!(bool)(NetFieldBase<bool, NetBool>)character.isInvisible && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(character.getTileLocation())))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, character.Position + new Vector2((float)(character.Sprite.SpriteWidth * 4) / 2f, (float)(character.GetBoundingBox().Height + (character.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)character.yJumpOffset / 40.0) * (float)(NetFieldBase<float, NetFloat>)character.scale, SpriteEffects.None, Math.Max(0.0f, (float)character.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                else
                                {
                                    foreach (NPC actor in Game1.CurrentEvent.actors)
                                    {
                                        if (!(bool)(NetFieldBase<bool, NetBool>)actor.swimming && !actor.HideShadow && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(actor.getTileLocation()))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, actor.Position + new Vector2((float)(actor.Sprite.SpriteWidth * 4) / 2f, (float)(actor.GetBoundingBox().Height + (actor.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)actor.yJumpOffset / 40.0) * (float)(NetFieldBase<float, NetFloat>)actor.scale, SpriteEffects.None, Math.Max(0.0f, (float)actor.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                foreach (Farmer farmerShadow in this._farmerShadows)
                                {
                                    float num1 = Math.Max(0.0001f, farmerShadow.getDrawLayer() + 0.00011f) - 0.0001f;
                                    if (!(bool)(NetFieldBase<bool, NetBool>)farmerShadow.swimming && !farmerShadow.isRidingHorse() && (Game1.currentLocation != null && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(farmerShadow.getTileLocation())))
                                    {
                                        SpriteBatch spriteBatch = Game1.spriteBatch;
                                        Texture2D shadowTexture = Game1.shadowTexture;
                                        Vector2 local = Game1.GlobalToLocal(farmerShadow.Position + new Vector2(32f, 24f));
                                        Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds);
                                        Microsoft.Xna.Framework.Color white = Microsoft.Xna.Framework.Color.White;
                                        Microsoft.Xna.Framework.Rectangle bounds = Game1.shadowTexture.Bounds;
                                        double x = (double)bounds.Center.X;
                                        bounds = Game1.shadowTexture.Bounds;
                                        double y = (double)bounds.Center.Y;
                                        Vector2 origin = new Vector2((float)x, (float)y);
                                        double num2 = 4.0 - (!farmerShadow.running && !farmerShadow.UsingTool || farmerShadow.FarmerSprite.currentAnimationIndex <= 1 ? 0.0 : (double)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[farmerShadow.FarmerSprite.CurrentFrame]) * 0.5);
                                        double num3 = (double)num1;
                                        spriteBatch.Draw(shadowTexture, local, sourceRectangle, white, 0.0f, origin, (float)num2, SpriteEffects.None, (float)num3);
                                    }
                                }
                            }
                            if ((Game1.eventUp || Game1.killScreen) && (!Game1.killScreen && Game1.currentLocation.currentEvent != null))
                                Game1.currentLocation.currentEvent.draw(Game1.spriteBatch);
                            if (Game1.player.currentUpgrade != null && Game1.player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && Game1.currentLocation.Name.Equals("Farm"))
                                Game1.spriteBatch.Draw(Game1.player.currentUpgrade.workerTexture, Game1.GlobalToLocal(Game1.viewport, Game1.player.currentUpgrade.positionOfCarpenter), new Microsoft.Xna.Framework.Rectangle?(Game1.player.currentUpgrade.getSourceRectangle()), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, (float)(((double)Game1.player.currentUpgrade.positionOfCarpenter.Y + 48.0) / 10000.0));
                            Game1.currentLocation.draw(Game1.spriteBatch);
                            foreach (Vector2 key in Game1.crabPotOverlayTiles.Keys)
                            {
                                Tile tile = layer1.Tiles[(int)key.X, (int)key.Y];
                                if (tile != null)
                                {
                                    Vector2 local = Game1.GlobalToLocal(Game1.viewport, key * 64f);
                                    Location location = new Location((int)local.X, (int)local.Y);
                                    Game1.mapDisplayDevice.DrawTile(tile, location, (float)(((double)key.Y * 64.0 - 1.0) / 10000.0));
                                }
                            }
                            if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                            {
                                string messageToScreen = Game1.currentLocation.currentEvent.messageToScreen;
                            }
                            if (Game1.player.ActiveObject == null && (Game1.player.UsingTool || Game1.pickingTool) && (Game1.player.CurrentTool != null && (!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool)))
                                Game1.drawTool(Game1.player);
                            if (Game1.currentLocation.Name.Equals("Farm"))
                                this.drawFarmBuildings();
                            if (Game1.tvStation >= 0)
                                Game1.spriteBatch.Draw(Game1.tvStationTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(400f, 160f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(Game1.tvStation * 24, 0, 24, 15)), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
                            if (Game1.panMode)
                            {
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(Game1.getOldMouseX() + Game1.viewport.X) / 64.0) * 64 - Game1.viewport.X, (int)Math.Floor((double)(Game1.getOldMouseY() + Game1.viewport.Y) / 64.0) * 64 - Game1.viewport.Y, 64, 64), Microsoft.Xna.Framework.Color.Lime * 0.75f);
                                foreach (Warp warp in (NetList<Warp, NetRef<Warp>>)Game1.currentLocation.warps)
                                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(warp.X * 64 - Game1.viewport.X, warp.Y * 64 - Game1.viewport.Y, 64, 64), Microsoft.Xna.Framework.Color.Red * 0.75f);
                            }
                            Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                            Game1.currentLocation.Map.GetLayer("Front").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
                            Game1.mapDisplayDevice.EndScene();
                            Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.displayFarmer && Game1.player.ActiveObject != null && ((bool)(NetFieldBase<bool, NetBool>)Game1.player.ActiveObject.bigCraftable && this.checkBigCraftableBoundariesForFrontLayer()) && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)
                                Game1.drawPlayerHeldObject(Game1.player);
                            else if (Game1.displayFarmer && Game1.player.ActiveObject != null)
                            {
                                if (Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y - 38), Game1.viewport.Size) == null || Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y - 38), Game1.viewport.Size).TileIndexProperties.ContainsKey("FrontAlways"))
                                {
                                    Layer layer2 = Game1.currentLocation.Map.GetLayer("Front");
                                    rectangle = Game1.player.GetBoundingBox();
                                    Location mapDisplayLocation1 = new Location(rectangle.Right, (int)Game1.player.Position.Y - 38);
                                    xTile.Dimensions.Size size1 = Game1.viewport.Size;
                                    if (layer2.PickTile(mapDisplayLocation1, size1) != null)
                                    {
                                        Layer layer3 = Game1.currentLocation.Map.GetLayer("Front");
                                        rectangle = Game1.player.GetBoundingBox();
                                        Location mapDisplayLocation2 = new Location(rectangle.Right, (int)Game1.player.Position.Y - 38);
                                        xTile.Dimensions.Size size2 = Game1.viewport.Size;
                                        if (layer3.PickTile(mapDisplayLocation2, size2).TileIndexProperties.ContainsKey("FrontAlways"))
                                            goto label_139;
                                    }
                                    else
                                        goto label_139;
                                }
                                Game1.drawPlayerHeldObject(Game1.player);
                            }
                        label_139:
                            if ((Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && ((!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool) && (Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), (int)Game1.player.Position.Y - 38), Game1.viewport.Size) != null && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)))
                                Game1.drawTool(Game1.player);
                            if (Game1.currentLocation.Map.GetLayer("AlwaysFront") != null)
                            {
                                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                                Game1.currentLocation.Map.GetLayer("AlwaysFront").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
                                Game1.mapDisplayDevice.EndScene();
                            }
                            if ((double)Game1.toolHold > 400.0 && Game1.player.CurrentTool.UpgradeLevel >= 1 && Game1.player.canReleaseTool)
                            {
                                Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.White;
                                switch ((int)((double)Game1.toolHold / 600.0) + 2)
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
                                Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - 2, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.player.CurrentTool.Name.Equals("Watering Can") ? 0 : 64) - 2, (int)((double)Game1.toolHold % 600.0 * 0.0799999982118607) + 4, 12), Microsoft.Xna.Framework.Color.Black);
                                Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.player.CurrentTool.Name.Equals("Watering Can") ? 0 : 64), (int)((double)Game1.toolHold % 600.0 * 0.0799999982118607), 8), color);
                            }
                            this.drawWeather(gameTime, target_screen);
                            if (Game1.farmEvent != null)
                                Game1.farmEvent.draw(Game1.spriteBatch);
                            if ((double)Game1.currentLocation.LightLevel > 0.0 && Game1.timeOfDay < 2000)
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Microsoft.Xna.Framework.Color.Black * Game1.currentLocation.LightLevel);
                            if (Game1.screenGlow)
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
                            Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);
                            if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod && ((Game1.player.CurrentTool as FishingRod).isTimingCast || (double)(Game1.player.CurrentTool as FishingRod).castingChosenCountdown > 0.0 || ((Game1.player.CurrentTool as FishingRod).fishCaught || (Game1.player.CurrentTool as FishingRod).showingTreasure)))
                                Game1.player.CurrentTool.draw(Game1.spriteBatch);
                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                            {
                                foreach (NPC actor in Game1.currentLocation.currentEvent.actors)
                                {
                                    if (actor.isEmoting)
                                    {
                                        Vector2 localPosition = actor.getLocalPosition(Game1.viewport);
                                        localPosition.Y -= 140f;
                                        if (actor.Age == 2)
                                            localPosition.Y += 32f;
                                        else if (actor.Gender == 1)
                                            localPosition.Y += 10f;
                                        Game1.spriteBatch.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(actor.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, actor.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)actor.getStandingY() / 10000f);
                                    }
                                }
                            }
                            Game1.spriteBatch.End();
                            if (Game1.drawLighting)
                            {
                                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, this.lightingBlend, SamplerState.LinearClamp, (DepthStencilState)null, (RasterizerState)null);
                                Game1.spriteBatch.Draw((Texture2D)Game1.lightmap, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(Game1.lightmap.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, (float)(Game1.options.lightingQuality / 2), SpriteEffects.None, 1f);
                                if (Game1.isRaining && (bool)(NetFieldBase<bool, NetBool>)Game1.currentLocation.isOutdoors && !(Game1.currentLocation is Desert))
                                    Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Microsoft.Xna.Framework.Color.OrangeRed * 0.45f);
                                Game1.spriteBatch.End();
                            }
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            events.RenderedWorld.RaiseEmpty();
                            if (Game1.drawGrid)
                            {
                                int num1 = -Game1.viewport.X % 64;
                                float num2 = (float)(-Game1.viewport.Y % 64);
                                int num3 = num1;
                                while (true)
                                {
                                    int num4 = num3;
                                    viewport = Game1.graphics.GraphicsDevice.Viewport;
                                    int width = viewport.Width;
                                    if (num4 < width)
                                    {
                                        SpriteBatch spriteBatch = Game1.spriteBatch;
                                        Texture2D staminaRect = Game1.staminaRect;
                                        int x = num3;
                                        int y = (int)num2;
                                        viewport = Game1.graphics.GraphicsDevice.Viewport;
                                        int height = viewport.Height;
                                        Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle(x, y, 1, height);
                                        Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Red * 0.5f;
                                        spriteBatch.Draw(staminaRect, destinationRectangle, color);
                                        num3 += 64;
                                    }
                                    else
                                        break;
                                }
                                float num5 = num2;
                                while (true)
                                {
                                    double num4 = (double)num5;
                                    viewport = Game1.graphics.GraphicsDevice.Viewport;
                                    double height = (double)viewport.Height;
                                    if (num4 < height)
                                    {
                                        SpriteBatch spriteBatch = Game1.spriteBatch;
                                        Texture2D staminaRect = Game1.staminaRect;
                                        int x = num1;
                                        int y = (int)num5;
                                        viewport = Game1.graphics.GraphicsDevice.Viewport;
                                        int width = viewport.Width;
                                        Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle(x, y, width, 1);
                                        Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Red * 0.5f;
                                        spriteBatch.Draw(staminaRect, destinationRectangle, color);
                                        num5 += 64f;
                                    }
                                    else
                                        break;
                                }
                            }
                            if (Game1.currentBillboard != 0 && !this.takingMapScreenshot)
                                this.drawBillboard();
                            if (!Game1.eventUp && Game1.farmEvent == null && (Game1.currentBillboard == 0 && Game1.gameMode == (byte)3) && (!this.takingMapScreenshot && Game1.isOutdoorMapSmallerThanViewport()))
                            {
                                SpriteBatch spriteBatch1 = Game1.spriteBatch;
                                Texture2D fadeToBlackRect1 = Game1.fadeToBlackRect;
                                int width1 = -Math.Min(Game1.viewport.X, 4096);
                                viewport = Game1.graphics.GraphicsDevice.Viewport;
                                int height1 = viewport.Height;
                                Microsoft.Xna.Framework.Rectangle destinationRectangle1 = new Microsoft.Xna.Framework.Rectangle(0, 0, width1, height1);
                                Microsoft.Xna.Framework.Color black1 = Microsoft.Xna.Framework.Color.Black;
                                spriteBatch1.Draw(fadeToBlackRect1, destinationRectangle1, black1);
                                SpriteBatch spriteBatch2 = Game1.spriteBatch;
                                Texture2D fadeToBlackRect2 = Game1.fadeToBlackRect;
                                int x = -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64;
                                viewport = Game1.graphics.GraphicsDevice.Viewport;
                                int width2 = Math.Min(4096, viewport.Width - (-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64));
                                viewport = Game1.graphics.GraphicsDevice.Viewport;
                                int height2 = viewport.Height;
                                Microsoft.Xna.Framework.Rectangle destinationRectangle2 = new Microsoft.Xna.Framework.Rectangle(x, 0, width2, height2);
                                Microsoft.Xna.Framework.Color black2 = Microsoft.Xna.Framework.Color.Black;
                                spriteBatch2.Draw(fadeToBlackRect2, destinationRectangle2, black2);
                            }
                            if ((Game1.displayHUD || Game1.eventUp) && (Game1.currentBillboard == 0 && Game1.gameMode == (byte)3) && (!Game1.freezeControls && !Game1.panMode && (!Game1.HostPaused && !this.takingMapScreenshot)))
                            {
                                events.RenderingHud.RaiseEmpty();
                                this.drawHUD();
                                events.RenderedHud.RaiseEmpty();
                            }
                            else if (Game1.activeClickableMenu == null)
                            {
                                FarmEvent farmEvent = Game1.farmEvent;
                            }
                            if (Game1.hudMessages.Count > 0 && !this.takingMapScreenshot)
                            {
                                for (int i = Game1.hudMessages.Count - 1; i >= 0; --i)
                                    Game1.hudMessages[i].draw(Game1.spriteBatch, i);
                            }
                        }
                        if (Game1.farmEvent != null)
                            Game1.farmEvent.draw(Game1.spriteBatch);
                        if (Game1.dialogueUp && !Game1.nameSelectUp && !Game1.messagePause && ((Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox)) && !this.takingMapScreenshot))
                            this.drawDialogueBox();
                        if (Game1.progressBar && !this.takingMapScreenshot)
                        {
                            SpriteBatch spriteBatch1 = Game1.spriteBatch;
                            Texture2D fadeToBlackRect = Game1.fadeToBlackRect;
                            int x1 = (Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2;
                            rectangle = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
                            int y1 = rectangle.Bottom - 128;
                            int dialogueWidth = Game1.dialogueWidth;
                            Microsoft.Xna.Framework.Rectangle destinationRectangle1 = new Microsoft.Xna.Framework.Rectangle(x1, y1, dialogueWidth, 32);
                            Microsoft.Xna.Framework.Color lightGray = Microsoft.Xna.Framework.Color.LightGray;
                            spriteBatch1.Draw(fadeToBlackRect, destinationRectangle1, lightGray);
                            SpriteBatch spriteBatch2 = Game1.spriteBatch;
                            Texture2D staminaRect = Game1.staminaRect;
                            int x2 = (Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2;
                            rectangle = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
                            int y2 = rectangle.Bottom - 128;
                            int width = (int)((double)Game1.pauseAccumulator / (double)Game1.pauseTime * (double)Game1.dialogueWidth);
                            Microsoft.Xna.Framework.Rectangle destinationRectangle2 = new Microsoft.Xna.Framework.Rectangle(x2, y2, width, 32);
                            Microsoft.Xna.Framework.Color dimGray = Microsoft.Xna.Framework.Color.DimGray;
                            spriteBatch2.Draw(staminaRect, destinationRectangle2, dimGray);
                        }
                        if (Game1.eventUp && Game1.currentLocation != null && Game1.currentLocation.currentEvent != null)
                            Game1.currentLocation.currentEvent.drawAfterMap(Game1.spriteBatch);
                        if (Game1.isRaining && Game1.currentLocation != null && ((bool)(NetFieldBase<bool, NetBool>)Game1.currentLocation.isOutdoors && !(Game1.currentLocation is Desert)))
                        {
                            SpriteBatch spriteBatch = Game1.spriteBatch;
                            Texture2D staminaRect = Game1.staminaRect;
                            viewport = Game1.graphics.GraphicsDevice.Viewport;
                            Microsoft.Xna.Framework.Rectangle bounds = viewport.Bounds;
                            Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Blue * 0.2f;
                            spriteBatch.Draw(staminaRect, bounds, color);
                        }
                        if ((Game1.fadeToBlack || Game1.globalFade) && !Game1.menuUp && ((!Game1.nameSelectUp || Game1.messagePause) && !this.takingMapScreenshot))
                        {
                            SpriteBatch spriteBatch = Game1.spriteBatch;
                            Texture2D fadeToBlackRect = Game1.fadeToBlackRect;
                            viewport = Game1.graphics.GraphicsDevice.Viewport;
                            Microsoft.Xna.Framework.Rectangle bounds = viewport.Bounds;
                            Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Black * (Game1.gameMode == (byte)0 ? 1f - Game1.fadeToBlackAlpha : Game1.fadeToBlackAlpha);
                            spriteBatch.Draw(fadeToBlackRect, bounds, color);
                        }
                        else if ((double)Game1.flashAlpha > 0.0 && !this.takingMapScreenshot)
                        {
                            if (Game1.options.screenFlash)
                            {
                                SpriteBatch spriteBatch = Game1.spriteBatch;
                                Texture2D fadeToBlackRect = Game1.fadeToBlackRect;
                                viewport = Game1.graphics.GraphicsDevice.Viewport;
                                Microsoft.Xna.Framework.Rectangle bounds = viewport.Bounds;
                                Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.White * Math.Min(1f, Game1.flashAlpha);
                                spriteBatch.Draw(fadeToBlackRect, bounds, color);
                            }
                            Game1.flashAlpha -= 0.1f;
                        }
                        if ((Game1.messagePause || Game1.globalFade) && (Game1.dialogueUp && !this.takingMapScreenshot))
                            this.drawDialogueBox();
                        if (!this.takingMapScreenshot)
                        {
                            foreach (TemporaryAnimatedSprite overlayTempSprite in Game1.screenOverlayTempSprites)
                                overlayTempSprite.draw(Game1.spriteBatch, true, 0, 0, 1f);
                        }
                        if (Game1.debugMode)
                        {
                            StringBuilder debugStringBuilder = Game1._debugStringBuilder;
                            debugStringBuilder.Clear();
                            if (Game1.panMode)
                            {
                                debugStringBuilder.Append((Game1.getOldMouseX() + Game1.viewport.X) / 64);
                                debugStringBuilder.Append(",");
                                debugStringBuilder.Append((Game1.getOldMouseY() + Game1.viewport.Y) / 64);
                            }
                            else
                            {
                                debugStringBuilder.Append("player: ");
                                debugStringBuilder.Append(Game1.player.getStandingX() / 64);
                                debugStringBuilder.Append(", ");
                                debugStringBuilder.Append(Game1.player.getStandingY() / 64);
                            }
                            debugStringBuilder.Append(" mouseTransparency: ");
                            debugStringBuilder.Append(Game1.mouseCursorTransparency);
                            debugStringBuilder.Append(" mousePosition: ");
                            debugStringBuilder.Append(Game1.getMouseX());
                            debugStringBuilder.Append(",");
                            debugStringBuilder.Append(Game1.getMouseY());
                            debugStringBuilder.Append(Environment.NewLine);
                            debugStringBuilder.Append(" mouseWorldPosition: ");
                            debugStringBuilder.Append(Game1.getMouseX() + Game1.viewport.X);
                            debugStringBuilder.Append(",");
                            debugStringBuilder.Append(Game1.getMouseY() + Game1.viewport.Y);
                            debugStringBuilder.Append("  debugOutput: ");
                            debugStringBuilder.Append(Game1.debugOutput);
                            Game1.spriteBatch.DrawString(Game1.smallFont, debugStringBuilder, new Vector2((float)this.GraphicsDevice.Viewport.GetTitleSafeArea().X, (float)(this.GraphicsDevice.Viewport.GetTitleSafeArea().Y + Game1.smallFont.LineSpacing * 8)), Microsoft.Xna.Framework.Color.Red, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
                        }
                        if (Game1.showKeyHelp && !this.takingMapScreenshot)
                            Game1.spriteBatch.DrawString(Game1.smallFont, Game1.keyHelpString, new Vector2(64f, (float)(Game1.viewport.Height - 64 - (Game1.dialogueUp ? 192 + (Game1.isQuestion ? Game1.questionChoices.Count * 64 : 0) : 0)) - Game1.smallFont.MeasureString(Game1.keyHelpString).Y), Microsoft.Xna.Framework.Color.LightGray, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
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
                            Game1.farmEvent.drawAboveEverything(Game1.spriteBatch);
                        if (Game1.emoteMenu != null && !this.takingMapScreenshot)
                            Game1.emoteMenu.draw(Game1.spriteBatch);
                        if (Game1.HostPaused && !this.takingMapScreenshot)
                        {
                            string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
                            SpriteText.drawStringWithScrollBackground(Game1.spriteBatch, s, 96, 32, "", 1f, -1, SpriteText.ScrollTextAlignment.Left);
                        }

                        events.Rendered.RaiseEmpty();
                        Game1.spriteBatch.End();
                        this.drawOverlays(Game1.spriteBatch);
                        this.renderScreenBuffer(target_screen);
                    }
                }
            }
        }
    }
}
