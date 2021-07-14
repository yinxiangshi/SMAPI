// This temporary utility fixes an esoteric issue in XNA Framework where deserialization depends on
// the order of fields returned by Type.GetFields, but that order changes after Harmony/MonoMod use
// reflection to access the fields due to an issue in .NET Framework.
// https://twitter.com/0x0ade/status/1414992316964687873
//
// This will be removed when Harmony/MonoMod are updated to incorporate the fix.
//
// Special thanks to 0x0ade for submitting this worokaround! Copy/pasted and adapted from MonoMod.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;

// ReSharper disable once CheckNamespace -- Temporary hotfix submitted by the MonoMod author.
namespace MonoMod.Utils
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Temporary hotfix submitted by the MonoMod author.")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Temporary hotfix submitted by the MonoMod author.")]
    static class MiniMonoModHotfix
    {
        // .NET Framework can break member ordering if using Module.Resolve* on certain members.

        private static readonly object[] _NoArgs = new object[0];
        private static readonly object[] _CacheGetterArgs = { /* MemberListType.All */ 0, /* name apparently always null? */ null };

        private static readonly Type t_RuntimeModule =
            typeof(Module).Assembly
            .GetType("System.Reflection.RuntimeModule");

        private static readonly PropertyInfo p_RuntimeModule_RuntimeType =
            typeof(Module).Assembly
            .GetType("System.Reflection.RuntimeModule")
            ?.GetProperty("RuntimeType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly Type t_RuntimeType =
            typeof(Type).Assembly
            .GetType("System.RuntimeType");

        private static readonly PropertyInfo p_RuntimeType_Cache =
            typeof(Type).Assembly
            .GetType("System.RuntimeType")
            ?.GetProperty("Cache", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo m_RuntimeTypeCache_GetFieldList =
            typeof(Type).Assembly
            .GetType("System.RuntimeType+RuntimeTypeCache")
            ?.GetMethod("GetFieldList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo m_RuntimeTypeCache_GetPropertyList =
            typeof(Type).Assembly
            .GetType("System.RuntimeType+RuntimeTypeCache")
            ?.GetMethod("GetPropertyList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConditionalWeakTable<Type, object> _CacheFixed = new ConditionalWeakTable<Type, object>();

        public static void Apply()
        {
            var harmony = new Harmony("MiniMonoModHotfix");

            harmony.Patch(
                original: typeof(Harmony).Assembly
                    .GetType("HarmonyLib.MethodBodyReader")
                    .GetMethod("ReadOperand", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                transpiler: new HarmonyMethod(typeof(MiniMonoModHotfix), nameof(ResolveTokenFix))
            );

            harmony.Patch(
                original: typeof(Harmony).Assembly
                    .GetType("MonoMod.Utils.DynamicMethodDefinition+<>c__DisplayClass3_0")
                    .GetMethod("<_CopyMethodToDefinition>g__ResolveTokenAs|1", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                transpiler: new HarmonyMethod(typeof(MiniMonoModHotfix), nameof(ResolveTokenFix))
            );

        }

        private static IEnumerable<CodeInstruction> ResolveTokenFix(IEnumerable<CodeInstruction> instrs)
        {
            MethodInfo getdecl = typeof(MiniMonoModHotfix).GetMethod(nameof(GetRealDeclaringType));
            MethodInfo fixup = typeof(MiniMonoModHotfix).GetMethod(nameof(FixReflectionCache));

            foreach (CodeInstruction instr in instrs)
            {
                yield return instr;

                if (instr.operand is MethodInfo called)
                {
                    switch (called.Name)
                    {
                        case "ResolveType":
                            // type.FixReflectionCache();
                            yield return new CodeInstruction(OpCodes.Dup);
                            yield return new CodeInstruction(OpCodes.Call, fixup);
                            break;

                        case "ResolveMember":
                        case "ResolveMethod":
                        case "ResolveField":
                            // member.GetRealDeclaringType().FixReflectionCache();
                            yield return new CodeInstruction(OpCodes.Dup);
                            yield return new CodeInstruction(OpCodes.Call, getdecl);
                            yield return new CodeInstruction(OpCodes.Call, fixup);
                            break;
                    }
                }
            }
        }

        public static Type GetModuleType(this Module module)
        {
            // Sadly we can't blindly resolve type 0x02000001 as the runtime throws ArgumentException.

            if (module == null || t_RuntimeModule == null || !t_RuntimeModule.IsInstanceOfType(module))
                return null;

            // .NET
            if (p_RuntimeModule_RuntimeType != null)
                return (Type)p_RuntimeModule_RuntimeType.GetValue(module, _NoArgs);

            // The hotfix doesn't apply to Mono anyway, thus that's not copied over.

            return null;
        }

        public static Type GetRealDeclaringType(this MemberInfo member)
            => member.DeclaringType ?? member.Module?.GetModuleType();

        public static void FixReflectionCache(this Type type)
        {
            if (t_RuntimeType == null ||
                p_RuntimeType_Cache == null ||
                m_RuntimeTypeCache_GetFieldList == null ||
                m_RuntimeTypeCache_GetPropertyList == null)
                return;

            for (; type != null; type = type.DeclaringType)
            {
                // All types SHOULD inherit RuntimeType, including those built at runtime.
                // One might never know what awaits us in the depths of reflection hell though.
                if (!t_RuntimeType.IsInstanceOfType(type))
                    continue;

                _CacheFixed.GetValue(type, rt =>
                {

                    object cache = p_RuntimeType_Cache.GetValue(rt, _NoArgs);
                    _FixReflectionCacheOrder<PropertyInfo>(cache, m_RuntimeTypeCache_GetPropertyList);
                    _FixReflectionCacheOrder<FieldInfo>(cache, m_RuntimeTypeCache_GetFieldList);

                    return new object();
                });
            }
        }

        private static void _FixReflectionCacheOrder<T>(object cache, MethodInfo getter) where T : MemberInfo
        {
            // Get and discard once, otherwise we might not be getting the actual backing array.
            getter.Invoke(cache, _CacheGetterArgs);
            Array orig = (Array)getter.Invoke(cache, _CacheGetterArgs);

            // Sort using a short-lived list.
            List<T> list = new List<T>(orig.Length);
            for (int i = 0; i < orig.Length; i++)
                list.Add((T)orig.GetValue(i));

            list.Sort((a, b) => a.MetadataToken - b.MetadataToken);

            for (int i = orig.Length - 1; i >= 0; --i)
                orig.SetValue(list[i], i);
        }

    }
}
