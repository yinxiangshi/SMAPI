using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace StardewModdingAPI.Framework.ModLoading.RewriteFacades
{
    /// <summary>Maps Harmony 1.x <code>HarmonyInstance</code> methods to Harmony 2.x's <see cref="Harmony"/> to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should not be referenced directly by mods.</remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used via assembly rewriting")]
    [SuppressMessage("ReSharper", "CS1591", Justification = "Documentation not needed for facade classes.")]
    public class HarmonyInstanceMethods : Harmony
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The unique patch identifier.</param>
        public HarmonyInstanceMethods(string id)
            : base(id) { }

        public static Harmony Create(string id)
        {
            return new Harmony(id);
        }

        public DynamicMethod Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null)
        {
            try
            {
                MethodInfo method = base.Patch(original: original, prefix: prefix, postfix: postfix, transpiler: transpiler);
                return (DynamicMethod)method;
            }
            catch (Exception ex)
            {
                // get patch types
                var patchTypes = new List<string>();
                if (prefix != null)
                    patchTypes.Add("prefix");
                if (postfix != null)
                    patchTypes.Add("postfix");
                if (transpiler != null)
                    patchTypes.Add("transpiler");

                // get original method label
                string methodLabel = original != null
                    ? $"method {original.DeclaringType?.FullName}.{original.Name}"
                    : "null method";

                throw new Exception($"Harmony instance {this.Id} failed applying {string.Join("/", patchTypes)} to {methodLabel}.", ex);
            }
        }
    }
}
