using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

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

        public MouseState MStateNow { get; private set; }
        public MouseState MStatePrior { get; private set; }

        public Keys[] CurrentlyPressedKeys { get; private set; }
        public Keys[] PreviouslyPressedKeys { get; private set; }

        public Keys[] FramePressedKeys 
        { 
            get { return CurrentlyPressedKeys.Where(x => !PreviouslyPressedKeys.Contains(x)).ToArray(); }
        }

        public int PreviousGameLocations { get; private set; }
        public GameLocation PreviousGameLocation { get; private set; }
        public IClickableMenu PreviousActiveMenu { get; private set; }

        public Int32 PreviousTimeOfDay { get; private set; }
        public Int32 PreviousDayOfMonth { get; private set; }
        public String PreviousSeasonOfYear { get; private set; }
        public Int32 PreviousYearOfGame { get; private set; }

        public Farmer PreviousFarmer { get; private set; }

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
            UpdateEventCalls();

            try
            {
                base.Update(gameTime);
            }
            catch (Exception ex)
            {
                Program.LogError("An error occured in the base update loop: " + ex);
                Console.ReadKey();
            }

            Events.InvokeUpdateTick();

            PreviouslyPressedKeys = CurrentlyPressedKeys;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Events.InvokeDrawTick();

            if (false)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                if (CurrentLocation != null)
                    CurrentLocation.draw(spriteBatch);

                if (player != null && player.position != null)
                    spriteBatch.DrawString(dialogueFont, player.position.ToString(), new Vector2(0, 180), Color.Orange);

                spriteBatch.End();
            }
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
                Program.LogError("ModItem Dictionary does not contain index: " + id);
                return null;
            }
            if (ModItems.ContainsKey(id))
            {
                return ModItems[id].Clone();
            }
            Program.LogError("ModItem Dictionary does not contain ID: " + id);
            return null;
        }

        public static SGameLocation GetLocationFromName(String name)
        {
            if (ModLocations.Any(x => x.name == name))
            {
                return ModLocations[ModLocations.IndexOf(ModLocations.First(x => x.name == name))];
            }
            return null;
        }

        public static SGameLocation LoadOrCreateSGameLocationFromName(String name)
        {
            if (GetLocationFromName(name) != null)
                return GetLocationFromName(name);
            GameLocation gl = locations.FirstOrDefault(x => x.name == name);
            if (gl != null)
            {
                Program.LogDebug("A custom location was created for the new name: " + name);
                SGameLocation s = SGameLocation.ConstructFromBaseClass(gl);
                ModLocations.Add(s);
                return s;
            }
            if (currentLocation != null && currentLocation.name == name)
            {
                gl = currentLocation;
                Program.LogDebug("A custom location was created from the current location for the new name: " + name);
                SGameLocation s = SGameLocation.ConstructFromBaseClass(gl);
                ModLocations.Add(s);
                return s;
            }

            Program.LogDebug("A custom location could not be created for: " + name);
            return null;
        }


        public void UpdateEventCalls()
        {
            KStateNow = Keyboard.GetState();
            CurrentlyPressedKeys = KStateNow.GetPressedKeys();
            MStateNow = Mouse.GetState();

            foreach (Keys k in FramePressedKeys)
                Events.InvokeKeyPressed(k);

            if (KStateNow != KStatePrior)
            {
                Events.InvokeKeyboardChanged(KStateNow);
                KStatePrior = KStateNow;
            }

            if (MStateNow != MStatePrior)
            {
                Events.InvokeMouseChanged(MStateNow);
                MStatePrior = MStateNow;
            }

            if (activeClickableMenu != null && activeClickableMenu != PreviousActiveMenu)
            {
                Events.InvokeMenuChanged(activeClickableMenu);
                PreviousActiveMenu = activeClickableMenu;
            }

            if (locations.GetHash() != PreviousGameLocations)
            {
                Events.InvokeLocationsChanged(locations);
                PreviousGameLocations = locations.GetHash();
            }

            if (currentLocation != PreviousGameLocation)
            {
                Events.InvokeCurrentLocationChanged(currentLocation);
                PreviousGameLocation = currentLocation;
            }

            if (player != null && player != PreviousFarmer)
            {
                Events.InvokeFarmerChanged(player);
                PreviousFarmer = player;
            }

            if (timeOfDay != PreviousTimeOfDay)
            {
                Events.InvokeTimeOfDayChanged(timeOfDay);
                PreviousTimeOfDay = timeOfDay;
            }

            if (dayOfMonth != PreviousDayOfMonth)
            {
                Events.InvokeDayOfMonthChanged(dayOfMonth);
                PreviousDayOfMonth = dayOfMonth;
            }

            if (currentSeason != PreviousSeasonOfYear)
            {
                Events.InvokeSeasonOfYearChanged(currentSeason);
                PreviousSeasonOfYear = currentSeason;
            }

            if (year != PreviousYearOfGame)
            {
                Events.InvokeYearOfGameChanged(year);
                PreviousYearOfGame = year;
            }
        }
    }
}