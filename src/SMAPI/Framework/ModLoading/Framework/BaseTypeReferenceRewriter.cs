using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.Framework.ModLoading.Framework
{
    /// <summary>Rewrites all references to a type.</summary>
    internal abstract class BaseTypeReferenceRewriter : IInstructionHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>The type finder which matches types to rewrite.</summary>
        private readonly BaseTypeFinder Finder;


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the handler matches.</summary>
        public string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="finder">The type finder which matches types to rewrite.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches.</param>
        public BaseTypeReferenceRewriter(BaseTypeFinder finder, string nounPhrase)
        {
            this.Finder = finder;
            this.NounPhrase = nounPhrase;
        }

        /// <summary>Perform the predefined logic for a method if applicable.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="method">The method definition containing the instruction.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public InstructionHandleResult Handle(ModuleDefinition module, MethodDefinition method, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            bool rewritten = false;

            // return type
            if (this.Finder.IsMatch(method.ReturnType))
            {
                rewritten |= this.RewriteIfNeeded(module, method.ReturnType, newType => method.ReturnType = newType);
            }

            // parameters
            foreach (ParameterDefinition parameter in method.Parameters)
            {
                if (this.Finder.IsMatch(parameter.ParameterType))
                    rewritten |= this.RewriteIfNeeded(module, parameter.ParameterType, newType => parameter.ParameterType = newType);
            }

            // generic parameters
            for (int i = 0; i < method.GenericParameters.Count; i++)
            {
                var parameter = method.GenericParameters[i];
                if (this.Finder.IsMatch(parameter))
                    rewritten |= this.RewriteIfNeeded(module, parameter, newType => method.GenericParameters[i] = new GenericParameter(parameter.Name, newType));
            }

            // local variables
            foreach (VariableDefinition variable in method.Body.Variables)
            {
                if (this.Finder.IsMatch(variable.VariableType))
                    rewritten |= this.RewriteIfNeeded(module, variable.VariableType, newType => variable.VariableType = newType);
            }

            return rewritten
                ? InstructionHandleResult.Rewritten
                : InstructionHandleResult.None;
        }

        /// <summary>Perform the predefined logic for an instruction if applicable.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="cil">The CIL processor.</param>
        /// <param name="instruction">The instruction to handle.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public InstructionHandleResult Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            if (!this.Finder.IsMatch(instruction))
                return InstructionHandleResult.None;
            bool rewritten = false;

            // field reference
            FieldReference fieldRef = RewriteHelper.AsFieldReference(instruction);
            if (fieldRef != null)
            {
                rewritten |= this.RewriteIfNeeded(module, fieldRef.DeclaringType, newType => fieldRef.DeclaringType = newType);
                rewritten |= this.RewriteIfNeeded(module, fieldRef.FieldType, newType => fieldRef.FieldType = newType);
            }

            // method reference
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            if (methodRef != null)
            {
                rewritten |= this.RewriteIfNeeded(module, methodRef.DeclaringType, newType => methodRef.DeclaringType = newType);
                rewritten |= this.RewriteIfNeeded(module, methodRef.ReturnType, newType => methodRef.ReturnType = newType);
                foreach (var parameter in methodRef.Parameters)
                    rewritten |= this.RewriteIfNeeded(module, parameter.ParameterType, newType => parameter.ParameterType = newType);
            }

            // type reference
            if (instruction.Operand is TypeReference typeRef)
                rewritten |= this.RewriteIfNeeded(module, typeRef, newType => cil.Replace(instruction, cil.Create(instruction.OpCode, newType)));

            return rewritten
                ? InstructionHandleResult.Rewritten
                : InstructionHandleResult.None;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Change a type reference if needed.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="type">The type to replace if it matches.</param>
        /// <param name="set">Assign the new type reference.</param>
        protected abstract bool RewriteIfNeeded(ModuleDefinition module, TypeReference type, Action<TypeReference> set);
    }
}
