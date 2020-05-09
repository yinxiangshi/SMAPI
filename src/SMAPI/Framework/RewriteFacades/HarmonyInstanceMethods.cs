using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace StardewModdingAPI.Framework.RewriteFacades
{
    /// <summary>Maps Harmony 1.x methods to Harmony 2.x to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should not be referenced directly by mods.</remarks>
    public class HarmonyInstanceMethods : Harmony
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The unique patch identifier.</param>
        public HarmonyInstanceMethods(string id)
            : base(id) { }

        /// <summary>Creates a new Harmony instance.</summary>
        /// <param name="id">A unique identifier for the instance.</param>
        public static Harmony Create(string id)
        {
            return new Harmony(id);
        }

        /// <summary>Apply one or more patches to a method.</summary>
        /// <param name="original">The original method.</param>
        /// <param name="prefix">The prefix to apply.</param>
        /// <param name="postfix">The postfix to apply.</param>
        /// <param name="transpiler">The transpiler to apply.</param>
        public DynamicMethod Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null)
        {
            try
            {
                MethodInfo method = base.Patch(original: original, prefix: prefix, postfix: postfix, transpiler: transpiler);
                return new DynamicMethod(method.Name, method.Attributes, method.CallingConvention, method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray(), method.Module, true);
            }
            catch (Exception ex)
            {
                var patchTypes = new List<string>();
                if (prefix != null)
                    patchTypes.Add("prefix");
                if (postfix != null)
                    patchTypes.Add("postfix");
                if (transpiler != null)
                    patchTypes.Add("transpiler");

                throw new Exception($"Failed applying {string.Join("/", patchTypes)} to method {original.DeclaringType?.FullName}.{original.Name}", ex);
            }
        }
    }
}
