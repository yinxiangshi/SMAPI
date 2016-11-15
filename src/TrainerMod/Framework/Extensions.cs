namespace TrainerMod.Framework
{
    /// <summary>Provides extension methods on primitive types.</summary>
    internal static class Extensions
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether an object is a number.</summary>
        /// <param name="value">The object value.</param>
        public static bool IsInt(this object value)
        {
            int i;
            return int.TryParse(value.ToString(), out i);
        }

        /// <summary>Parse an object into a number.</summary>
        /// <param name="value">The object value.</param>
        /// <exception cref="System.FormatException">The value is not a valid number.</exception>
        public static int ToInt(this object value)
        {
            return int.Parse(value.ToString());
        }
    }
}
