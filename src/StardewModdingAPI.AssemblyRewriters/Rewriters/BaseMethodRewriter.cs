using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Base class for a method rewriter.</summary>
    public abstract class BaseMethodRewriter : IMethodRewriter
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the given method reference can be rewritten.</summary>
        /// <param name="methodRef">The method reference.</param>
        public abstract bool ShouldRewrite(MethodReference methodRef);

        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="callOp">The instruction which calls the method.</param>
        /// <param name="methodRef">The method reference invoked by the <paramref name="callOp"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        public abstract void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction callOp, MethodReference methodRef, PlatformAssemblyMap assemblyMap);


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a method definition matches the signature expected by a method reference.</summary>
        /// <param name="definition">The method definition.</param>
        /// <param name="reference">The method reference.</param>
        protected bool HasMatchingSignature(MethodInfo definition, MethodReference reference)
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
                if (!this.IsMatchingType(definitionParameters[i].ParameterType, referenceParameters[i].ParameterType))
                    return false;
            }
            return true;
        }

        /// <summary>Get whether a type has a method whose signature matches the one expected by a method reference.</summary>
        /// <param name="type">The type to check.</param>
        /// <param name="reference">The method reference.</param>
        protected bool HasMatchingSignature(Type type, MethodReference reference)
        {
            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Any(method => this.HasMatchingSignature(method, reference));
        }

        /// <summary>Get whether a type matches a type reference.</summary>
        /// <param name="type">The defined type.</param>
        /// <param name="reference">The type reference.</param>
        private bool IsMatchingType(Type type, TypeReference reference)
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
                    if (!this.IsMatchingType(defGenerics[i], refGenerics[i]))
                        return false;
                }
            }

            return true;
        }
    }
}