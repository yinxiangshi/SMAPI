# SMAPI
A Modding API For Stardew Valley

You can create a mod by making a direct reference to the ModdingApi.exe

From there, you need to inherit from StardewModdingAPI.Mod

The first class that inherits from that class will be loaded into the game at runtime, and once the game fully initializes the mod, the method Entry() will be called once.

It is recommended to subscribe to an event (from Events.cs) to be able to interface with the game rather than directly make changes from the Entry() method.


    TestMod.cs:
    
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;
        using Microsoft.Xna.Framework.Input;
        using StardewModdingAPI;

        namespace StardewTestMod
        {
            public class TestMod : Mod
            {
                public override string Name
                {
                    get { return "Test Mod"; }
                }

                public override string Authour
                {
                    get { return "Zoryn Aaron"; }
                }

                public override string Version
                {
                    get { return "0.0.1Test"; }
                }

                public override string Description
                {
                    get { return "A Test Mod"; }
                }

                public override void Entry()
                {
                    Console.WriteLine("Test Mod Has Loaded");
                    Program.LogError("Test Mod can call to Program.cs in the API");
                    Program.LogColour(ConsoleColor.Magenta, "Test Mod is just a tiny DLL file in AppData/Roaming/StardewValley/Mods");
                    
                    //Subscribe to an event from the modding API
                    Events.KeyPressed += Events_KeyPressed;
                }

                void Events_KeyPressed(Keys key)
                {
                    Console.WriteLine("TestMod sees that the following key was pressed: " + key);
                }
            }
        }
        
        
Break Fishing (WARNING: SOFTLOCKS YOUR GAME):

    public override void Entry()
        {
            Events.UpdateTick += Events_UpdateTick;
            Events.Initialize += Events_Initialize;
        }
    
    private FieldInfo cmg;
    private bool gotGame;
    private SBobberBar sb;
    void Events_Initialize()
        {
            cmg = SGame.StaticFields.First(x => x.Name == "activeClickableMenu");
        }
    
    void Events_UpdateTick()
        {
            if (cmg != null && cmg.GetValue(null) != null)
            {
                if (cmg.GetValue(null).GetType() == typeof(BobberBar))
                {
                    if (!gotGame)
                    {
                        gotGame = true;
                        BobberBar b = (BobberBar)cmg.GetValue(null);
                        sb = SBobberBar.ConstructFromBaseClass(b);
                    }
                    else
                    {
                        sb.bobberPosition = Extensions.Random.Next(0, 750);
                        sb.treasure = true;
                        sb.distanceFromCatching = 0.5f;
                    }
                }
                else
                {
                    gotGame = false;
                    sb = null;
                }
            }
        }