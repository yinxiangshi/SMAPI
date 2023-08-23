using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Framework;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters
{
    /// <summary>Rewrites references to types or type members to point to a different type or member.</summary>
    /// <remarks>
    ///   This supports mapping...
    ///   <list type="bullet">
    ///     <item>types to another type;</item>
    ///     <item>constructors, methods, fields, and properties to their exact equivalents on another type;</item>
    ///     <item>and fields to properties with the same type and name (either on the same or different type).</item>
    ///   </list>
    /// </remarks>
    internal class ReplaceReferencesRewriter : BaseInstructionHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>The new types to reference, indexed by the old type's full name.</summary>
        private readonly Dictionary<string, Type> TypeMap = new();

        /// <summary>The new members to reference, indexed by the old member's full name.</summary>
        private readonly Dictionary<string, MemberInfo> MemberMap = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ReplaceReferencesRewriter()
            : base(defaultPhrase: "type or member reference") { } // overridden by this.Phrases when a reference is replaced

        /****
        ** Management
        ****/
        /// <summary>Rewrite type references to point to another type.</summary>
        /// <param name="fromFullName">The full type name, like <c>Microsoft.Xna.Framework.Vector2</c>.</param>
        /// <param name="toType">The new type to reference.</param>
        public ReplaceReferencesRewriter MapType(string fromFullName, Type toType)
        {
            // validate parameters
            if (string.IsNullOrWhiteSpace(fromFullName))
                throw new ArgumentException("Can't replace a type given an empty name.", nameof(fromFullName));
            if (toType is null)
                throw new ArgumentException("Can't replace a type given a null target type.", nameof(toType));

            // add mapping
            if (!this.TypeMap.TryAdd(fromFullName, toType))
                throw new InvalidOperationException($"The '{fromFullName}' type is already mapped.");

            return this;
        }

        /// <summary>Rewrite field references to point to another field with the same field type.</summary>
        /// <param name="fromFullName">The full field name, like <c>Microsoft.Xna.Framework.Vector2 StardewValley.Character::Tile</c>.</param>
        /// <param name="toType">The new type which will have the field.</param>
        /// <param name="toName">The new field name to reference.</param>
        public ReplaceReferencesRewriter MapField(string fromFullName, Type toType, string toName)
        {
            // validate parameters
            if (string.IsNullOrWhiteSpace(fromFullName))
                throw new ArgumentException("Can't replace a field given an empty name.", nameof(fromFullName));
            if (toType is null)
                throw new ArgumentException("Can't replace a field given a null target type.", nameof(toType));
            if (string.IsNullOrWhiteSpace(toName))
                throw new ArgumentException("Can't replace a field given an empty target name.", nameof(toType));

            // get field
            FieldInfo? toField;
            try
            {
                toField = toType.GetField(toName);
                if (toField is null)
                    throw new InvalidOperationException($"Required field {toType.FullName}::{toName} could not be loaded.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Required field {toType.FullName}::{toName} could not be loaded.", ex);
            }

            // add mapping
            return this.MapMember(fromFullName, toField, "field");
        }

        /// <summary>Rewrite field references to point to a property with the same return type.</summary>
        /// <param name="fromFullName">The full field name, like <c>Microsoft.Xna.Framework.Vector2 StardewValley.Character::Tile</c>.</param>
        /// <param name="toType">The new type which will have the field.</param>
        /// <param name="toName">The new field name to reference.</param>
        public ReplaceReferencesRewriter MapFieldToProperty(string fromFullName, Type toType, string toName)
        {
            // validate parameters
            if (string.IsNullOrWhiteSpace(fromFullName))
                throw new ArgumentException("Can't replace a field given an empty name.", nameof(fromFullName));
            if (toType is null)
                throw new ArgumentException("Can't replace a field given a null target type.", nameof(toType));
            if (string.IsNullOrWhiteSpace(toName))
                throw new ArgumentException("Can't replace a field given an empty target name.", nameof(toType));

            // get field
            PropertyInfo? toProperty;
            try
            {
                toProperty = toType.GetProperty(toName);
                if (toProperty is null)
                    throw new InvalidOperationException($"Required property {toType.FullName}::{toName} could not be loaded.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Required property {toType.FullName}::{toName} could not be loaded.", ex);
            }

            // add mapping
            return this.MapMember(fromFullName, toProperty, "field-to-property");
        }

        /// <summary>Rewrite method references to point to another method with the same signature.</summary>
        /// <param name="fromFullName">The full method name, like <c>Microsoft.Xna.Framework.Vector2 StardewValley.Character::getTileLocation()</c>.</param>
        /// <param name="toType">The new type which will have the method.</param>
        /// <param name="toName">The new method name to reference.</param>
        /// <param name="parameterTypes">The method's parameter types to disambiguate between overloads, if needed.</param>
        public ReplaceReferencesRewriter MapMethod(string fromFullName, Type toType, string toName, Type[]? parameterTypes = null)
        {
            // validate parameters
            if (string.IsNullOrWhiteSpace(fromFullName))
                throw new ArgumentException("Can't replace a field given an empty name.", nameof(fromFullName));
            if (toType is null)
                throw new ArgumentException("Can't replace a field given a null target type.", nameof(toType));
            if (string.IsNullOrWhiteSpace(toName))
                throw new ArgumentException("Can't replace a field given an empty target name.", nameof(toType));

            // get method
            MethodInfo? method;
            try
            {
                method = parameterTypes != null
                    ? toType.GetMethod(toName, parameterTypes)
                    : toType.GetMethod(toName);
                if (method is null)
                    throw new InvalidOperationException($"Required method {toType.FullName}::{toName} could not be loaded.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Required method {toType.FullName}::{toName} could not be loaded.", ex);
            }

            // add mapping
            return this.MapMember(fromFullName, method, "method");
        }

        /// <summary>Rewrite field, property, constructor, and method references to point to a matching equivalent on another class.</summary>
        /// <typeparam name="TFromType">The type to which references should be rewritten.</typeparam>
        /// <typeparam name="TFacade">The facade type to which to point matching references.</typeparam>
        /// <param name="mapDefaultConstructor">If the facade has a public constructor with no parameters, whether to rewrite references to empty constructors to use that one. (This is needed because .NET has no way to distinguish between an implicit and explicit constructor.)</param>
        public ReplaceReferencesRewriter MapFacade<TFromType, TFacade>(bool mapDefaultConstructor = false)
        {
            return this.MapFacade(typeof(TFromType).FullName!, typeof(TFacade), mapDefaultConstructor);
        }

        /// <summary>Rewrite field, property, constructor, and method references to point to a matching equivalent on another class.</summary>
        /// <param name="fromTypeName">The full name of the type to which references should be rewritten.</param>
        /// <param name="toType">The facade type to which to point matching references.</param>
        /// <param name="mapDefaultConstructor">If the facade has a public constructor with no parameters, whether to rewrite references to empty constructors to use that one. (This is needed because .NET has no way to distinguish between an implicit and explicit constructor.)</param>
        public ReplaceReferencesRewriter MapFacade(string fromTypeName, Type toType, bool mapDefaultConstructor = false)
        {
            // properties
            foreach (PropertyInfo property in toType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                string propertyType = this.FormatCecilType(property.PropertyType);

                // add getter
                MethodInfo? get = property.GetMethod;
                if (get != null)
                    this.MapMember($"{propertyType} {fromTypeName}::get_{property.Name}()", get, "method");

                // add setter
                MethodInfo? set = property.SetMethod;
                if (set != null)
                    this.MapMember($"System.Void {fromTypeName}::set_{property.Name}({propertyType})", set, "method");

                // add field => property
                this.MapMember($"{propertyType} {fromTypeName}::{property.Name}", property, "field-to-property");
            }

            // methods
            foreach (MethodInfo method in toType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                    continue; // handled via properties above

                string fromFullName = $"{this.FormatCecilType(method.ReturnType)} {fromTypeName}::{method.Name}({this.FormatCecilParameterList(method.GetParameters())})";

                this.MapMember(fromFullName, method, "method");
            }

            // constructors
            ConstructorInfo[] constructors = toType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (ConstructorInfo constructor in constructors)
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                string fromFullName = $"System.Void {fromTypeName}::.ctor({this.FormatCecilParameterList(parameters)})";

                if (!mapDefaultConstructor && parameters.Length == 0)
                    continue;

                this.MapMember(fromFullName, constructor, "constructor");
            }

            return this;
        }

        /****
        ** Handlers
        ****/
        /// <inheritdoc />
        public override bool Handle(ModuleDefinition module, TypeReference type, Action<TypeReference> replaceWith)
        {
            if (this.TypeMap.TryGetValue(type.FullName, out Type? newType))
            {
                replaceWith(module.ImportReference(newType));

                this.Phrases.Add($"{type.FullName} type");
                return this.MarkRewritten();
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction)
        {
            if (instruction.Operand is not MemberReference fromMember || !this.MemberMap.TryGetValue(fromMember.FullName, out MemberInfo? toMember))
                return false;

            switch (toMember)
            {
                // constructor
                case ConstructorInfo toConstructor:
                    instruction.Operand = module.ImportReference(toConstructor);
                    return this.OnRewritten(fromMember, "constructor");

                // method
                case MethodInfo toMethod:
                    instruction.Operand = module.ImportReference(toMethod);
                    return this.OnRewritten(fromMember, "method");

                // field
                case FieldInfo toField:
                    instruction.Operand = module.ImportReference(toField);
                    return this.OnRewritten(fromMember, "field");

                // field to property
                // (property-to-property is handled as a method)
                case PropertyInfo toProperty:
                    {
                        MethodInfo? toPropMethod = null;
                        if (instruction.OpCode == OpCodes.Ldfld || instruction.OpCode == OpCodes.Ldsfld)
                            toPropMethod = toProperty.GetMethod;
                        else if (instruction.OpCode == OpCodes.Stfld || instruction.OpCode == OpCodes.Stsfld)
                            toPropMethod = toProperty.SetMethod;

                        if (toPropMethod != null)
                        {
                            instruction.OpCode = OpCodes.Call;
                            instruction.Operand = module.ImportReference(toPropMethod);
                            return this.OnRewritten(fromMember, "field");
                        }
                    }
                    break;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Add a member to replace.</summary>
        /// <param name="fromFullName">The full member name, like <c>Microsoft.Xna.Framework.Vector2 StardewValley.Character::getTileLocation()</c>.</param>
        /// <param name="toMember">The new member to reference.</param>
        /// <param name="typeLabel">A human-readable label for the reference type, like 'field' or 'method'.</param>
        private ReplaceReferencesRewriter MapMember(string fromFullName, MemberInfo toMember, string typeLabel)
        {
            // validate parameters
            if (string.IsNullOrWhiteSpace(fromFullName))
                throw new ArgumentException($"Can't replace a {typeLabel} given an empty name.", nameof(fromFullName));
            if (toMember is null)
                throw new InvalidOperationException($"The replacement {typeLabel} for '{fromFullName}' can't be null.");

            // add mapping
            if (!this.MemberMap.TryAdd(fromFullName, toMember))
                throw new InvalidOperationException($"The '{fromFullName}' {typeLabel} is already mapped.");

            return this;
        }

        /// <summary>Update the rewriter state after an reference is replaced.</summary>
        /// <param name="fromMember">The previous member reference.</param>
        /// <param name="typeLabel">A human-readable label for the reference type, like 'field' or 'method'.</param>
        public bool OnRewritten(MemberReference fromMember, string typeLabel)
        {
            this.Phrases.Add($"{fromMember.DeclaringType!.Name}.{fromMember.Name} {typeLabel}");
            return this.MarkRewritten();
        }

        /// <summary>Get a formatted type name in the Cecil full-method-name format.</summary>
        /// <param name="type">The type to format.</param>
        private string FormatCecilType(Type type)
        {
            return RewriteHelper.GetFullCecilName(type);
        }

        /// <summary>Get a formatted parameter list in the Cecil full-method-name format.</summary>
        /// <param name="parameters">The parameter list to format.</param>
        private string FormatCecilParameterList(ParameterInfo[] parameters)
        {
            var paramTypes = parameters.Select(p => RewriteHelper.GetFullCecilName(p.ParameterType));
            return string.Join(",", paramTypes);
        }
    }
}
