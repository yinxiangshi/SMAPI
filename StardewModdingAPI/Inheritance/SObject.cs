using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;

namespace StardewModdingAPI.Inheritance
{
    public class SObject : StardewValley.Object
    {
        public override String Name { get; set; }
        public String Description { get; set; }
        public Texture2D Texture { get; set; }
        public String CategoryName { get; set; }
        public Color CategoryColour { get; set; }
        public Boolean IsPassable { get; set; }
        public Boolean IsPlaceable { get; set; }
        public Boolean HasBeenRegistered { get; set; }
        public Int32 RegisteredId { get; set; }

        public Int32 MaxStackSize { get; set; }

        public Boolean WallMounted { get; set; }
        public Vector2 DrawPosition { get; set; }

        public Boolean FlaggedForPickup { get; set; }

        public SObject()
        {
            Name = "Modded Item Name";
            Description = "Modded Item Description";
            CategoryName = "Modded Item Category";
            Category = 4163;
            CategoryColour = Color.White;
            IsPassable = false;
            IsPlaceable = false;
            boundingBox = new Rectangle(0, 0, 64, 64);
        }

        public override string getDescription()
        {
            return Description;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (Texture != null)
                {
                    int targSize = Game1.tileSize;
                    int midX = (int) ((x) + 32);
                    int midY = (int) ((y) + 32);

                    int targX = midX - targSize / 2;
                    int targY = midY - targSize / 2;

                    Rectangle targ = new Rectangle(targX, targY, targSize, targSize);
                    spriteBatch.Draw(Texture, targ, null, new Color(255, 255, 255, 255f * alpha), 0, Vector2.Zero, SpriteEffects.None, 0.999f);
                }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            Program.LogInfo("DRAW ME2");
            return;
            try
            {
                if (Texture != null)
                {
                    int targSize = Game1.tileSize;
                    int midX = (int) ((xNonTile) + 32);
                    int midY = (int) ((yNonTile) + 32);

                    int targX = midX - targSize / 2;
                    int targY = midY - targSize / 2;

                    Rectangle targ = new Rectangle(targX, targY, targSize, targSize);
                    spriteBatch.Draw(Texture, targ, null, new Color(255, 255, 255, 255f * alpha), 0, Vector2.Zero, SpriteEffects.None, layerDepth);
                    //spriteBatch.Draw(Program.DebugPixel, targ, null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, layerDepth);
                    /*
                    spriteBatch.DrawString(Game1.dialogueFont, "TARG: " + targ, new Vector2(128, 0), Color.Red);
                    spriteBatch.DrawString(Game1.dialogueFont, ".", new Vector2(targX * 0.5f, targY), Color.Orange);
                    spriteBatch.DrawString(Game1.dialogueFont, ".", new Vector2(targX, targY), Color.Red);
                    spriteBatch.DrawString(Game1.dialogueFont, ".", new Vector2(targX * 1.5f, targY), Color.Yellow);
                    spriteBatch.DrawString(Game1.dialogueFont, ".", new Vector2(targX * 2f, targY), Color.Green);
                    */
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex.ToString());
                Console.ReadKey();
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            if (this.isRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }

            if (Texture != null)
            {
                int targSize = (int) (64 * scaleSize * 0.9f);
                int midX = (int) ((location.X) + 32);
                int midY = (int) ((location.Y) + 32);

                int targX = midX - targSize / 2;
                int targY = midY - targSize / 2;

                spriteBatch.Draw(Texture, new Rectangle(targX, targY, targSize, targSize), null, new Color(255, 255, 255, transparency), 0, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
            if (drawStackNumber)
            {
                float scale = 0.5f + scaleSize;
                Game1.drawWithBorder(string.Concat(this.stack), Color.Black, Color.White, location + new Vector2((float) Game1.tileSize - Game1.tinyFont.MeasureString(string.Concat(this.stack)).X * scale, (float) Game1.tileSize - (float) ((double) Game1.tinyFont.MeasureString(string.Concat(this.stack)).Y * 3.0f / 4.0f) * scale), 0.0f, scale, 1f, true);
            }
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (Texture != null)
            {
                int targSize = 64;
                int midX = (int) ((objectPosition.X) + 32);
                int midY = (int) ((objectPosition.Y) + 32);

                int targX = midX - targSize / 2;
                int targY = midY - targSize / 2;

                spriteBatch.Draw(Texture, new Rectangle(targX, targY, targSize, targSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, (f.getStandingY() + 2) / 10000f);
            }
        }

        public override Color getCategoryColor()
        {
            return CategoryColour;
        }

        public override string getCategoryName()
        {
            if (string.IsNullOrEmpty(CategoryName))
                return "Modded Item";
            return CategoryName;
        }

        public override bool isPassable()
        {
            return IsPassable;
        }

        public override bool isPlaceable()
        {
            return IsPlaceable;
        }

        public override int maximumStackSize()
        {
            return MaxStackSize;
        }

        public SObject Clone()
        {
            SObject toRet = new SObject();

            toRet.Name = this.Name;
            toRet.CategoryName = this.CategoryName;
            toRet.Description = this.Description;
            toRet.Texture = this.Texture;
            toRet.IsPassable = this.IsPassable;
            toRet.IsPlaceable = this.IsPlaceable;
            toRet.quality = this.quality;
            toRet.scale = this.scale;
            toRet.isSpawnedObject = this.isSpawnedObject;
            toRet.isRecipe = this.isRecipe;
            toRet.questItem = this.questItem;
            toRet.stack = 1;
            toRet.HasBeenRegistered = this.HasBeenRegistered;
            toRet.RegisteredId = this.RegisteredId;

            return toRet;
        }

        public override Item getOne()
        {
            return this.Clone();
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 key = new Vector2(x, y);
            if (!location.objects.ContainsKey(key))
                location.objects.Add(key, this);
            return false;

            SGameLocation s = SGame.GetLocationFromName(location.name);

            if (s.GetHashCode() != SGame.CurrentLocation.GetHashCode())
            {
                Program.LogError("HASH DIFFERENCE: " + s.GetHashCode() + " | " + SGame.ModLocations[SGame.ModLocations.IndexOf(SGame.ModLocations.First(z => z.name == location.name))].GetHashCode() + " | " + SGame.CurrentLocation.GetHashCode());
                Console.ReadKey();
            }

            Console.Title = (this.GetHashCode() + " PLACEMENT");

            if (s != null)
            {
                Vector2 index1 = new Vector2(x - (Game1.tileSize / 2), y - (Game1.tileSize / 2));
                if (!s.ModObjects.ContainsKey(index1))
                {
                    s.ModObjects.Add(index1, this);
                    Game1.player.position = index1;
                    return true;
                }
            }
            else
            {
                Program.LogError("No SGameLocation could be found for the supplied GameLocation!");
                return false;
            }
            return false;
        }
    }
}