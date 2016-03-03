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
        public delegate void BlankEventHandler();

        public static event BlankEventHandler GameLoaded = delegate { };
        public static event BlankEventHandler Initialize = delegate { };
        public static event BlankEventHandler LoadContent = delegate { };
        public static event BlankEventHandler UpdateTick = delegate { };
        public static event BlankEventHandler DrawTick = delegate { };

        public delegate void KStateChanged(KeyboardState newState);
        public static event KStateChanged KeyboardChanged = delegate { };

        public delegate void KeyStateChanged(Keys key);
        public static event KeyStateChanged KeyPressed = delegate { };

        public delegate void MStateChanged(MouseState newState);
        public static event MStateChanged MouseChanged = delegate { };

        public delegate void ClickableMenuChanged(IClickableMenu newMenu);
        public static event ClickableMenuChanged MenuChanged = delegate { };

        public delegate void GameLocationsChanged(List<GameLocation> newLocations);
        public static event GameLocationsChanged LocationsChanged = delegate { };

        public delegate void CurrentLocationsChanged(GameLocation newLocation);
        public static event CurrentLocationsChanged CurrentLocationChanged = delegate { };

        public static event EventHandler Resize = delegate { };

        public delegate void FarmerChangedD(Farmer newFarmer);
        public static event FarmerChangedD FarmerChanged = delegate { };

        public delegate void IntChanged(Int32 newInt);
        public static event IntChanged TimeOfDayChanged = delegate { };
        public static event IntChanged DayOfMonthChanged = delegate { };
        public static event IntChanged YearOfGameChanged = delegate { };

        public delegate void StringChanged(String newString);
        public static event StringChanged SeasonOfYearChanged = delegate { };

        public static void InvokeGameLoaded()
        {
            GameLoaded.Invoke();
        }

        public static void InvokeInitialize()
        {
            try
            {
                Initialize.Invoke();
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
                LoadContent.Invoke();
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
                UpdateTick.Invoke();
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
                DrawTick.Invoke();
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA DrawTick: " + ex.ToString());
            }
        }

        public static void InvokeKeyboardChanged(KeyboardState newState)
        {
            KeyboardChanged.Invoke(newState);
        }

        public static void InvokeMouseChanged(MouseState newState)
        {
            MouseChanged.Invoke(newState);
        }

        public static void InvokeKeyPressed(Keys key)
        {
            KeyPressed.Invoke(key);
        }

        public static void InvokeMenuChanged(IClickableMenu newMenu)
        {
            MenuChanged.Invoke(newMenu);
        }

        public static void InvokeLocationsChanged(List<GameLocation> newLocations)
        {
            LocationsChanged.Invoke(newLocations);
        }

        public static void InvokeCurrentLocationChanged(GameLocation newLocation)
        {
            CurrentLocationChanged.Invoke(newLocation);
        }

        public static void InvokeResize(object sender, EventArgs e)
        {
            Resize.Invoke(sender, e);
        }

        public static void InvokeFarmerChanged(Farmer newFarmer)
        {
            FarmerChanged.Invoke(newFarmer);
        }

        public static void InvokeTimeOfDayChanged(Int32 newInt)
        {
            TimeOfDayChanged.Invoke(newInt);
        }

        public static void InvokeDayOfMonthChanged(Int32 newInt)
        {
            DayOfMonthChanged.Invoke(newInt);
        }

        public static void InvokeYearOfGameChanged(Int32 newInt)
        {
            YearOfGameChanged.Invoke(newInt);
        }

        public static void InvokeSeasonOfYearChanged(String newString)
        {
            SeasonOfYearChanged.Invoke(newString);
        }
    }
}
