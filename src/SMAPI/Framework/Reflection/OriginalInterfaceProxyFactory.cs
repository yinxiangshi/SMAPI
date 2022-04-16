using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace StardewModdingAPI.Framework.Reflection
{
    /// <inheritdoc />
    internal class OriginalInterfaceProxyFactory : IInterfaceProxyFactory
    {
        /*********
        ** Fields
        *********/
        /// <summary>The CLR module in which to create proxy classes.</summary>
        private readonly ModuleBuilder ModuleBuilder;

        /// <summary>The generated proxy types.</summary>
        private readonly IDictionary<string, OriginalInterfaceProxyBuilder> Builders = new Dictionary<string, OriginalInterfaceProxyBuilder>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public OriginalInterfaceProxyFactory()
        {
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"StardewModdingAPI.Proxies, Version={this.GetType().Assembly.GetName().Version}, Culture=neutral"), AssemblyBuilderAccess.Run);
            this.ModuleBuilder = assemblyBuilder.DefineDynamicModule("StardewModdingAPI.Proxies");
        }

        /// <inheritdoc />
        public TInterface CreateProxy<TInterface>(object instance, string sourceModID, string targetModID)
            where TInterface : class
        {
            lock (this.Builders)
            {
                // validate
                if (instance == null)
                    throw new InvalidOperationException("Can't proxy access to a null API.");
                if (!typeof(TInterface).IsInterface)
                    throw new InvalidOperationException("The proxy type must be an interface, not a class.");

                // get proxy type
                Type targetType = instance.GetType();
                string proxyTypeName = $"StardewModdingAPI.Proxies.From<{sourceModID}_{typeof(TInterface).FullName}>_To<{targetModID}_{targetType.FullName}>";
                if (!this.Builders.TryGetValue(proxyTypeName, out OriginalInterfaceProxyBuilder? builder))
                {
                    builder = new OriginalInterfaceProxyBuilder(proxyTypeName, this.ModuleBuilder, typeof(TInterface), targetType);
                    this.Builders[proxyTypeName] = builder;
                }

                // create instance
                return (TInterface)builder.CreateInstance(instance);
            }
        }
    }
}
