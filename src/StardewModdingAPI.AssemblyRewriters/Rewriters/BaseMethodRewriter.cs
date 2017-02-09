using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters.Rewriters
{
    /// <summary>Base class for a method rewriter.</summary>
    public abstract class BaseMethodRewriter : IInstructionRewriter
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether a CIL instruction should be rewritten.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public bool ShouldRewrite(Instruction instruction, bool platformChanged)
        {
            // ignore non-method-call instructions
            if (instruction.OpCode != OpCodes.Call && instruction.OpCode != OpCodes.Callvirt)
                return false;

            // check reference
            MethodReference methodRef = (MethodReference)instruction.Operand;
            return this.ShouldRewrite(methodRef, platformChanged);
        }

        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        public void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap)
        {
            MethodReference methodRef = (MethodReference)instruction.Operand;
            this.Rewrite(module, cil, instruction, methodRef, assemblyMap);
        }

        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether the given method reference can be rewritten.</summary>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        protected abstract bool ShouldRewrite(MethodReference methodRef, bool platformChanged);

        /// <summary>Rewrite a method for compatibility.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction which calls the method.</param>
        /// <param name="methodRef">The method reference invoked by the <paramref name="instruction"/>.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        protected abstract void Rewrite(ModuleDefinition module, ILProcessor cil, Instruction instruction, MethodReference methodRef, PlatformAssemblyMap assemblyMap);

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