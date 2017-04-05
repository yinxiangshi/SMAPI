using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters
{
    /// <summary>Provides helper methods for field rewriters.</summary>
    internal static class RewriteHelper
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the field reference from an instruction if it matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        public static FieldReference AsFieldReference(Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Ldfld || instruction.OpCode == OpCodes.Ldsfld || instruction.OpCode == OpCodes.Stfld || instruction.OpCode == OpCodes.Stsfld
                ? (FieldReference)instruction.Operand
                : null;
        }

        /// <summary>Get the method reference from an instruction if it matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        public static MethodReference AsMethodReference(Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt
                ? (MethodReference)instruction.Operand
                : null;
        }

        /// <summary>Get whether a type matches a type reference.</summary>
        /// <param name="type">The defined type.</param>
        /// <param name="reference">The type reference.</param>
        public static bool IsSameType(Type type, TypeReference reference)
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
                    if (!RewriteHelper.IsSameType(defGenerics[i], refGenerics[i]))
                        return false;
                }
            }

            return true;
        }

        /// <summary>Get whether a method definition matches the signature expected by a method reference.</summary>
        /// <param name="definition">The method definition.</param>
        /// <param name="reference">The method reference.</param>
        public static bool HasMatchingSignature(MethodInfo definition, MethodReference reference)
        {
            // same name
            if (definition.Name != reference.Name)
                return false;

            // same arguments
            ParameterInfo[] definitionParameters = definition.GetParameters();
            ParameterDefinition[] referenceParameters = reference.Parameters.ToArray();
            if (referenceParameters.Length != definitionParameters.Length)
                return false;
            for (int i = 0; i < referenceParameters.Length; i++)
            {
                if (!RewriteHelper.IsSameType(definitionParameters[i].ParameterType, referenceParameters[i].ParameterType))
                    return false;
            }
            return true;
        }

        /// <summary>Get whether a type has a method whose signature matches the one expected by a method reference.</summary>
        /// <param name="type">The type to check.</param>
        /// <param name="reference">The method reference.</param>
        public static bool HasMatchingSignature(Type type, MethodReference reference)
        {
            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Any(method => RewriteHelper.HasMatchingSignature(method, reference));
        }
    }
}
