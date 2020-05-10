using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace StardewModdingAPI.Framework.ModLoading.Framework
{
    /// <summary>Rewrites all references to a type.</summary>
    internal abstract class BaseTypeReferenceRewriter : BaseInstructionHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>The type finder which matches types to rewrite.</summary>
        private readonly BaseTypeFinder Finder;


        /*********
        ** Public methods
        *********/
        /// <summary>Perform the predefined logic for a method if applicable.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="type">The type definition to handle.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public override InstructionHandleResult Handle(ModuleDefinition module, TypeDefinition type, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            bool rewritten = this.RewriteCustomAttributesIfNeeded(module, type.CustomAttributes);

            return rewritten
                ? InstructionHandleResult.Rewritten
                : InstructionHandleResult.None;
        }

        /// <summary>Perform the predefined logic for a method if applicable.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="method">The method definition to handle.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public override InstructionHandleResult Handle(ModuleDefinition module, MethodDefinition method, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            bool rewritten = false;

            // return type
            if (this.Finder.IsMatch(method.ReturnType))
                rewritten |= this.RewriteIfNeeded(module, method.ReturnType, newType => method.ReturnType = newType);

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

            // custom attributes
            rewritten |= this.RewriteCustomAttributesIfNeeded(module, method.CustomAttributes);

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
        /// <param name="instruction">The CIL instruction to handle.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public override InstructionHandleResult Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
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
                if (methodRef is GenericInstanceMethod genericRef)
                {
                    for (int i = 0; i < genericRef.GenericArguments.Count; i++)
                        rewritten |= this.RewriteIfNeeded(module, genericRef.GenericArguments[i], newType => genericRef.GenericArguments[i] = newType);
                }
            }

            // type reference
            if (instruction.Operand is TypeReference typeRef)
                rewritten |= this.RewriteIfNeeded(module, typeRef, newType => cil.Replace(instruction, cil.Create(instruction.OpCode, newType)));

            return rewritten
                ? InstructionHandleResult.Rewritten
                : InstructionHandleResult.None;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="finder">The type finder which matches types to rewrite.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches.</param>
        protected BaseTypeReferenceRewriter(BaseTypeFinder finder, string nounPhrase)
            : base(nounPhrase)
        {
            this.Finder = finder;
        }

        /// <summary>Change a type reference if needed.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="type">The type to replace if it matches.</param>
        /// <param name="set">Assign the new type reference.</param>
        protected abstract bool RewriteIfNeeded(ModuleDefinition module, TypeReference type, Action<TypeReference> set);

        /// <summary>Rewrite custom attributes if needed.</summary>
        /// <param name="module">The assembly module containing the attributes.</param>
        /// <param name="attributes">The custom attributes to handle.</param>
        private bool RewriteCustomAttributesIfNeeded(ModuleDefinition module, Collection<CustomAttribute> attributes)
        {
            bool rewritten = false;

            for (int attrIndex = 0; attrIndex < attributes.Count; attrIndex++)
            {
                CustomAttribute attribute = attributes[attrIndex];
                bool curChanged = false;

                // attribute type
                TypeReference newAttrType = null;
                if (this.Finder.IsMatch(attribute.AttributeType))
                {
                    rewritten |= this.RewriteIfNeeded(module, attribute.AttributeType, newType =>
                    {
                        newAttrType = newType;
                        curChanged = true;
                    });
                }

                // constructor arguments
                TypeReference[] argTypes = new TypeReference[attribute.ConstructorArguments.Count];
                for (int i = 0; i < argTypes.Length; i++)
                {
                    var arg = attribute.ConstructorArguments[i];

                    argTypes[i] = arg.Type;
                    rewritten |= this.RewriteIfNeeded(module, arg.Type, newType =>
                    {
                        argTypes[i] = newType;
                        curChanged = true;
                    });
                }

                // swap attribute
                if (curChanged)
                {
                    // get constructor
                    MethodDefinition constructor = (newAttrType ?? attribute.AttributeType)
                        .Resolve()
                        .Methods
                        .Where(method => method.IsConstructor)
                        .FirstOrDefault(ctor => RewriteHelper.HasMatchingSignature(ctor, attribute.Constructor));
                    if (constructor == null)
                        throw new InvalidOperationException($"Can't rewrite attribute type '{attribute.AttributeType.FullName}' to '{newAttrType?.FullName}', no equivalent constructor found.");

                    // create new attribute
                    var newAttr = new CustomAttribute(module.ImportReference(constructor));
                    for (int i = 0; i < argTypes.Length; i++)
                        newAttr.ConstructorArguments.Add(new CustomAttributeArgument(argTypes[i], attribute.ConstructorArguments[i].Value));
                    foreach (var prop in attribute.Properties)
                        newAttr.Properties.Add(new CustomAttributeNamedArgument(prop.Name, prop.Argument));
                    foreach (var field in attribute.Fields)
                        newAttr.Fields.Add(new CustomAttributeNamedArgument(field.Name, field.Argument));

                    // swap attribute
                    attributes[attrIndex] = newAttr;
                    rewritten = true;
                }
            }

            return rewritten;
        }
    }
}
