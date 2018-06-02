namespace StardewModdingAPI.Events
{
    /// <summary>Manages access to events raised by SMAPI.</summary>
    public interface IModEvents
    {
        /// <summary>Events raised when the player provides input using a controller, keyboard, or mouse.</summary>
        IInputEvents Input { get; }

        /// <summary>Events raised when something changes in the world.</summary>
        IWorldEvents World { get; }
    }
}
