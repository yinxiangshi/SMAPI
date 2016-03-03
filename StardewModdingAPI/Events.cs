using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace StardewModdingAPI
{
    public static class Events
    {
        public static event EventHandler GameLoaded = delegate { };
        public static event EventHandler Initialize = delegate { };
        public static event EventHandler LoadContent = delegate { };
        public static event EventHandler UpdateTick = delegate { };
        public static event EventHandler DrawTick = delegate { };

        public static event EventHandler<EventArgsKeyboardStateChanged> KeyboardChanged = delegate { };
        public static event EventHandler<EventArgsKeyPressed> KeyPressed = delegate { };
        public static event EventHandler<EventArgsMouseStateChanged> MouseChanged = delegate { };
        public static event EventHandler<EventArgsClickableMenuChanged> MenuChanged = delegate { };
        public static event EventHandler<EventArgsGameLocationsChanged> LocationsChanged = delegate { };
        public static event EventHandler<EventArgsCurrentLocationChanged> CurrentLocationChanged = delegate { };
        public static event EventHandler Resize = delegate { };
        public static event EventHandler<EventArgsFarmerChanged> FarmerChanged = delegate { };
        public static event EventHandler<EventArgsIntChanged> TimeOfDayChanged = delegate { };
        public static event EventHandler<EventArgsIntChanged> DayOfMonthChanged = delegate { };
        public static event EventHandler<EventArgsIntChanged> YearOfGameChanged = delegate { };
        public static event EventHandler<EventArgsStringChanged> SeasonOfYearChanged = delegate { };
                
        public static void InvokeGameLoaded()
        {
            GameLoaded.Invoke(null, EventArgs.Empty);
        }

        public static void InvokeInitialize()
        {
            try
            {
                Initialize.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA Initialize: " + ex.ToString());
            }
        }

        public static void InvokeLoadContent()
        {
            try
            {
                LoadContent.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA LoadContent: " + ex.ToString());
            }
        }

        public static void InvokeUpdateTick()
        {
            try
            {
                UpdateTick.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA UpdateTick: " + ex.ToString());
            }
        }

        public static void InvokeDrawTick()
        {
            try
            {
                DrawTick.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA DrawTick: " + ex.ToString());
            }
        }

        public static void InvokeKeyboardChanged(KeyboardState priorState, KeyboardState newState)
        {
            KeyboardChanged.Invoke(null, new EventArgsKeyboardStateChanged(priorState, newState));
        }

        public static void InvokeMouseChanged(MouseState priorState, MouseState newState)
        {
            MouseChanged.Invoke(null, new EventArgsMouseStateChanged(priorState, newState));
        }

        public static void InvokeKeyPressed(Keys key)
        {
            KeyPressed.Invoke(null, new EventArgsKeyPressed(key));
        }

        public static void InvokeMenuChanged(IClickableMenu priorMenu, IClickableMenu newMenu)
        {
            MenuChanged.Invoke(null, new EventArgsClickableMenuChanged(priorMenu, newMenu));
        }

        public static void InvokeLocationsChanged(List<GameLocation> newLocations)
        {
            LocationsChanged.Invoke(null, new EventArgsGameLocationsChanged(newLocations));
        }

        public static void InvokeCurrentLocationChanged(GameLocation priorLocation, GameLocation newLocation)
        {
            CurrentLocationChanged.Invoke(null, new EventArgsCurrentLocationChanged(priorLocation, newLocation));
        }

        public static void InvokeResize(object sender, EventArgs e)
        {
            Resize.Invoke(sender, e);
        }

        public static void InvokeFarmerChanged(Farmer priorFarmer, Farmer newFarmer)
        {
            FarmerChanged.Invoke(null, new EventArgsFarmerChanged(priorFarmer, newFarmer));
        }

        public static void InvokeTimeOfDayChanged(Int32 priorInt, Int32 newInt)
        {
            TimeOfDayChanged.Invoke(null, new EventArgsIntChanged(priorInt, newInt));
        }

        public static void InvokeDayOfMonthChanged(Int32 priorInt, Int32 newInt)
        {
            DayOfMonthChanged.Invoke(null, new EventArgsIntChanged(priorInt, newInt));
        }

        public static void InvokeYearOfGameChanged(Int32 priorInt, Int32 newInt)
        {
            YearOfGameChanged.Invoke(null, new EventArgsIntChanged(priorInt, newInt));
        }

        public static void InvokeSeasonOfYearChanged(String priorString, String newString)
        {
            SeasonOfYearChanged.Invoke(null, new EventArgsStringChanged(priorString, newString));
        }
    }
}
