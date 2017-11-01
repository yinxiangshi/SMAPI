using System;
using System.Reflection;
using StardewModdingAPI.Framework.Reflection;

namespace StardewModdingAPI.Framework.ModHelpers
{
    /// <summary>Provides helper methods for accessing private game code.</summary>
    /// <remarks>This implementation searches up the type hierarchy, and caches the reflected fields and methods with a sliding expiry (to optimise performance without unnecessary memory usage).</remarks>
    internal class ReflectionHelper : BaseHelper, IReflectionHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying reflection helper.</summary>
        private readonly Reflector Reflector;

        /// <summary>The mod name for error messages.</summary>
        private readonly string ModName;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modID">The unique ID of the relevant mod.</param>
        /// <param name="modName">The mod name for error messages.</param>
        /// <param name="reflector">The underlying reflection helper.</param>
        public ReflectionHelper(string modID, string modName, Reflector reflector)
            : base(modID)
        {
            this.ModName = modName;
            this.Reflector = reflector;
        }

        /****
        ** Fields
        ****/
        /// <summary>Get a private instance field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="obj">The object which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        /// <returns>Returns the field wrapper, or <c>null</c> if the field doesn't exist and <paramref name="required"/> is <c>false</c>.</returns>
        public IPrivateField<TValue> GetPrivateField<TValue>(object obj, string name, bool required = true)
        {
            return this.AssertAccessAllowed(
                this.Reflector.GetPrivateField<TValue>(obj, name, required)
            );
        }

        /// <summary>Get a private static field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="type">The type which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public IPrivateField<TValue> GetPrivateField<TValue>(Type type, string name, bool required = true)
        {
            return this.AssertAccessAllowed(
                this.Reflector.GetPrivateField<TValue>(type, name, required)
            );
        }

        /****
        ** Properties
        ****/
        /// <summary>Get a private instance property.</summary>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="obj">The object which has the property.</param>
        /// <param name="name">The property name.</param>
        /// <param name="required">Whether to throw an exception if the private property is not found.</param>
        public IPrivateProperty<TValue> GetPrivateProperty<TValue>(object obj, string name, bool required = true)
        {
            return this.AssertAccessAllowed(
                this.Reflector.GetPrivateProperty<TValue>(obj, name, required)
               );
        }

        /// <summary>Get a private static property.</summary>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="type">The type which has the property.</param>
        /// <param name="name">The property name.</param>
        /// <param name="required">Whether to throw an exception if the private property is not found.</param>
        public IPrivateProperty<TValue> GetPrivateProperty<TValue>(Type type, string name, bool required = true)
        {
            return this.AssertAccessAllowed(
                this.Reflector.GetPrivateProperty<TValue>(type, name, required)
            );
        }

        /****
        ** Field values
        ** (shorthand since this is the most common case)
        ****/
        /// <summary>Get the value of a private instance field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="obj">The object which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        /// <returns>Returns the field value, or the default value for <typeparamref name="TValue"/> if the field wasn't found and <paramref name="required"/> is false.</returns>
        /// <remarks>
        /// This is a shortcut for <see cref="GetPrivateField{TValue}(object,string,bool)"/> followed by <see cref="IPrivateField{TValue}.GetValue"/>.
        /// When <paramref name="required" /> is false, this will return the default value if reflection fails. If you need to check whether the field exists, use <see cref="GetPrivateField{TValue}(object,string,bool)" /> instead.
        /// </remarks>
        public TValue GetPrivateValue<TValue>(object obj, string name, bool required = true)
        {
            IPrivateField<TValue> field = this.GetPrivateField<TValue>(obj, name, required);
            return field != null
                ? field.GetValue()
                : default(TValue);
        }

        /// <summary>Get the value of a private static field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="type">The type which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        /// <returns>Returns the field value, or the default value for <typeparamref name="TValue"/> if the field wasn't found and <paramref name="required"/> is false.</returns>
        /// <remarks>
        /// This is a shortcut for <see cref="GetPrivateField{TValue}(Type,string,bool)"/> followed by <see cref="IPrivateField{TValue}.GetValue"/>.
        /// When <paramref name="required" /> is false, this will return the default value if reflection fails. If you need to check whether the field exists, use <see cref="GetPrivateField{TValue}(Type,string,bool)" /> instead.
        /// </remarks>
        public TValue GetPrivateValue<TValue>(Type type, string name, bool required = true)
        {
            IPrivateField<TValue> field = this.GetPrivateField<TValue>(type, name, required);
            return field != null
                ? field.GetValue()
                : default(TValue);
        }

        /****
        ** Methods
        ****/
        /// <summary>Get a private instance method.</summary>
        /// <param name="obj">The object which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public IPrivateMethod GetPrivateMethod(object obj, string name, bool required = true)
        {
            return this.AssertAccessAllowed(
                this.Reflector.GetPrivateMethod(obj, name, required)
            );
        }

        /// <summary>Get a private static method.</summary>
        /// <param name="type">The type which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public IPrivateMethod GetPrivateMethod(Type type, string name, bool required = true)
        {
            return this.AssertAccessAllowed(
                this.Reflector.GetPrivateMethod(type, name, required)
            );
        }

        /****
        ** Methods by signature
        ****/
        /// <summary>Get a private instance method.</summary>
        /// <param name="obj">The object which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="argumentTypes">The argument types of the method signature to find.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public IPrivateMethod GetPrivateMethod(object obj, string name, Type[] argumentTypes, bool required = true)
        {
            return this.AssertAccessAllowed(
                this.Reflector.GetPrivateMethod(obj, name, argumentTypes, required)
            );
        }

        /// <summary>Get a private static method.</summary>
        /// <param name="type">The type which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="argumentTypes">The argument types of the method signature to find.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public IPrivateMethod GetPrivateMethod(Type type, string name, Type[] argumentTypes, bool required = true)
        {
            return this.AssertAccessAllowed(
                this.Reflector.GetPrivateMethod(type, name, argumentTypes, required)
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Assert that mods can use the reflection helper to access the given member.</summary>
        /// <typeparam name="T">The field value type.</typeparam>
        /// <param name="field">The field being accessed.</param>
        /// <returns>Returns the same field instance for convenience.</returns>
        private IPrivateField<T> AssertAccessAllowed<T>(IPrivateField<T> field)
        {
            this.AssertAccessAllowed(field?.FieldInfo);
            return field;
        }

        /// <summary>Assert that mods can use the reflection helper to access the given member.</summary>
        /// <typeparam name="T">The property value type.</typeparam>
        /// <param name="property">The property being accessed.</param>
        /// <returns>Returns the same property instance for convenience.</returns>
        private IPrivateProperty<T> AssertAccessAllowed<T>(IPrivateProperty<T> property)
        {
            this.AssertAccessAllowed(property?.PropertyInfo);
            return property;
        }

        /// <summary>Assert that mods can use the reflection helper to access the given member.</summary>
        /// <param name="method">The method being accessed.</param>
        /// <returns>Returns the same method instance for convenience.</returns>
        private IPrivateMethod AssertAccessAllowed(IPrivateMethod method)
        {
            this.AssertAccessAllowed(method?.MethodInfo);
            return method;
        }

        /// <summary>Assert that mods can use the reflection helper to access the given member.</summary>
        /// <param name="member">The member being accessed.</param>
        private void AssertAccessAllowed(MemberInfo member)
        {
            if (member == null)
                return;

            // get type which defines the member
            Type declaringType = member.DeclaringType;
            if (declaringType == null)
                throw new InvalidOperationException($"Can't validate access to {member.MemberType} {member.Name} because it has no declaring type."); // should never happen

            // validate access
            string rootNamespace = typeof(Program).Namespace;
            if (declaringType.Namespace == rootNamespace || declaringType.Namespace?.StartsWith(rootNamespace + ".") == true)
                throw new InvalidOperationException($"SMAPI blocked access by {this.ModName} to its internals through the reflection API. Accessing the SMAPI internals is strongly discouraged since they're subject to change, which means the mod can break without warning. (Detected access to {declaringType.FullName}.{member.Name}.)");
        }
    }
}
