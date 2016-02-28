# SMAPI
A Modding API For Stardew Valley

There is no documentation right now.
You compile this if you're interested and throw it next to your StardewValley.exe and run it. It should open the game and a beautiful black box that look atrocious honestly. That's the modding api. You can make a project and reference that to add functionality. Below is my test mod class. Mods go in C:\Users\<USERNAME>\AppData\Roaming\StardewValley\Mods or something like that.

It is currently 7AM EST and I have not slept in over 40 hours, so good night/morning/etc to anyone reading, I'll make this more proper some other time.

TestMod.cs:

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

            Events.GameLoaded += Events_GameLoaded;
        }

        void Events_GameLoaded()
        {
            
            Program.LogInfo("[Game Loaded Event] I can do things directly to the game now that I am certain it is loaded thanks to events.");
        }
    }
