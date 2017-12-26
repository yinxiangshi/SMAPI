using System;

namespace StardewModdingAPI
{
    /// <summary>Provides an API for accessing inaccessible code.</summary>
    public interface IReflectionHelper : IModLinked
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get an instance field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="obj">The object which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the field is not found.</param>
        IReflectedField<TValue> GetField<TValue>(object obj, string name, bool required = true);

        /// <summary>Get a static field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="type">The type which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the field is not found.</param>
        IReflectedField<TValue> GetField<TValue>(Type type, string name, bool required = true);

        /// <summary>Get an instance property.</summary>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="obj">The object which has the property.</param>
        /// <param name="name">The property name.</param>
        /// <param name="required">Whether to throw an exception if the property is not found.</param>
        IReflectedProperty<TValue> GetProperty<TValue>(object obj, string name, bool required = true);

        /// <summary>Get a static property.</summary>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="type">The type which has the property.</param>
        /// <param name="name">The property name.</param>
        /// <param name="required">Whether to throw an exception if the property is not found.</param>
        IReflectedProperty<TValue> GetProperty<TValue>(Type type, string name, bool required = true);

        /// <summary>Get an instance method.</summary>
        /// <param name="obj">The object which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the field is not found.</param>
        IReflectedMethod GetMethod(object obj, string name, bool required = true);

        /// <summary>Get a static method.</summary>
        /// <param name="type">The type which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the field is not found.</param>
        IReflectedMethod GetMethod(Type type, string name, bool required = true);

        /*****
        ** Obsolete
        *****/
        /// <summary>Get a private instance field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="obj">The object which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        [Obsolete("Use " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetField) + " instead")]
        IPrivateField<TValue> GetPrivateField<TValue>(object obj, string name, bool required = true);

        /// <summary>Get a private static field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="type">The type which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        [Obsolete("Use " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetField) + " instead")]
        IPrivateField<TValue> GetPrivateField<TValue>(Type type, string name, bool required = true);

        /// <summary>Get a private instance property.</summary>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="obj">The object which has the property.</param>
        /// <param name="name">The property name.</param>
        /// <param name="required">Whether to throw an exception if the private property is not found.</param>
        [Obsolete("Use " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetProperty) + " instead")]
        IPrivateProperty<TValue> GetPrivateProperty<TValue>(object obj, string name, bool required = true);

        /// <summary>Get a private static property.</summary>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="type">The type which has the property.</param>
        /// <param name="name">The property name.</param>
        /// <param name="required">Whether to throw an exception if the private property is not found.</param>
        [Obsolete("Use " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetProperty) + " instead")]
        IPrivateProperty<TValue> GetPrivateProperty<TValue>(Type type, string name, bool required = true);

        /// <summary>Get the value of a private instance field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="obj">The object which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        /// <remarks>This is a shortcut for <see cref="GetPrivateField{TValue}(object,string,bool)"/> followed by <see cref="IPrivateField{TValue}.GetValue"/>.</remarks>
        [Obsolete("Use " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetField) + " or " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetProperty) + " instead")]
        TValue GetPrivateValue<TValue>(object obj, string name, bool required = true);

        /// <summary>Get the value of a private static field.</summary>
        /// <typeparam name="TValue">The field type.</typeparam>
        /// <param name="type">The type which has the field.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        /// <remarks>This is a shortcut for <see cref="GetPrivateField{TValue}(Type,string,bool)"/> followed by <see cref="IPrivateField{TValue}.GetValue"/>.</remarks>
        [Obsolete("Use " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetField) + " or " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetProperty) + " instead")]
        TValue GetPrivateValue<TValue>(Type type, string name, bool required = true);

        /// <summary>Get a private instance method.</summary>
        /// <param name="obj">The object which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        [Obsolete("Use " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetMethod) + " instead")]
        IPrivateMethod GetPrivateMethod(object obj, string name, bool required = true);

        /// <summary>Get a private static method.</summary>
        /// <param name="type">The type which has the method.</param>
        /// <param name="name">The field name.</param>
        /// <param name="required">Whether to throw an exception if the private field is not found.</param>
        [Obsolete("Use " + nameof(IReflectionHelper) + "." + nameof(IReflectionHelper.GetMethod) + " instead")]
        IPrivateMethod GetPrivateMethod(Type type, string name, bool required = true);
    }
}
