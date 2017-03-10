using System;

namespace StardewModdingAPI
{
    /// <summary>Encapsulates access and changes to content being read from a data file.</summary>
    public interface IContentEventHelper : IContentEventData<object>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get a helper to manipulate the data as a dictionary.</summary>
        /// <typeparam name="TKey">The expected dictionary key.</typeparam>
        /// <typeparam name="TValue">The expected dictionary balue.</typeparam>
        /// <exception cref="InvalidOperationException">The content being read isn't a dictionary.</exception>
        IContentEventHelperForDictionary<TKey, TValue> AsDictionary<TKey, TValue>();

        /// <summary>Get a helper to manipulate the data as an image.</summary>
        /// <exception cref="InvalidOperationException">The content being read isn't an image.</exception>
        IContentEventHelperForImage AsImage();

        /// <summary>Get the data as a given type.</summary>
        /// <typeparam name="TData">The expected data type.</typeparam>
        /// <exception cref="InvalidCastException">The data can't be converted to <typeparamref name="TData"/>.</exception>
        TData GetData<TData>();
    }
}
