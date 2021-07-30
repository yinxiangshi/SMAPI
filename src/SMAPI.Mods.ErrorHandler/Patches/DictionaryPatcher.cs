using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI.Framework.Patching;
using StardewValley.GameData;
using StardewValley.GameData.HomeRenovations;
using StardewValley.GameData.Movies;

namespace StardewModdingAPI.Mods.ErrorHandler.Patches
{
    /// <summary>A Harmony patch for <see cref="Dictionary{TKey,TValue}"/> which adds the accessed key to <see cref="KeyNotFoundException"/> exceptions.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class DictionaryPatcher : IHarmonyPatch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private code.</summary>
        private static IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflector">Simplifies access to private code.</param>
        public DictionaryPatcher(IReflectionHelper reflector)
        {
            DictionaryPatcher.Reflection = reflector;
        }

        /// <inheritdoc />
        public void Apply(Harmony harmony)
        {
            Type[] keyTypes = { typeof(int), typeof(string) };
            Type[] valueTypes = { typeof(int), typeof(string), typeof(HomeRenovation), typeof(MovieData), typeof(SpecialOrderData) };

            foreach (Type keyType in keyTypes)
            {
                foreach (Type valueType in valueTypes)
                {
                    Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

                    harmony.Patch(
                        original: AccessTools.Method(dictionaryType, "get_Item"),
                        finalizer: new HarmonyMethod(this.GetType(), nameof(DictionaryPatcher.Finalize_GetItem))
                    );
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after the dictionary indexer throws an exception.</summary>
        /// <param name="key">The dictionary key being fetched.</param>
        /// <param name="__exception">The exception thrown by the wrapped method, if any.</param>
        /// <returns>Returns the exception to throw, if any.</returns>
        private static Exception Finalize_GetItem(object key, Exception __exception)
        {
            if (__exception is KeyNotFoundException)
                AddKeyTo(__exception, key?.ToString());

            return __exception;
        }

        /// <summary>Add the accessed key to an exception message.</summary>
        /// <param name="exception">The exception to modify.</param>
        /// <param name="key">The dictionary key.</param>
        private static void AddKeyTo(Exception exception, string key)
        {
            DictionaryPatcher.Reflection
                .GetField<string>(exception, "_message")
                .SetValue($"{exception.Message}\nkey: '{key}'");
        }
    }
}
