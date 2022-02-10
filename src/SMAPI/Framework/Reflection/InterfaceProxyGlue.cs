namespace StardewModdingAPI.Framework.Reflection
{
    /// <summary>Provides an interface for proxied types to create other proxied types.</summary>
    public sealed class InterfaceProxyGlue
    {
        private readonly InterfaceProxyFactory Factory;

        internal InterfaceProxyGlue(InterfaceProxyFactory factory)
        {
            this.Factory = factory;
        }

        /// <summary>Get an existing or create a new proxied instance by its type name.</summary>
        /// <param name="proxyTypeName">The full name of the proxy type.</param>
        /// <param name="toProxy">The target instance to proxy.</param>
        public object ObtainInstanceForProxyTypeName(string proxyTypeName, object toProxy)
        {
            var builder = this.Factory.GetBuilderByProxyTypeName(proxyTypeName);
            return builder.ObtainInstance(toProxy, this.Factory);
        }
    }
}
