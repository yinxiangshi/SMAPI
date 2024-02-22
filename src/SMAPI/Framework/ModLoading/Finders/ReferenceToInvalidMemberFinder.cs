using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Framework;

namespace StardewModdingAPI.Framework.ModLoading.Finders
{
    /// <summary>Finds references to a field, property, or method which either doesn't exist or returns a different type than the code expects.</summary>
    /// <remarks>This implementation is purely heuristic. It should never return a false positive, but won't detect all cases.</remarks>
    internal class ReferenceToInvalidMemberFinder : BaseInstructionHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>The assembly names to which to heuristically detect broken references.</summary>
        private readonly ISet<string> ValidateReferencesToAssemblies;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="validateReferencesToAssemblies">The assembly names to which to heuristically detect broken references.</param>
        public ReferenceToInvalidMemberFinder(ISet<string> validateReferencesToAssemblies)
            : base(defaultPhrase: "")
        {
            this.ValidateReferencesToAssemblies = validateReferencesToAssemblies;
        }

        /// <inheritdoc />
        public override bool Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction)
        {
            // field reference
            FieldReference? fieldRef = RewriteHelper.AsFieldReference(instruction);
            if (fieldRef != null && this.ShouldValidate(fieldRef.DeclaringType))
            {
                FieldDefinition? targetField = fieldRef.DeclaringType.Resolve()?.Fields.FirstOrDefault(p => p.Name == fieldRef.Name);

                // wrong return type
                if (targetField != null && !RewriteHelper.LooksLikeSameType(fieldRef.FieldType, targetField.FieldType))
                    this.MarkFlag(InstructionHandleResult.NotCompatible, $"reference to {fieldRef.DeclaringType.FullName}.{fieldRef.Name} (field returns {this.GetFriendlyTypeName(targetField.FieldType)}, not {this.GetFriendlyTypeName(fieldRef.FieldType)})");

                // missing
                else if (targetField == null || targetField.HasConstant || !RewriteHelper.HasSameNamespaceAndName(fieldRef.DeclaringType, targetField.DeclaringType))
                    this.MarkFlag(InstructionHandleResult.NotCompatible, $"reference to {fieldRef.DeclaringType.FullName}.{fieldRef.Name} (no such field)");

                return false;
            }

            // method reference
            MethodReference? methodRef = RewriteHelper.AsMethodReference(instruction);
            if (methodRef != null && !this.IsUnsupported(methodRef))
            {
                MethodDefinition? methodDef = methodRef.Resolve();

                // wrong return type
                if (methodDef != null && this.ShouldValidate(methodRef.DeclaringType))
                {
                    MethodDefinition[]? candidateMethods = methodRef.DeclaringType.Resolve()?.Methods.Where(found => found.Name == methodRef.Name).ToArray();
                    if (candidateMethods?.Any() is true && candidateMethods.All(method => !RewriteHelper.LooksLikeSameType(method.ReturnType, methodDef.ReturnType)))
                        this.MarkFlag(InstructionHandleResult.NotCompatible, $"reference to {methodDef.DeclaringType.FullName}.{methodDef.Name} (no such method returns {this.GetFriendlyTypeName(methodDef.ReturnType)})");
                }

                // missing
                else if (methodDef is null)
                {
                    string phrase;
                    if (this.IsProperty(methodRef))
                        phrase = $"reference to {methodRef.DeclaringType.FullName}.{methodRef.Name.Substring(4)} (no such property)";
                    else if (methodRef.Name == ".ctor")
                        phrase = $"reference to {methodRef.DeclaringType.FullName}.{methodRef.Name} (no matching constructor)";
                    else
                        phrase = $"reference to {methodRef.DeclaringType.FullName}.{methodRef.Name} (no such method)";

                    this.MarkFlag(InstructionHandleResult.NotCompatible, phrase);
                }
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Whether references to the given type should be validated.</summary>
        /// <param name="type">The type reference.</param>
        private bool ShouldValidate([NotNullWhen(true)] TypeReference? type)
        {
            return type != null && this.ValidateReferencesToAssemblies.Contains(type.Scope.Name);
        }

        /// <summary>Get whether a method reference is a special case that's not currently supported (e.g. array methods).</summary>
        /// <param name="method">The method reference.</param>
        private bool IsUnsupported(MethodReference method)
        {
            return
                method.DeclaringType.Name.Contains("["); // array methods
        }

        /// <summary>Get a shorter type name for display.</summary>
        /// <param name="type">The type reference.</param>
        private string GetFriendlyTypeName(TypeReference type)
        {
            // most common built-in types
            switch (type.FullName)
            {
                case "System.Boolean":
                    return "bool";
                case "System.Int32":
                    return "int";
                case "System.String":
                    return "string";
            }

            // most common unambiguous namespaces
            foreach (string @namespace in new[] { "Microsoft.Xna.Framework", "Netcode", "System", "System.Collections.Generic" })
            {
                if (type.Namespace == @namespace)
                    return type.Name;
            }

            return type.FullName;
        }

        /// <summary>Get whether a method reference is a property getter or setter.</summary>
        /// <param name="method">The method reference.</param>
        private bool IsProperty(MethodReference method)
        {
            return method.Name.StartsWith("get_") || method.Name.StartsWith("set_");
        }
    }
}
