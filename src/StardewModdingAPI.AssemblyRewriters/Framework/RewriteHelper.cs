using System;
using Mono.Cecil;

namespace StardewModdingAPI.AssemblyRewriters.Framework
{
    /// <summary>Provides helper methods for field rewriters.</summary>
    internal static class RewriteHelper
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether a type matches a type reference.</summary>
        /// <param name="type">The defined type.</param>
        /// <param name="reference">The type reference.</param>
        public static bool IsMatchingType(Type type, TypeReference reference)
        {
            // same namespace & name
            if (type.Namespace != reference.Namespace || type.Name != reference.Name)
                return false;

            // same generic parameters
            if (type.IsGenericType)
            {
                if (!reference.IsGenericInstance)
                    return false;

                Type[] defGenerics = type.GetGenericArguments();
                TypeReference[] refGenerics = ((GenericInstanceType)reference).GenericArguments.ToArray();
                if (defGenerics.Length != refGenerics.Length)
                    return false;
                for (int i = 0; i < defGenerics.Length; i++)
                {
                    if (!RewriteHelper.IsMatchingType(defGenerics[i], refGenerics[i]))
                        return false;
                }
            }

            return true;
        }
    }
}
