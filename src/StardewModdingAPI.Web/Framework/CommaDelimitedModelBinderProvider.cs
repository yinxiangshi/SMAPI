using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StardewModdingAPI.Web.Framework
{
    /// <summary>Provides comma-delimited model binds for mapping parameters.</summary>
    /// <remarks>Derived from <a href="https://stackoverflow.com/a/43655986/262123" />.</remarks>
    public class CommaDelimitedModelBinderProvider : IModelBinderProvider
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Creates a model binder based on the given context.</summary>
        /// <param name="context">The model binding context.</param>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            // validate
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // get model binder
            return context.Metadata.IsEnumerableType && !context.Metadata.ElementMetadata.IsComplexType
                ? new CommaDelimitedModelBinder()
                : null;
        }
    }
}
