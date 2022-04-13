using System.Reflection;
using System.Reflection.Emit;
using Nanoray.Pintail;

namespace StardewModdingAPI.Framework.Reflection
{
    /// <summary>Generates proxy classes to access mod APIs through an arbitrary interface.</summary>
    internal class InterfaceProxyFactory
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying proxy type builder.</summary>
        private readonly IProxyManager<string> ProxyManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public InterfaceProxyFactory()
        {
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"StardewModdingAPI.Proxies, Version={this.GetType().Assembly.GetName().Version}, Culture=neutral"), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("StardewModdingAPI.Proxies");
            this.ProxyManager = new ProxyManager<string>(moduleBuilder, new ProxyManagerConfiguration<string>(
                proxyPrepareBehavior: ProxyManagerProxyPrepareBehavior.Eager,
                proxyObjectInterfaceMarking: ProxyObjectInterfaceMarking.Disabled
            ));
        }

        /// <summary>Create an API proxy.</summary>
        /// <typeparam name="TInterface">The interface through which to access the API.</typeparam>
        /// <param name="instance">The API instance to access.</param>
        /// <param name="sourceModID">The unique ID of the mod consuming the API.</param>
        /// <param name="targetModID">The unique ID of the mod providing the API.</param>
        public TInterface CreateProxy<TInterface>(object instance, string sourceModID, string targetModID)
            where TInterface : class
        {
            return this.ProxyManager.ObtainProxy<string, TInterface>(instance, targetContext: targetModID, proxyContext: sourceModID);
        }
    }
}
