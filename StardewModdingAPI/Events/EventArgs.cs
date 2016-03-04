using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI.Events
{
    public class EventArgsKeyboardStateChanged : EventArgs
    {
        public EventArgsKeyboardStateChanged(KeyboardState priorState, KeyboardState newState)
        {
            NewState = newState;
            NewState = newState;
        }
        public KeyboardState NewState { get; private set; }
        public KeyboardState PriorState { get; private set; }
    }

    public class EventArgsKeyPressed : EventArgs
    {
        public EventArgsKeyPressed(Keys keyPressed)
        {
            KeyPressed = keyPressed;
        }
        public Keys KeyPressed { get; private set; }
    }
        
    public class EventArgsMouseStateChanged : EventArgs
    { 
        public EventArgsMouseStateChanged(MouseState priorState, MouseState newState)
        {
            NewState = newState;
            NewState = newState;
        }
        public MouseState NewState { get; private set; }
        public MouseState PriorState { get; private set; }
    }

    public class EventArgsClickableMenuChanged : EventArgs
    {
        public EventArgsClickableMenuChanged(IClickableMenu priorMenu, IClickableMenu newMenu)
        {
            NewMenu = newMenu;
            PriorMenu = priorMenu;
        }
        public IClickableMenu NewMenu { get; private set; }
        public IClickableMenu PriorMenu { get; private set; }
    }

    public class EventArgsGameLocationsChanged : EventArgs
    {
        public EventArgsGameLocationsChanged(List<GameLocation> newLocations)
        {
            NewLocations = newLocations;
        }
        public List<GameLocation> NewLocations { get; private set; }
    }

    public class EventArgsLocationObjectsChanged : EventArgs
    {
        public EventArgsLocationObjectsChanged(SerializableDictionary<Vector2, StardewValley.Object> newObjects)
        {
            NewObjects = newObjects;
        }
        public SerializableDictionary<Vector2, StardewValley.Object> NewObjects { get; private set; }
    }

    public class EventArgsCurrentLocationChanged : EventArgs
    {
        public EventArgsCurrentLocationChanged(GameLocation priorLocation, GameLocation newLocation)
        {
            NewLocation = newLocation;
            PriorLocation = priorLocation;
        }
        public GameLocation NewLocation { get; private set; }
        public GameLocation PriorLocation { get; private set; }
    }

    public class EventArgsFarmerChanged : EventArgs
    {
        public EventArgsFarmerChanged(Farmer priorFarmer, Farmer newFarmer)
        {
            NewFarmer = NewFarmer;
            PriorFarmer = PriorFarmer;
        }
        public Farmer NewFarmer { get; private set; }
        public Farmer PriorFarmer { get; private set; }
    }

    public class EventArgsInventoryChanged : EventArgs
    {
        public EventArgsInventoryChanged(List<Item> inventory)
        {
            Inventory = inventory;
        }
        public List<Item> Inventory { get; private set; }
    }

    public class EventArgsIntChanged : EventArgs
    {
        public EventArgsIntChanged(Int32 priorInt, Int32 newInt)
        {
            NewInt = NewInt;
            PriorInt = PriorInt;
        }
        public Int32 NewInt { get; private set; }
        public Int32 PriorInt { get; private set; }
    }

    public class EventArgsStringChanged : EventArgs
    {
        public EventArgsStringChanged(String priorString, String newString)
        {
            NewString = newString;
            PriorString = priorString;
        }
        public String NewString { get; private set; }
        public String PriorString { get; private set; }
    }

    public class EventArgsCommand : EventArgs
    {
        public EventArgsCommand(Command command)
        {
            Command = command;
        }
        public Command Command { get; private set; }
    }
}

