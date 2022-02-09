namespace StardewModdingAPI.Framework.Reflection
{
    public sealed class InterfaceProxyGlue
    {
        private readonly InterfaceProxyFactory Factory;

        internal InterfaceProxyGlue(InterfaceProxyFactory factory)
        {
            this.Factory = factory;
        }

        public object CreateInstanceForProxyTypeName(string proxyTypeName, object toProxy)
        {
            var builder = this.Factory.GetBuilderByProxyTypeName(proxyTypeName);
            return builder.CreateInstance(toProxy, this.Factory);
        }
    }
}
