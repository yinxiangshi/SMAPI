using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.Framework.ModLoading.Framework
{
    /// <summary>Finds incompatible CIL type reference instructions.</summary>
    internal abstract class BaseTypeFinder : BaseInstructionHandler
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Matches the type references to handle.</summary>
        private readonly Func<TypeReference, bool> IsMatchImpl;

        /// <summary>The result to return for matching instructions.</summary>
        private readonly InstructionHandleResult Result;


        /*********
        ** Public methods
        *********/
        /// <summary>Perform the predefined logic for a method if applicable.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="method">The method definition to handle.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public override InstructionHandleResult Handle(ModuleDefinition module, MethodDefinition method, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            return this.IsMatch(method)
                ? this.Result
                : InstructionHandleResult.None;
        }

        /// <summary>Perform the predefined logic for an instruction if applicable.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="cil">The CIL processor.</param>
        /// <param name="instruction">The CIL instruction to handle.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public override InstructionHandleResult Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            return this.IsMatch(instruction)
                ? this.Result
                : InstructionHandleResult.None;
        }

        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="method">The method definition.</param>
        public bool IsMatch(MethodDefinition method)
        {
            // return type
            if (this.IsMatch(method.ReturnType))
                return true;

            // parameters
            foreach (ParameterDefinition parameter in method.Parameters)
            {
                if (this.IsMatch(parameter.ParameterType))
                    return true;
            }

            // generic parameters
            foreach (GenericParameter parameter in method.GenericParameters)
            {
                if (this.IsMatch(parameter))
                    return true;
            }

            // custom attributes
            foreach (CustomAttribute attribute in method.CustomAttributes)
            {
                if (this.IsMatch(attribute.AttributeType))
                    return true;

                foreach (var arg in attribute.ConstructorArguments)
                {
                    if (this.IsMatch(arg.Type))
                        return true;
                }
            }

            // local variables
            foreach (VariableDefinition variable in method.Body.Variables)
            {
                if (this.IsMatch(variable.VariableType))
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        public bool IsMatch(Instruction instruction)
        {
            // field reference
            FieldReference fieldRef = RewriteHelper.AsFieldReference(instruction);
            if (fieldRef != null)
            {
                return
                    this.IsMatch(fieldRef.DeclaringType) // field on target class
                    || this.IsMatch(fieldRef.FieldType); // field value is target class
            }

            // method reference
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            if (methodRef != null)
            {
                return
                    this.IsMatch(methodRef.DeclaringType) // method on target class
                    || this.IsMatch(methodRef.ReturnType) // method returns target class
                    || methodRef.Parameters.Any(p => this.IsMatch(p.ParameterType)); // method parameters
            }

            return false;
        }

        /// <summary>Get whether a type reference matches the expected type.</summary>
        /// <param name="type">The type to check.</param>
        public bool IsMatch(TypeReference type)
        {
            // root type
            if (this.IsMatchImpl(type))
                return true;

            // generic arguments
            if (type is GenericInstanceType genericType)
            {
                if (genericType.GenericArguments.Any(this.IsMatch))
                    return true;
            }

            // generic parameters (e.g. constraints)
            if (type.GenericParameters.Any(this.IsMatch))
                return true;

            return false;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="isMatch">Matches the type references to handle.</param>
        /// <param name="result">The result to return for matching instructions.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches.</param>
        protected BaseTypeFinder(Func<TypeReference, bool> isMatch, InstructionHandleResult result, string nounPhrase)
            : base(nounPhrase)
        {
            this.IsMatchImpl = isMatch;
            this.Result = result;
        }
    }
}
