using System;

namespace StardewModdingAPI.Events
{
    public class EventArgsLoadedGameChanged : EventArgs
    {
        public EventArgsLoadedGameChanged(bool loadedGame)
        {
            LoadedGame = loadedGame;
        }

        public bool LoadedGame { get; private set; }
    }
}