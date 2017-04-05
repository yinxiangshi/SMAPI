using System.Reflection;

namespace StardewModdingAPI
{
    /// <summary>A private property obtained through reflection.</summary>
    /// <typeparam name="TValue">The property value type.</typeparam>
    public interface IPrivateProperty<TValue>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The reflection metadata.</summary>
        PropertyInfo PropertyInfo { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the property value.</summary>
        TValue GetValue();

        /// <summary>Set the property value.</summary>
        //// <param name="value">The value to set.</param>
        void SetValue(TValue value);
    }
}