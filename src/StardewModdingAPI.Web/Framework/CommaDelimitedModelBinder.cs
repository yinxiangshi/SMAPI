using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StardewModdingAPI.Web.Framework
{
    /// <summary>Maps comma-delimited values to an <see cref="System.Collections.Generic.IEnumerable{T}"/> parameter.</summary>
    /// <remarks>Derived from <a href="https://stackoverflow.com/a/43655986/262123" />.</remarks>
    public class CommaDelimitedModelBinder : IModelBinder
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Attempts to bind a model.</summary>
        /// <param name="bindingContext">The model binding context.</param>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // validate
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            // extract values
            string modelName = bindingContext.ModelName;
            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            string[] values = valueProviderResult
                .ToString()
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Type elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            if (values.Length == 0)
            {
                bindingContext.Result = ModelBindingResult.Success(Array.CreateInstance(elementType, 0));
                return Task.CompletedTask;
            }

            // map values
            TypeConverter converter = TypeDescriptor.GetConverter(elementType);
            Array typedArray = Array.CreateInstance(elementType, values.Length);
            try
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    string value = values[i];
                    object convertedValue = converter.ConvertFromString(value);
                    typedArray.SetValue(convertedValue, i);
                }
            }
            catch (Exception exception)
            {
                bindingContext.ModelState.TryAddModelError(modelName, exception, bindingContext.ModelMetadata);
            }

            bindingContext.Result = ModelBindingResult.Success(typedArray);
            return Task.CompletedTask;
        }
    }
}
