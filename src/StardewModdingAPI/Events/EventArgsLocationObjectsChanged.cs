using System;
using Microsoft.Xna.Framework;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewModdingAPI.Events
{
    public class EventArgsLocationObjectsChanged : EventArgs
    {
        public EventArgsLocationObjectsChanged(SerializableDictionary<Vector2, Object> newObjects)
        {
            NewObjects = newObjects;
        }

        public SerializableDictionary<Vector2, Object> NewObjects { get; private set; }
    }
}