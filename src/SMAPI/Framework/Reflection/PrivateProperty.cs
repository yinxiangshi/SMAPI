using System;
using System.Reflection;

namespace StardewModdingAPI.Framework.Reflection
{
    /// <summary>A private property obtained through reflection.</summary>
    /// <typeparam name="TValue">The property value type.</typeparam>
    internal class PrivateProperty<TValue> : IPrivateProperty<TValue>
    {
        /*********
        ** Properties
        *********/
        /// <summary>The display name shown in error messages.</summary>
        private readonly string DisplayName;

        /// <summary>The underlying property getter.</summary>
        private readonly Func<TValue> GetMethod;

        /// <summary>The underlying property setter.</summary>
        private readonly Action<TValue> SetMethod;


        /*********
        ** Accessors
        *********/
        /// <summary>The reflection metadata.</summary>
        public PropertyInfo PropertyInfo { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="parentType">The type that has the field.</param>
        /// <param name="obj">The object that has the instance field (if applicable).</param>
        /// <param name="property">The reflection metadata.</param>
        /// <param name="isStatic">Whether the field is static.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="parentType"/> or <paramref name="property"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is null for a non-static field, or not null for a static field.</exception>
        public PrivateProperty(Type parentType, object obj, PropertyInfo property, bool isStatic)
        {
            // validate input
            if (parentType == null)
                throw new ArgumentNullException(nameof(parentType));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            // validate static
            if (isStatic && obj != null)
                throw new ArgumentException("A static property cannot have an object instance.");
            if (!isStatic && obj == null)
                throw new ArgumentException("A non-static property must have an object instance.");


            this.DisplayName = $"{parentType.FullName}::{property.Name}";
            this.PropertyInfo = property;

            if (this.PropertyInfo.GetMethod != null)
                this.GetMethod = (Func<TValue>)Delegate.CreateDelegate(typeof(Func<TValue>), obj, this.PropertyInfo.GetMethod);
            if (this.PropertyInfo.SetMethod != null)
                this.SetMethod = (Action<TValue>)Delegate.CreateDelegate(typeof(Action<TValue>), obj, this.PropertyInfo.SetMethod);
        }

        /// <summary>Get the property value.</summary>
        public TValue GetValue()
        {
            if (this.GetMethod == null)
                throw new InvalidOperationException($"The private {this.DisplayName} property has no get method.");

            try
            {
                return this.GetMethod();
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Can't convert the private {this.DisplayName} property from {this.PropertyInfo.PropertyType.FullName} to {typeof(TValue).FullName}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't get the value of the private {this.DisplayName} property", ex);
            }
        }

        /// <summary>Set the property value.</summary>
        //// <param name="value">The value to set.</param>
        public void SetValue(TValue value)
        {
            if (this.SetMethod == null)
                throw new InvalidOperationException($"The private {this.DisplayName} property has no set method.");

            try
            {
                this.SetMethod(value);
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Can't assign the private {this.DisplayName} property a {typeof(TValue).FullName} value, must be compatible with {this.PropertyInfo.PropertyType.FullName}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't set the value of the private {this.DisplayName} property", ex);
            }
        }
    }
}
