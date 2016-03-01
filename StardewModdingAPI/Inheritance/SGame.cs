using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace StardewModdingAPI.Inheritance
{
    public class SGame : Game1
    {
        public static List<SGameLocation> ModLocations = new List<SGameLocation>();
        public static SGameLocation CurrentLocation { get; internal set; }
        public static Dictionary<Int32, SObject> ModItems { get; private set; }
        public const Int32 LowestModItemID = 1000;

        public static FieldInfo[] StaticFields { get { return GetStaticFields(); } }

        public static FieldInfo[] GetStaticFields()
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

        public int PreviousGameLocations { get; private set; }
        public GameLocation PreviousGameLocation { get; private set; }
        public IClickableMenu PreviousActiveMenu { get; private set; }

        protected override void Initialize()
        {
            Program.Log("XNA Initialize");
            ModItems = new Dictionary<Int32, SObject>();
            PreviouslyPressedKeys = new Keys[0];
            base.Initialize();
            Events.InvokeInitialize();
        }

        protected override void LoadContent()
        {
            Program.Log("XNA LoadContent");
            base.LoadContent();
            Events.InvokeLoadContent();
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
                KStatePrior = KStateNow;
            }

            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu != PreviousActiveMenu)
            {
                Events.InvokeMenuChanged(Game1.activeClickableMenu);
                PreviousActiveMenu = Game1.activeClickableMenu;
            }

            if (Game1.locations.GetHash() != PreviousGameLocations)
            {
                Events.InvokeLocationsChanged(Game1.locations);
                PreviousGameLocations = Game1.locations.GetHash();
            }

            if (Game1.currentLocation != PreviousGameLocation)
            {
                Events.InvokeCurrentLocationChanged(Game1.currentLocation);
                PreviousGameLocation = Game1.currentLocation;
            }

            if (CurrentLocation != null)
                CurrentLocation.update(gameTime);

            base.Update(gameTime);
            Events.InvokeUpdateTick();

            PreviouslyPressedKeys = CurrentlyPressedKeys;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Events.InvokeDrawTick();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
            if (CurrentLocation != null)
                CurrentLocation.draw(Game1.spriteBatch);

            if (player != null && player.position != null)
                spriteBatch.DrawString(Game1.dialogueFont, Game1.player.position.ToString(), new Vector2(0, 180), Color.Orange);
            
            spriteBatch.End();
        }

        public static Int32 RegisterModItem(SObject modItem)
        {
            if (modItem.HasBeenRegistered)
            {
                Program.LogError("The item {0} has already been registered with ID {1}", modItem.Name, modItem.RegisteredId);
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
                else
                {
                    Program.LogError("ModItem Dictionary does not contain index: " + id);
                    return null;
                }
            }
            else
            {
                if (ModItems.ContainsKey(id))
                {
                    return ModItems[id].Clone();
                }
                else
                {
                    Program.LogError("ModItem Dictionary does not contain ID: " + id);
                    return null;
                }
            }
        }

        public static SGameLocation GetLocationFromName(String name)
        {
            if (ModLocations.Any(x => x.name == name))
            {
                return ModLocations[ModLocations.IndexOf(ModLocations.First(x => x.name == name))];
            }
            return null;
        }
    }
}