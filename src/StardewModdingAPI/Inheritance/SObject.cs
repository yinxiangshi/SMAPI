using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework;
using StardewValley;
using Object = StardewValley.Object;

#pragma warning disable 1591
namespace StardewModdingAPI.Inheritance
{
    /// <summary>Provides access to the game's <see cref="Object"/> internals.</summary>
    [Obsolete("This class is deprecated and will be removed in a future version.")]
    public class SObject : Object
    {
        /*********
        ** Accessors
        *********/
        public string Description { get; set; }
        public Texture2D Texture { get; set; }
        public string CategoryName { get; set; }
        public Color CategoryColour { get; set; }
        public bool IsPassable { get; set; }
        public bool IsPlaceable { get; set; }
        public bool HasBeenRegistered { get; set; }
        public int RegisteredId { get; set; }

        public int MaxStackSize { get; set; }

        public bool WallMounted { get; set; }
        public Vector2 DrawPosition { get; set; }

        public bool FlaggedForPickup { get; set; }

        [XmlIgnore]
        public Vector2 CurrentMouse { get; protected set; }

        [XmlIgnore]
        public Vector2 PlacedAt { get; protected set; }

        public override int Stack
        {
            get { return this.stack; }
            set { this.stack = value; }
        }

        /*********
        ** Public methods
        *********/
        public SObject()
        {
            Program.DeprecationManager.Warn(nameof(SObject), "0.39.3", DeprecationLevel.PendingRemoval);

            this.name = "Modded Item Name";
            this.Description = "Modded Item Description";
            this.CategoryName = "Modded Item Category";
            this.Category = 4163;
            this.CategoryColour = Color.White;
            this.IsPassable = false;
            this.IsPlaceable = false;
            this.boundingBox = new Rectangle(0, 0, 64, 64);
            this.MaxStackSize = 999;

            this.type = "interactive";
        }

        public override string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public override string getDescription()
        {
            return this.Description;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (this.Texture != null)
            {
                spriteBatch.Draw(this.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize + Game1.tileSize / 2 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), y * Game1.tileSize + Game1.tileSize / 2 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0))), Game1.currentLocation.getSourceRectForObject(this.ParentSheetIndex), Color.White * alpha, 0f, new Vector2(8f, 8f), this.scale.Y > 1f ? this.getScale().Y : Game1.pixelZoom, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.isPassable() ? this.getBoundingBox(new Vector2(x, y)).Top : this.getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
            }
        }

        public new void drawAsProp(SpriteBatch b)
        {
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            if (this.isRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }

            if (this.Texture != null)
            {
                var targSize = (int) (64 * scaleSize * 0.9f);
                var midX = (int) (location.X + 32);
                var midY = (int) (location.Y + 32);

                var targX = midX - targSize / 2;
                var targY = midY - targSize / 2;

                spriteBatch.Draw(this.Texture, new Rectangle(targX, targY, targSize, targSize), null, new Color(255, 255, 255, transparency), 0, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
            if (drawStackNumber)
            {
                var _scale = 0.5f + scaleSize;
                Game1.drawWithBorder(this.stack.ToString(), Color.Black, Color.White, location + new Vector2(Game1.tileSize - Game1.tinyFont.MeasureString(string.Concat(this.stack.ToString())).X * _scale, Game1.tileSize - (float) ((double) Game1.tinyFont.MeasureString(string.Concat(this.stack.ToString())).Y * 3.0f / 4.0f) * _scale), 0.0f, _scale, 1f, true);
            }
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (this.Texture != null)
            {
                var targSize = 64;
                var midX = (int) (objectPosition.X + 32);
                var midY = (int) (objectPosition.Y + 32);

                var targX = midX - targSize / 2;
                var targY = midY - targSize / 2;

                spriteBatch.Draw(this.Texture, new Rectangle(targX, targY, targSize, targSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, (f.getStandingY() + 2) / 10000f);
            }
        }

        public override Color getCategoryColor()
        {
            return this.CategoryColour;
        }

        public override string getCategoryName()
        {
            if (string.IsNullOrEmpty(this.CategoryName))
                return "Modded Item";
            return this.CategoryName;
        }

        public override bool isPassable()
        {
            return this.IsPassable;
        }

        public override bool isPlaceable()
        {
            return this.IsPlaceable;
        }

        public override int maximumStackSize()
        {
            return this.MaxStackSize;
        }

        public SObject Clone()
        {
            var toRet = new SObject
            {
                Name = this.Name,
                CategoryName = this.CategoryName,
                Description = this.Description,
                Texture = this.Texture,
                IsPassable = this.IsPassable,
                IsPlaceable = this.IsPlaceable,
                quality = this.quality,
                scale = this.scale,
                isSpawnedObject = this.isSpawnedObject,
                isRecipe = this.isRecipe,
                questItem = this.questItem,
                stack = 1,
                HasBeenRegistered = this.HasBeenRegistered,
                RegisteredId = this.RegisteredId
            };


            return toRet;
        }

        public override Item getOne()
        {
            return this.Clone();
        }

        public override void actionWhenBeingHeld(Farmer who)
        {
            var x = Game1.oldMouseState.X + Game1.viewport.X;
            var y = Game1.oldMouseState.Y + Game1.viewport.Y;

            x = x / Game1.tileSize;
            y = y / Game1.tileSize;

            this.CurrentMouse = new Vector2(x, y);
            //Program.LogDebug(canBePlacedHere(Game1.currentLocation, CurrentMouse));
            base.actionWhenBeingHeld(who);
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            //Program.LogDebug(CurrentMouse.ToString().Replace("{", "").Replace("}", ""));
            if (!l.objects.ContainsKey(tile))
                return true;

            return false;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            if (Game1.didPlayerJustRightClick())
                return false;

            x = x / Game1.tileSize;
            y = y / Game1.tileSize;

            var key = new Vector2(x, y);

            if (!this.canBePlacedHere(location, key))
                return false;

            var s = this.Clone();

            s.PlacedAt = key;
            s.boundingBox = new Rectangle(x / Game1.tileSize * Game1.tileSize, y / Game1.tileSize * Game1.tileSize, this.boundingBox.Width, this.boundingBox.Height);

            location.objects.Add(key, s);

            return true;
        }

        public override void actionOnPlayerEntry()
        {
            //base.actionOnPlayerEntry();
        }

        public override void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
        {
            if (this.canBePlacedHere(location, this.CurrentMouse))
            {
                var targSize = Game1.tileSize;

                var x = Game1.oldMouseState.X + Game1.viewport.X;
                var y = Game1.oldMouseState.Y + Game1.viewport.Y;
                spriteBatch.Draw(Game1.mouseCursors, new Vector2(x / Game1.tileSize * Game1.tileSize - Game1.viewport.X, y / Game1.tileSize * Game1.tileSize - Game1.viewport.Y), new Rectangle(Utility.playerCanPlaceItemHere(location, this, x, y, Game1.player) ? 194 : 210, 388, 16, 16), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.01f);
            }
        }
    }
}