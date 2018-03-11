#if !STARDEW_VALLEY_1_3
using System;
using System.Reflection;

namespace StardewModdingAPI
{
    /// <summary>A private field obtained through reflection.</summary>
    /// <typeparam name="TValue">The field value type.</typeparam>
    [Obsolete("Use " + nameof(IReflectedField<TValue>) + " instead")]
    public interface IPrivateField<TValue>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The reflection metadata.</summary>
        FieldInfo FieldInfo { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the field value.</summary>
        TValue GetValue();

        /// <summary>Set the field value.</summary>
        //// <param name="value">The value to set.</param>
        void SetValue(TValue value);
    }
}
#endif
