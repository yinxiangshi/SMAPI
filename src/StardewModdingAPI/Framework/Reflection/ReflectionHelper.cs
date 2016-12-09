using System;
using System.Reflection;
using StardewModdingAPI.Reflection;

namespace StardewModdingAPI.Framework.Reflection
{
    /// <summary>Provides helper methods for accessing private game code.</summary>
    internal class ReflectionHelper : IReflectionHelper
    {
        /*********
        ** Public methods
        *********/
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
            // validate
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Can't get a private instance field from a null object.");

            // get field from hierarchy
            IPrivateField<TValue> field = this.GetFieldFromHierarchy<TValue>(obj.GetType(), obj, name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (required && field == null)
                throw new InvalidOperationException($"The {obj.GetType().FullName} object doesn't have a private '{name}' instance field.");
            return field;
        }

        /// <summary>Get a private static field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="type">The type which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public IPrivateField<TValue> GetPrivateField<TValue>(Type type, string name, bool required = true)
        {
            // get field from hierarchy
            IPrivateField<TValue> field = this.GetFieldFromHierarchy<TValue>(type, null, name, BindingFlags.NonPublic | BindingFlags.Static);
            if (required && field == null)
                throw new InvalidOperationException($"The {type.FullName} object doesn't have a private '{name}' static field.");
            return field;
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
        /// <remarks>This is a shortcut for <see cref="GetPrivateField{TValue}(object,string,bool)"/> followed by <see cref="IPrivateField{TValue}.GetValue"/>.</remarks>
        public TValue GetPrivateValue<TValue>(object obj, string name, bool required = true)
        {
            return this.GetPrivateField<TValue>(obj, name, required).GetValue();
        }

        /// <summary>Get the value of a private static field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="type">The type which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        /// <remarks>This is a shortcut for <see cref="GetPrivateField{TValue}(Type,string,bool)"/> followed by <see cref="IPrivateField{TValue}.GetValue"/>.</remarks>
        public TValue GetPrivateValue<TValue>(Type type, string name, bool required = true)
        {
            return this.GetPrivateField<TValue>(type, name, required).GetValue();
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
            // validate
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Can't get a private instance method from a null object.");

            // get method from hierarchy
            IPrivateMethod method = this.GetMethodFromHierarchy(obj.GetType(), obj, name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (required && method == null)
                throw new InvalidOperationException($"The {obj.GetType().FullName} object doesn't have a private '{name}' instance method.");
            return method;
        }

        /// <summary>Get a private static method.</summary>
        /// <param name="type">The type which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public IPrivateMethod GetPrivateMethod(Type type, string name, bool required = true)
        {
            // get method from hierarchy
            IPrivateMethod method = this.GetMethodFromHierarchy(type, null, name, BindingFlags.NonPublic | BindingFlags.Static);
            if (required && method == null)
                throw new InvalidOperationException($"The {type.FullName} object doesn't have a private '{name}' static method.");
            return method;
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
            // validate parent
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Can't get a private instance method from a null object.");

            // get method from hierarchy
            PrivateMethod method = this.GetMethodFromHierarchy(obj.GetType(), obj, name, BindingFlags.Instance | BindingFlags.NonPublic, argumentTypes);
            if (required && method == null)
                throw new InvalidOperationException($"The {obj.GetType().FullName} object doesn't have a private '{name}' instance method with that signature.");
            return method;
        }

        /// <summary>Get a private static method.</summary>
        /// <param name="type">The type which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="argumentTypes">The argument types of the method signature to find.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        public IPrivateMethod GetPrivateMethod(Type type, string name, Type[] argumentTypes, bool required = true)
        {
            // get field from hierarchy
            PrivateMethod method = this.GetMethodFromHierarchy(type, null, name, BindingFlags.NonPublic | BindingFlags.Static, argumentTypes);
            if (required && method == null)
                throw new InvalidOperationException($"The {type.FullName} object doesn't have a private '{name}' static method with that signature.");
            return method;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a field from the type hierarchy.</summary>
        /// <typeparam name="TValue">The expected field type.</typeparam>
        /// <param name="type">The type which has the field.</param>
        /// <param name="obj">The object which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="bindingFlags">The reflection binding which flags which indicates what type of field to find.</param>
        private IPrivateField<TValue> GetFieldFromHierarchy<TValue>(Type type, object obj, string name, BindingFlags bindingFlags)
        {
            FieldInfo field = null;
            for (; type != null && field == null; type = type.BaseType)
                field = type.GetField(name, bindingFlags);

            return field != null
                ? new PrivateField<TValue>(type, obj, field, isStatic: bindingFlags.HasFlag(BindingFlags.Static))
                : null;
        }

        /// <summary>Get a method from the type hierarchy.</summary>
        /// <param name="type">The type which has the method.</param>
        /// <param name="obj">The object which has the method.</param>
        /// <param name="name">The method name.</param>
        /// <param name="bindingFlags">The reflection binding which flags which indicates what type of method to find.</param>
        private IPrivateMethod GetMethodFromHierarchy(Type type, object obj, string name, BindingFlags bindingFlags)
        {
            MethodInfo method = null;
            for (; type != null && method == null; type = type.BaseType)
                method = type.GetMethod(name, bindingFlags);

            return method != null
                ? new PrivateMethod(type, obj, method, isStatic: bindingFlags.HasFlag(BindingFlags.Static))
                : null;
        }

        /// <summary>Get a method from the type hierarchy.</summary>
        /// <param name="type">The type which has the method.</param>
        /// <param name="obj">The object which has the method.</param>
        /// <param name="name">The method name.</param>
        /// <param name="bindingFlags">The reflection binding which flags which indicates what type of method to find.</param>
        /// <param name="argumentTypes">The argument types of the method signature to find.</param>
        private PrivateMethod GetMethodFromHierarchy(Type type, object obj, string name, BindingFlags bindingFlags, Type[] argumentTypes)
        {
            MethodInfo method = null;
            for (; type != null && method == null; type = type.BaseType)
                method = type.GetMethod(name, bindingFlags, null, argumentTypes, null);

            return method != null
                ? new PrivateMethod(type, obj, method, isStatic: bindingFlags.HasFlag(BindingFlags.Static))
                : null;
        }
    }
}