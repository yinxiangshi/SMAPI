using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Finders;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters
{
    /// <summary>Rewrites all references to a type.</summary>
    internal class TypeReferenceRewriter : TypeFinder
    {
        /*********
        ** Properties
        *********/
        /// <summary>The full type name to which to find references.</summary>
        private readonly string FromTypeName;

        /// <summary>The new type to reference.</summary>
        private readonly Type ToType;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fromTypeFullName">The full type name to which to find references.</param>
        /// <param name="toType">The new type to reference.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches (or <c>null</c> to generate one).</param>
        public TypeReferenceRewriter(string fromTypeFullName, Type toType, string nounPhrase = null)
            : base(fromTypeFullName, nounPhrase)
        {
            this.FromTypeName = fromTypeFullName;
            this.ToType = toType;
        }

        /// <summary>Rewrite a method definition for compatibility.</summary>
        /// <param name="mod">The mod to which the module belongs.</param>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="method">The method definition to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        public override bool Rewrite(IModMetadata mod, ModuleDefinition module, MethodDefinition method, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            bool rewritten = false;

            // return type
            if (this.IsMatch(method.ReturnType))
            {
                method.ReturnType = this.RewriteIfNeeded(module, method.ReturnType);
                rewritten = true;
            }

            // parameters
            foreach (ParameterDefinition parameter in method.Parameters)
            {
                if (this.IsMatch(parameter.ParameterType))
                {
                    parameter.ParameterType = this.RewriteIfNeeded(module, parameter.ParameterType);
                    rewritten = true;
                }
            }

            // generic parameters
            for (int i = 0; i < method.GenericParameters.Count; i++)
            {
                var parameter = method.GenericParameters[i];
                if (this.IsMatch(parameter))
                {
                    TypeReference newType = this.RewriteIfNeeded(module, parameter);
                    if (newType != parameter)
                        method.GenericParameters[i] = new GenericParameter(parameter.Name, newType);
                    rewritten = true;
                }
            }

            // local variables
            foreach (VariableDefinition variable in method.Body.Variables)
            {
                if (this.IsMatch(variable.VariableType))
                {
                    variable.VariableType = this.RewriteIfNeeded(module, variable.VariableType);
                    rewritten = true;
                }
            }

            return rewritten;
        }

        /// <summary>Rewrite a CIL instruction for compatibility.</summary>
        /// <param name="mod">The mod to which the module belongs.</param>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="cil">The CIL rewriter.</param>
        /// <param name="instruction">The instruction to rewrite.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        /// <returns>Returns whether the instruction was rewritten.</returns>
        /// <exception cref="IncompatibleInstructionException">The CIL instruction is not compatible, and can't be rewritten.</exception>
        public override bool Rewrite(IModMetadata mod, ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            if (!this.IsMatch(instruction) && !instruction.ToString().Contains(this.FromTypeName))
                return false;

            // field reference
            FieldReference fieldRef = RewriteHelper.AsFieldReference(instruction);
            if (fieldRef != null)
            {
                fieldRef.DeclaringType = this.RewriteIfNeeded(module, fieldRef.DeclaringType);
                fieldRef.FieldType = this.RewriteIfNeeded(module, fieldRef.FieldType);
            }

            // method reference
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            if (methodRef != null)
            {
                methodRef.DeclaringType = this.RewriteIfNeeded(module, methodRef.DeclaringType);
                methodRef.ReturnType = this.RewriteIfNeeded(module, methodRef.ReturnType);
                foreach (var parameter in methodRef.Parameters)
                    parameter.ParameterType = this.RewriteIfNeeded(module, parameter.ParameterType);
            }

            // type reference
            if (instruction.Operand is TypeReference typeRef)
            {
                TypeReference newRef = this.RewriteIfNeeded(module, typeRef);
                if (typeRef != newRef)
                    cil.Replace(instruction, cil.Create(instruction.OpCode, newRef));
            }

            return true;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get the adjusted type reference if it matches, else the same value.</summary>
        /// <param name="module">The module being rewritten.</param>
        /// <param name="type">The type to replace if it matches.</param>
        private TypeReference RewriteIfNeeded(ModuleDefinition module, TypeReference type)
        {
            // root type
            if (type.FullName == this.FromTypeName)
                return module.Import(this.ToType);

            // generic arguments
            if (type is GenericInstanceType genericType)
            {
                for (int i = 0; i < genericType.GenericArguments.Count; i++)
                    genericType.GenericArguments[i] = this.RewriteIfNeeded(module, genericType.GenericArguments[i]);
            }

            // generic parameters (e.g. constraints)
            for (int i = 0; i < type.GenericParameters.Count; i++)
                type.GenericParameters[i] = new GenericParameter(this.RewriteIfNeeded(module, type.GenericParameters[i]));

            return type;
        }
    }
}
